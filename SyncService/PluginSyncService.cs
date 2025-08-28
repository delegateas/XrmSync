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
    XrmSyncConfiguration configuration,
    ILogger log) : ISyncService
{
    private readonly PluginSyncOptions options = configuration.Plugin?.Sync
        ?? throw new XrmSyncException("Plugin sync options are not configured");

    public async Task Sync(CancellationToken cancellationToken)
    {
        log.LogInformation("{header}", description.ToolHeader);

        if (options.DryRun)
        {
            log.LogInformation("***** DRY RUN *****");
            log.LogInformation("No changes will be made to Dataverse.");
        }

        log.LogInformation("Comparing plugins registered in Dataverse versus those found in your local code");
        log.LogInformation("Connecting to Dataverse at {dataverseUrl}", solutionReader.ConnectedHost);

        // Read the data from the local assembly and from Dataverse
        var (localAssembly, crmAssembly) = await ReadData(cancellationToken);

        // Align the local and remote info, matching IDs
        AlignPluginsIds(localAssembly, crmAssembly);
        AlignCustomApiIds(localAssembly, crmAssembly);

        // Calculate the differences
        var differences = differenceUtility.CalculateDifferences(localAssembly, crmAssembly);

        // Delete
        DoDeletes(differences);

        // Update the actual assembly file in Dataverse
        crmAssembly = UpsertAssembly(localAssembly, crmAssembly);

        // Update
        DoUpdates(differences);

        // Create
        DoCreates(differences, crmAssembly);

        // Done
        log.LogInformation("Plugin synchronization was completed successfully");
    }

    private static void AlignPluginsIds(AssemblyInfo localAssembly, AssemblyInfo? crmAssembly)
    {
        crmAssembly?.Plugins.ForEach(crmPlugin =>
        {
            var localPlugin = localAssembly.Plugins.SingleOrDefault(x => x.Name == crmPlugin.Name);
            if (localPlugin is null) return;

            localPlugin.Id = crmPlugin.Id;

            // Transfer IDs for images
            crmPlugin.PluginSteps.ForEach(crmStep =>
            {
                var localStep = localPlugin.PluginSteps.SingleOrDefault(x => x.Name == crmStep.Name);
                if (localStep is null) return;
                
                localStep.Id = crmStep.Id;

                crmStep.PluginImages.ForEach(crmImage =>
                {
                    var localImage = localStep.PluginImages.SingleOrDefault(x => x.Name == crmImage.Name);
                    if (localImage is null) return;

                    localImage.Id = crmImage.Id;
                });
            });
        });
    }

    private static void AlignCustomApiIds(AssemblyInfo localAssembly, AssemblyInfo? crmAssembly)
    {
        crmAssembly?.CustomApis.ForEach(crmCustomApi =>
        {
            var localCustomApi = localAssembly.CustomApis.SingleOrDefault(x => x.Name == crmCustomApi.Name);
            if (localCustomApi is null) return;

            localCustomApi.Id = crmCustomApi.Id;

            // Transfer IDs for request parameters
            crmCustomApi.RequestParameters.ForEach(crmParameter =>
            {
                var localParameter = localCustomApi.RequestParameters.SingleOrDefault(x => x.Name == crmParameter.Name);
                if (localParameter is null) return;

                localParameter.Id = crmParameter.Id;
            });

            // Transfer IDs for response properties
            crmCustomApi.ResponseProperties.ForEach(crmProperty =>
            {
                var localProperty = localCustomApi.ResponseProperties.SingleOrDefault(x => x.Name == crmProperty.Name);
                if (localProperty is null) return;

                localProperty.Id = crmProperty.Id;
            });
        });
    }

    private async Task<SyncData> ReadData(CancellationToken cancellationToken)
    {
        try
        {
            log.LogInformation("Reading solution information for solution \"{solutionName}\"", options.SolutionName);
            var (solutionId, solutionPrefix) = solutionReader.RetrieveSolution(options.SolutionName);

            log.LogInformation("Loading local assembly and its plugins");
            var localAssembly = await assemblyReader.ReadAssemblyAsync(options.AssemblyPath, solutionPrefix, cancellationToken);
            log.LogInformation("Identified {pluginCount} plugins and {customApiCount} custom apis locally", localAssembly.Plugins.Count, localAssembly.CustomApis.Count);

            log.LogInformation("Validating plugins and custom apis to be registered");
            pluginValidator.Validate(localAssembly.Plugins);
            pluginValidator.Validate(localAssembly.CustomApis);
            log.LogInformation("Plugins and custom apis validated");

            log.LogInformation("Retrieving registered plugins from Dataverse solution \"{solutionName}\"", options.SolutionName);
            var crmAssembly = GetPluginAssembly(solutionId, localAssembly.Name);
            log.LogInformation("Identified {pluginCount} plugins and {customApiCount} custom apis registered in CRM", crmAssembly?.Plugins.Count ?? 0, crmAssembly?.CustomApis.Count ?? 0);

            return new SyncData(localAssembly, crmAssembly);
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

    internal AssemblyInfo? GetPluginAssembly(Guid solutionId, string assemblyName)
    {
        var assemblyInfo = pluginReader.GetPluginAssembly(solutionId, assemblyName);
        if (assemblyInfo == null)
        {
            log.LogInformation("Assembly {assemblyName} not found in CRM, creating new assembly", assemblyName);
            return null;
        }

        var pluginDefinitions = GetPluginTypes(solutionId, assemblyInfo.Id);

        return assemblyInfo with
        {
            Plugins = [.. pluginDefinitions.Where(p => p.PluginSteps.Count > 0)],
            CustomApis = customApiReader.GetCustomApis(solutionId),
        };
    }

    private List<PluginDefinition> GetPluginTypes(Guid solutionId, Guid assemblyId)
    {
        // TODO: Combine these into a single call like CustomAPI
        var pluginDefinitions = pluginReader.GetPluginTypes(assemblyId);
        var pluginSteps = pluginReader.GetPluginSteps(pluginDefinitions, solutionId);
        pluginSteps.ForEach(reference =>
        {
            var (step, plugin) = reference;
            plugin.PluginSteps.Add(step);
        });

        return pluginDefinitions;
    }

    private AssemblyInfo UpsertAssembly(AssemblyInfo localAssembly, AssemblyInfo? remoteAssembly)
    {
        if (remoteAssembly == null)
        {
            log.LogInformation("Creating assembly {assemblyName}", localAssembly.Name);
            remoteAssembly = CreatePluginAssembly(localAssembly);
        }
        else if (new Version(remoteAssembly.Version) < new Version(localAssembly.Version))
        {
            log.LogInformation("Registered assembly version {RemoteVersion} is lower than local assembly version {LocalVersion}, updating", remoteAssembly.Version, localAssembly.Version);
            UpdatePluginAssembly(remoteAssembly.Id, localAssembly);
        }
        else if (remoteAssembly.Hash != localAssembly.Hash)
        {
            log.LogInformation("Registered assembly hash does not match local assembly hash, updating");
            UpdatePluginAssembly(remoteAssembly.Id, localAssembly);
        }
        else
        {
            log.LogInformation("Assembly {assemblyName} already exists in CRM with matching version and hash, skipping update", remoteAssembly.Name);
        }

        return remoteAssembly;
    }

    internal AssemblyInfo CreatePluginAssembly(AssemblyInfo localAssembly)
    {
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
        if (localAssembly.DllPath is null) throw new XrmSyncException("Assembly DLL path is null. Ensure the assembly has been read correctly.");
        pluginWriter.UpdatePluginAssembly(assemblyId, localAssembly.Name, localAssembly.DllPath, localAssembly.Hash, localAssembly.Version, description.SyncDescription);
    }

    internal void DoCreates(Differences differences, AssemblyInfo dataverseAssembly)
    {
        pluginWriter.CreatePluginTypes(differences.Types.Creates.ConvertAll(c => c.Local), dataverseAssembly.Id, description.SyncDescription);
        pluginWriter.CreatePluginSteps(differences.PluginSteps.Creates.ConvertAll(c => c.Local), description.SyncDescription);
        pluginWriter.CreatePluginImages(differences.PluginImages.Creates.ConvertAll(c => c.Local));
        customApiWriter.CreateCustomApis(differences.CustomApis.Creates.ConvertAll(c => c.Local), description.SyncDescription);
        customApiWriter.CreateRequestParameters(differences.RequestParameters.Creates.ConvertAll(c => c.Local));
        customApiWriter.CreateResponseProperties(differences.ResponseProperties.Creates.ConvertAll(c => c.Local));
    }

    internal void DoUpdates(Differences differences)
    {
        pluginWriter.UpdatePluginSteps(differences.PluginSteps.Updates.ConvertAll(c => c.Local.Entity), description.SyncDescription);
        pluginWriter.UpdatePluginImages(differences.PluginImages.Updates.ConvertAll(c => c.Local));
        customApiWriter.UpdateCustomApis(differences.CustomApis.Updates.ConvertAll(c => c.Local), description.SyncDescription);
        customApiWriter.UpdateRequestParameters(differences.RequestParameters.Updates.ConvertAll(c => c.Local.Entity));
        customApiWriter.UpdateResponseProperties(differences.ResponseProperties.Updates.ConvertAll(c => c.Local.Entity));
    }

    internal void DoDeletes(Differences differences)
    {
        // Delete in the correct order: images first, then steps, then custom api components, finally types
        pluginWriter.DeletePluginImages(differences.PluginImages.Deletes.ConvertAll(d => d.Entity));
        pluginWriter.DeletePluginSteps(differences.PluginSteps.Deletes.ConvertAll(d => d.Entity));

        customApiWriter.DeleteCustomApiRequestParameters(differences.RequestParameters.Deletes.ConvertAll(d => d.Entity));
        customApiWriter.DeleteCustomApiResponseProperties(differences.ResponseProperties.Deletes.ConvertAll(d => d.Entity));
        customApiWriter.DeleteCustomApiDefinitions(differences.CustomApis.Deletes);

        pluginWriter.DeletePluginTypes(differences.Types.Deletes);
    }
}

internal record SyncData(AssemblyInfo LocalAssembly, AssemblyInfo? CrmAssembly);