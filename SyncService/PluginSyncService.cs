using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using XrmSync.AssemblyAnalyzer;
using XrmSync.AssemblyAnalyzer.AssemblyReader;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Difference;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.Extensions;
using XrmSync.SyncService.PluginValidator;

[assembly: InternalsVisibleTo("Tests")]
namespace XrmSync.SyncService;

public class PluginSyncService(
    IPluginReader pluginReader,
    IPluginWriter pluginWriter,
    IPluginValidator pluginValidator,
    ICustomApiReader customApiReader,
    ICustomApiWriter customApiWriter,
    IAssemblyReader assemblyReader,
    ISolutionReader solutionReader,
    IDifferenceUtility differenceUtility,
    Description description,
    XrmSyncOptions options,
    ILogger log) : ISyncService
{
    public async Task Sync()
    {
        log.LogInformation("Comparing plugins registered in Dataverse versus those found in your local code");

        // Read the data from the local assembly and from Dataverse
        var (localAssembly, crmAssembly, localPluginTypes, crmPluginTypes, prefix) = await ReadData();

        // Align the local and remote info, matching IDs
        var (localPluginSteps, crmPluginSteps) = AlignSteps(localAssembly, crmAssembly);
        var (localPluginImages, crmPluginImages) = AlignImages(localPluginSteps, crmPluginSteps);
        var (localCustomApis, crmCustomApis) = AlignCustomApis(localAssembly, crmAssembly);
        var (localRequestParameters, crmRequestParameters) = AlignRequestParameters(localAssembly, crmAssembly);
        var (localResponseProperties, crmResponseProperties) = AlignResponseProperties(localAssembly, crmAssembly);

        var localData = new CompiledData(
            localPluginTypes,
            localPluginSteps,
            localPluginImages,
            localCustomApis,
            localRequestParameters,
            localResponseProperties
        );

        var crmData = new CompiledData(
            crmPluginTypes,
            crmPluginSteps,
            crmPluginImages,
            crmCustomApis,
            crmRequestParameters,
            crmResponseProperties
        );

        // Calculate the differences
        var differences = differenceUtility.CalculateDifferences(localData, crmData);

        // Delete
        DoDeletes(differences);

        // Update the actual assembly file in Dataverse
        crmAssembly = UpsertAssembly(localAssembly, crmAssembly);

        // Update
        DoUpdates(differences, crmPluginTypes, crmPluginSteps);

        // Create
        DoCreates(differences, crmPluginTypes, crmPluginSteps, crmAssembly, prefix);

        // Done
        log.LogInformation("Plugin synchronization was completed successfully");
    }

    private static (List<ApiDefinition> localCustomApis, List<ApiDefinition> crmCustomApis) AlignCustomApis(AssemblyInfo localAssembly, AssemblyInfo? crmAssembly)
    {
        crmAssembly?.CustomApis.TransferIdsTo(localAssembly.CustomApis, x => x.Name);

        return (localAssembly.CustomApis, crmAssembly?.CustomApis ?? []);
    }

    private static (List<ResponseProperty> localProperties, List<ResponseProperty> crmProperties) AlignResponseProperties(AssemblyInfo localAssembly, AssemblyInfo? crmAssembly)
    {
        var localCustomApiProperties = localAssembly.CustomApis.SelectMany(x => x.ResponseProperties).ToList();
        var crmCustomApiProperties = crmAssembly?.CustomApis.SelectMany(x => x.ResponseProperties).ToList() ?? [];
        crmCustomApiProperties.TransferIdsTo(localCustomApiProperties, x => x.Name);

        return (localCustomApiProperties, crmCustomApiProperties);
    }

    private static (List<RequestParameter> localRequests, List<RequestParameter> crmRequests) AlignRequestParameters(AssemblyInfo localAssembly, AssemblyInfo? crmAssembly)
    {
        var localCustomApiRequests = localAssembly.CustomApis.SelectMany(x => x.RequestParameters).ToList();
        var crmCustomApiRequests = crmAssembly?.CustomApis.SelectMany(x => x.RequestParameters).ToList() ?? [];
        crmCustomApiRequests.TransferIdsTo(localCustomApiRequests, x => x.Name);

        return (localCustomApiRequests,  crmCustomApiRequests);
    }

    private static (List<Image> localPluginImages, List<Image> crmPluginImages) AlignImages(List<Step> localPluginSteps, List<Step> crmPluginSteps)
    {
        var localPluginImages = localPluginSteps.SelectMany(x => x.PluginImages).ToList();
        var crmPluginImages = crmPluginSteps.SelectMany(x => x.PluginImages).ToList();
        crmPluginImages.TransferIdsTo(localPluginImages, x => $"[{x.Name}] {x.PluginStepName}");

        return (localPluginImages, crmPluginImages);
    }

    private static (List<Step> localSteps, List<Step> crmSteps) AlignSteps(AssemblyInfo localAssembly, AssemblyInfo? crmAssembly)
    {
        var crmPluginSteps = crmAssembly?.Plugins.SelectMany(x => x.PluginSteps).ToList() ?? [];
        var localPluginSteps = localAssembly.Plugins.SelectMany(x => x.PluginSteps).ToList();

        crmPluginSteps.TransferIdsTo(localPluginSteps, x => x.Name);

        return (localPluginSteps, crmPluginSteps);
    }

    private async Task<SyncData> ReadData()
    {
        try
        {
            log.LogInformation("Loading local assembly and its plugins");
            var localAssembly = await assemblyReader.ReadAssemblyAsync(options.AssemblyPath);
            log.LogInformation("Identified {pluginCount} plugins and {customApiCount} custom apis locally", localAssembly.Plugins.Count, localAssembly.CustomApis.Count);

            log.LogInformation("Validating plugins to be registered");
            pluginValidator.Validate(localAssembly.Plugins);
            log.LogInformation("Plugins validated");

            log.LogInformation("Retrieving registered plugins from Dataverse solution \"{solutionName}\"", options.SolutionName);
            var (solutionId, solutionPrefix) = solutionReader.RetrieveSolution(options.SolutionName);
            var (crmAssembly, crmPluginTypes) = GetPluginAssembly(solutionId, localAssembly.Name);
            log.LogInformation("Identified {pluginCount} plugins and {customApiCount} custom apis registered in CRM", crmAssembly?.Plugins.Count ?? 0, crmAssembly?.CustomApis.Count ?? 0);

            // Identify the associated local plugin types
            var localPluginTypes = localAssembly.CustomApis
                .ConvertAll(localApi => localApi.ToPluginType(crmPluginTypes, c => c.PluginTypeName))
                .Concat(localAssembly.Plugins.ConvertAll(localPlugin => localPlugin.ToPluginType(crmPluginTypes, c => c.Name)))
                .ToList();

            return new SyncData(localAssembly, crmAssembly, localPluginTypes, crmPluginTypes, solutionPrefix);
        }
        catch (AnalysisException ex)
        {
            log.LogCritical(ex, "Failed to analyze local assembly. Ensure the assembly is valid and contains plugins.");
            throw new XrmSyncException("Failed to analyse local assembly", ex);
        }
        catch (ValidationException ex)
        {
            log.LogError(ex, "Validation failed for the plugins in the assembly. Ensure the plugins are valid and compatible with Dataverse.");
            throw new XrmSyncException("Validation failed for the plugins in the assembly", ex);
        }
        catch (AggregateException ex)
        {
            log.LogError("An error occurred while reading the assembly or plugins. Ensure the assembly is valid and contains plugins.");
            foreach (var inner in ex.InnerExceptions)
            {
                if (inner is AnalysisException analysisEx)
                {
                    log.LogError("Analysis error: {Message}", analysisEx.Message);
                }
                else if (inner is ValidationException validationEx)
                {
                    log.LogError("Validation error: {message}", validationEx.Message);
                }
                else
                {
                    log.LogCritical(inner, "Unexpected error:");
                }
            }

            throw new XrmSyncException("Failed to read data", ex);
        }
    }

    internal (AssemblyInfo? assemblyInfo, List<PluginType> pluginTypes) GetPluginAssembly(Guid solutionId, string assemblyName)
    {
        var assemblyInfo = pluginReader.GetPluginAssembly(solutionId, assemblyName);
        if (assemblyInfo == null)
        {
            log.LogInformation("Assembly {assemblyName} not found in CRM, creating new assembly", assemblyName);
            return (null, []);
        }

        var pluginDefinitions = GetPluginTypes(solutionId, assemblyInfo.Id);

        assemblyInfo = assemblyInfo with {
            Plugins = [.. pluginDefinitions.Where(p => p.PluginSteps.Count > 0)],
            CustomApis = customApiReader.GetCustomApis(solutionId),
        };

        return (assemblyInfo, pluginDefinitions.ConvertAll(p => new PluginType { Id = p.Id, Name = p.Name }));
    }

    private List<PluginDefinition> GetPluginTypes(Guid solutionId, Guid assemblyId)
    {
        var pluginAssemblyTypes = pluginReader.GetPluginTypes(assemblyId);

        var typeIds = pluginAssemblyTypes.ConvertAll(t => t.Id);
        var pluginStepsLookup = pluginReader.GetPluginSteps(solutionId, typeIds);

        return pluginAssemblyTypes
            .ConvertAll(type => new PluginDefinition
            {
                Id = type.Id,
                Name = type.Name,
                PluginSteps = [.. pluginStepsLookup[type.Id]]
            });
    }

    private AssemblyInfo UpsertAssembly(AssemblyInfo localAssembly, AssemblyInfo? remoteAssembly)
    {
        if (remoteAssembly == null)
        {
            remoteAssembly = CreatePluginAssembly(localAssembly);
        } else if (new Version(remoteAssembly.Version) < new Version(localAssembly.Version))
        {
            log.LogDebug("Registered assembly version {RemoteVersion} is lower than local assembly version {LocalVersion}, updating", remoteAssembly.Version, localAssembly.Version);
            UpdatePluginAssembly(remoteAssembly.Id, localAssembly);
        }
        else if (remoteAssembly.Hash != localAssembly.Hash)
        {
            log.LogDebug("Registered assembly hash does not match local assembly hash, updating");
            UpdatePluginAssembly(remoteAssembly.Id, localAssembly);
        }

        return remoteAssembly;
    }

    internal AssemblyInfo CreatePluginAssembly(AssemblyInfo localAssembly)
    {
        log.LogInformation("Creating assembly {assemblyName}", localAssembly.Name);
        if (localAssembly.DllPath is null) throw new XrmSyncException("Assembly DLL path is null. Ensure the assembly has been read correctly.");
        var assemblyId = pluginWriter.CreatePluginAssembly(localAssembly.Name, localAssembly.DllPath, localAssembly.Hash, localAssembly.Version, description.SyncDescription);
        return localAssembly with {
            Id = assemblyId,
            Plugins = [],
            CustomApis = []
        };
    }

    internal void UpdatePluginAssembly(Guid assemblyId, AssemblyInfo localAssembly)
    {
        log.LogInformation($"Updating assembly {localAssembly.Name}");
        if (localAssembly.DllPath is null) throw new XrmSyncException("Assembly DLL path is null. Ensure the assembly has been read correctly.");
        pluginWriter.UpdatePluginAssembly(assemblyId, localAssembly.Name, localAssembly.DllPath, localAssembly.Hash, localAssembly.Version, description.SyncDescription);
    }

    internal void DoCreates(Differences differences, List<PluginType> dataversePluginTypes, List<Step> dataversePluginSteps, AssemblyInfo dataverseAssembly, string prefix)
    {
        dataversePluginTypes.AddRange(pluginWriter.CreatePluginTypes(differences.Types.Creates, dataverseAssembly.Id, description.SyncDescription));
        dataversePluginSteps.AddRange(pluginWriter.CreatePluginSteps(differences.PluginSteps.Creates, dataversePluginTypes, description.SyncDescription));
        pluginWriter.CreatePluginImages(differences.PluginImages.Creates, dataversePluginSteps);
        dataverseAssembly.CustomApis.AddRange(customApiWriter.CreateCustomApis(differences.CustomApis.Creates, dataversePluginTypes, prefix, description.SyncDescription));
        customApiWriter.CreateRequestParameters(differences.RequestParameters.Creates, dataverseAssembly.CustomApis);
        customApiWriter.CreateResponseProperties(differences.ResponseProperties.Creates, dataverseAssembly.CustomApis);
    }

    internal void DoUpdates(Differences differences, List<PluginType> dataverseTypes, List<Step> dataversePluginSteps)
    {
        pluginWriter.UpdatePluginSteps(differences.PluginSteps.Updates, description.SyncDescription);
        pluginWriter.UpdatePluginImages(differences.PluginImages.Updates, dataversePluginSteps);
        customApiWriter.UpdateCustomApis(differences.CustomApis.Updates, dataverseTypes, description.SyncDescription);
        customApiWriter.UpdateRequestParameters(differences.RequestParameters.Updates);
        customApiWriter.UpdateResponseProperties(differences.ResponseProperties.Updates);
    }

    internal void DoDeletes(Differences differences)
    {
        // Delete in the correct order: images first, then steps, then custom api components, finally types
        pluginWriter.DeletePluginImages(differences.PluginImages.Deletes);
        pluginWriter.DeletePluginSteps(differences.PluginSteps.Deletes);

        customApiWriter.DeleteCustomApiRequestParameters(differences.RequestParameters.Deletes);
        customApiWriter.DeleteCustomApiResponseProperties(differences.ResponseProperties.Deletes);
        customApiWriter.DeleteCustomApiDefinitions(differences.CustomApis.Deletes);

        pluginWriter.DeletePluginTypes(differences.Types.Deletes);
    }
}

internal record SyncData(AssemblyInfo LocalAssembly, AssemblyInfo? CrmAssembly, List<PluginType> LocalPluginTypes, List<PluginType> CrmPluginTypes, string Prefix);