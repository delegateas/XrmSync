using DG.XrmPluginSync.Dataverse.Interfaces;
using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.Model.CustomApi;
using DG.XrmPluginSync.Model.Plugin;
using DG.XrmPluginSync.SyncService.AssemblyReader;
using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.SyncService.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace DG.XrmPluginSync.SyncService;

public class PluginSyncService(
    IPluginReader pluginReader,
    IPluginWriter pluginWriter,
    ICustomApiReader customApiReader,
    ICustomApiWriter customApiWriter,
    IAssemblyReader assemblyReader,
    ISolutionReader solutionReader,
    IDifferenceUtility differenceUtility,
    Description description,
    XrmPluginSyncOptions options,
    ILogger log) : ISyncService
{
    public async Task Sync()
    {
        log.LogInformation("Comparing plugins registered in Dataverse versus those found in your local code");

        // Read the data from the local assembly and from Dataverse
        var (localAssembly, crmAssembly, localPluginTypes, crmPluginTypes) = await ReadData();

        // Update the actual assembly file in Dataverse
        crmAssembly = UpsertAssembly(localAssembly, crmAssembly);

        // Align the local and remote info, matching IDs
        var (localPluginSteps, crmPluginSteps) = AlignSteps(localAssembly, crmAssembly);
        var (localPluginImages, crmPluginImages) = AlignImages(localPluginSteps, crmPluginSteps);
        var localCustomApis = AlignCustomApis(localAssembly, crmAssembly);
        var (localRequestParameters, crmRequestParameters) = AlignRequestParameters(localAssembly, crmAssembly);
        var (localResponseProperties, crmResponseProperties) = AlignResponseProperties(localAssembly, crmAssembly);

        var localData = new CompiledData(localPluginTypes, localPluginSteps, localPluginImages, localCustomApis, localRequestParameters, localResponseProperties);
        var crmData = new CompiledData(crmPluginTypes, crmPluginSteps, crmPluginImages, crmAssembly.CustomApis, crmRequestParameters, crmResponseProperties);

        // Calculate the differences
        var differences = differenceUtility.CalculateDifferences(localData, crmData);

        // Delete
        var deleteData = new CompiledData(differences.Types.Deletes, differences.PluginSteps.Deletes, differences.PluginImages.Deletes, differences.CustomApis.Deletes, differences.RequestParameters.Deletes, differences.ResponseProperties.Deletes);
        DeletePlugins(deleteData);

        // Update
        var updateData = new CompiledData(differences.Types.Updates, differences.PluginSteps.Updates, differences.PluginImages.Updates, differences.CustomApis.Updates, differences.RequestParameters.Updates, differences.ResponseProperties.Updates);
        UpdatePlugins(updateData);

        // Create
        crmPluginTypes.AddRange(CreateTypes(crmAssembly, differences.Types.Creates));
        crmPluginSteps.AddRange(CreateSteps(differences.PluginSteps.Creates, crmPluginTypes));
        CreateImages(differences.PluginImages.Creates, crmPluginSteps);

        // Done
        log.LogInformation("Plugin synchronization was completed successfully");
    }

    private static List<ApiDefinition> AlignCustomApis(AssemblyInfo localAssembly, AssemblyInfo crmAssembly)
    {
        crmAssembly.CustomApis.TransferIdsTo(localAssembly.CustomApis, x => x.Name);
        return localAssembly.CustomApis;
    }

    private static (List<ResponseProperty> localProperties, List<ResponseProperty> crmProperties) AlignResponseProperties(AssemblyInfo localAssembly, AssemblyInfo crmAssembly)
    {
        var localCustomApiProperties = localAssembly.CustomApis.SelectMany(x => x.ResponseProperties).ToList();
        var crmCustomApiProperties = crmAssembly.CustomApis.SelectMany(x => x.ResponseProperties).ToList();
        crmCustomApiProperties.TransferIdsTo(localCustomApiProperties, x => x.Name);

        return (localCustomApiProperties, crmCustomApiProperties);
    }

    private static (List<RequestParameter> localRequests, List<RequestParameter> crmRequests) AlignRequestParameters(AssemblyInfo localAssembly, AssemblyInfo crmAssembly)
    {
        var localCustomApiRequests = localAssembly.CustomApis.SelectMany(x => x.RequestParameters).ToList();
        var crmCustomApiRequests = crmAssembly.CustomApis.SelectMany(x => x.RequestParameters).ToList();
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

    private static (List<Step> localSteps, List<Step> crmSteps) AlignSteps(AssemblyInfo localAssembly, AssemblyInfo crmAssembly)
    {
        var crmPluginSteps = crmAssembly.Plugins.SelectMany(x => x.PluginSteps).ToList();
        var localPluginSteps = localAssembly.Plugins.SelectMany(x => x.PluginSteps).ToList();

        crmPluginSteps.TransferIdsTo(localPluginSteps, x => x.Name);

        return (localPluginSteps, crmPluginSteps);
    }

    private async Task<(AssemblyInfo localAssembly, AssemblyInfo? crmAssembly, List<PluginType> localPluginTypes, List<PluginType> crmPluginTypes)> ReadData()
    {
        log.LogInformation("Loading local assembly and its plugins");
        var localAssembly = await assemblyReader.ReadAssemblyAsync(options.AssemblyPath);
        log.LogInformation("Identified {pluginCount} plugins and {customApiCount} custom apis locally", localAssembly.Plugins.Count, localAssembly.CustomApis.Count);

        log.LogInformation("Validating plugins to be registered");
        ValidatePlugins(localAssembly.Plugins);
        log.LogInformation("Plugins validated");

        log.LogInformation("Retrieving registered plugins from Dataverse");
        var solutionId = solutionReader.GetSolutionId(options.SolutionName);
        var (crmAssembly, crmPluginTypes) = GetPluginAssembly(solutionId, localAssembly.Name);
        log.LogInformation("Identified {pluginCount} plugins and {customApiCount} custom apis registered in CRM", crmAssembly?.Plugins.Count ?? 0, crmAssembly?.CustomApis.Count ?? 0);

        // Identify the associated local plugin types
        var localPluginTypes = localAssembly.CustomApis
            .ConvertAll(localApi => localApi.ToPluginType(crmPluginTypes, c => c.PluginTypeName))
            .Concat(localAssembly.Plugins.ConvertAll(localPlugin => localPlugin.ToPluginType(crmPluginTypes, c => c.Name)))
            .ToList();

        return (localAssembly, crmAssembly, localPluginTypes, crmPluginTypes);
    }

    internal (AssemblyInfo? assemblyInfo, List<PluginType> pluginTypes) GetPluginAssembly(Guid solutionId, string assemblyName)
    {
        var assemblyEntity = pluginReader.GetPluginAssembly(solutionId, assemblyName);
        if (assemblyEntity == null) return (null, []);

        var pluginDefinitions = GetPluginTypes(solutionId, assemblyEntity.Id);

        var assemblyInfo = new AssemblyInfo()
        {
            Id = assemblyEntity.Id,
            Name = assemblyEntity.GetAttributeValue<string>("name"),
            Version = assemblyEntity.GetAttributeValue<string>("version"),
            Hash = assemblyEntity.GetAttributeValue<string>("sourcehash"),
            Plugins = [.. pluginDefinitions.Where(p => p.PluginSteps.Count > 0)],
            CustomApis = GetCustomApis(pluginDefinitions, solutionId),
            DllPath = string.Empty,
        };

        return (assemblyInfo, pluginDefinitions.ConvertAll(p => new PluginType { Id = p.Id, Name = p.Name }));
    }

    private List<PluginDefinition> GetPluginTypes(Guid solutionId, Guid assemblyId)
    {
        var pluginAssemblyTypes = pluginReader.GetPluginTypes(assemblyId);

        return pluginAssemblyTypes
            .ConvertAll(type =>
            {
                var pluginSteps = pluginReader.GetPluginSteps(solutionId, type.Id);
                var pluginStepDtos = pluginSteps.ConvertAll(step =>
                {
                    var pluginImages = GetPluginImages(step);

                    return new Step
                    {
                        Id = step.Id,
                        LogicalName = string.Empty, // TODO step.GetAttributeValue<string>(),
                        EventOperation = string.Empty, // TODO step.GetAttributeValue<string>("eventoperation"),
                        ExecutionStage = step.GetAttributeValue<OptionSetValue>("stage").Value,
                        Deployment = step.GetAttributeValue<OptionSetValue>("supporteddeployment").Value,
                        ExecutionMode = step.GetAttributeValue<OptionSetValue>("mode").Value,
                        ExecutionOrder = step.GetAttributeValue<int>("rank"),
                        FilteredAttributes = step.GetAttributeValue<string>("filteringattributes"),
                        UserContext = step.GetAttributeValue<EntityReference>("impersonatinguserid")?.Id ?? Guid.Empty,
                        PluginTypeName = type.GetAttributeValue<string>("name"),
                        Name = step.GetAttributeValue<string>("name"),
                        PluginImages = pluginImages
                    };
                });

                return new PluginDefinition
                {
                    Id = type.Id,
                    Name = type.GetAttributeValue<string>("name"),
                    PluginSteps = pluginStepDtos
                };
            });
    }

    private List<ApiDefinition> GetCustomApis(List<PluginDefinition> pluginDefinitions, Guid solutionId)
    {
        var customApis = customApiReader.GetCustomApis(solutionId);

        return [.. customApis.Select(api =>
        {
            var name = api.GetAttributeValue<string>("name");
            var pluginTypeId = api.GetAttributeValue<EntityReference>("plugintypeid")?.Id ?? Guid.Empty;

            if (pluginTypeId == default)
            {
                return null;
            }

            var pluginDefinition = pluginDefinitions.FirstOrDefault(p => p.Id == pluginTypeId);
            if (pluginDefinition is null) {
                return null;
            }
            
            var requestParameters =
                customApiReader.GetCustomApiRequestParameters(api.Id)
                .ConvertAll(p => new RequestParameter
                {
                    Id = p.Id,
                    CustomApiName = name,
                    DisplayName = p.GetAttributeValue<string>("displayname"),
                    UniqueName = p.GetAttributeValue<string>("uniquename"),
                    Name = p.GetAttributeValue<string>("name"),
                    IsCustomizable = p.GetAttributeValue<BooleanManagedProperty>("iscustomizable").Value,
                    LogicalEntityName = p.GetAttributeValue<string>("logicalentityname"),
                    IsOptional = p.GetAttributeValue<bool>("isoptional"),
                    Type = p.GetAttributeValue<OptionSetValue>("type").Value
                });

            var responseProperties =
                customApiReader.GetCustomApiResponseProperties(api.Id)
                .ConvertAll(p => new ResponseProperty
                {
                    Id = p.Id,
                    CustomApiName = name,
                    DisplayName = p.GetAttributeValue<string>("displayname"),
                    UniqueName = p.GetAttributeValue<string>("uniquename"),
                    Name = p.GetAttributeValue<string>("name"),
                    IsCustomizable = p.GetAttributeValue<BooleanManagedProperty>("iscustomizable").Value,
                    LogicalEntityName = p.GetAttributeValue<string>("logicalentityname"),
                    Type = p.GetAttributeValue<OptionSetValue>("type").Value
                });

            return new ApiDefinition
            {
                Id = api.Id,
                Name = name,
                PluginTypeName = pluginDefinition.Name,
                UniqueName = api.GetAttributeValue<string>("uniquename"),
                DisplayName = api.GetAttributeValue<string>("displayname"),
                Description = api.GetAttributeValue<string>("description"),
                IsFunction = api.GetAttributeValue<bool>("isfunction"),
                EnabledForWorkflow = api.GetAttributeValue<bool>("workflowssdkstepenabled"),
                BindingType = api.GetAttributeValue<OptionSetValue>("bindingtype").Value,
                BoundEntityLogicalName = api.GetAttributeValue<string>("boundentitylogicalname"),
                AllowedCustomProcessingStepType = api.GetAttributeValue<OptionSetValue>("allowedcustomprocessingsteptype").Value,
                OwnerId = api.GetAttributeValue<EntityReference>("owninguser").Id,
                IsCustomizable = api.GetAttributeValue<BooleanManagedProperty>("iscustomizable").Value,
                IsPrivate = api.GetAttributeValue<bool>("isprivate"),
                ExecutePrivilegeName = api.GetAttributeValue<string>("executeprivilegename"),

                RequestParameters = requestParameters,
                ResponseProperties = responseProperties
            };
        })
        .Where(p => p != null).Select(p => p!)];
    }

    private List<Image> GetPluginImages(Entity step)
    {
        return pluginReader.GetPluginImages(step.Id)
                            .ConvertAll(image => new Image
                            {
                                Id = image.Id,
                                PluginStepName = step.GetAttributeValue<string>("name"),
                                Name = image.GetAttributeValue<string>("name"),
                                EntityAlias = image.GetAttributeValue<string>("entityalias"),
                                ImageType = image.GetAttributeValue<OptionSetValue>("imagetype").Value,
                                Attributes = image.GetAttributeValue<string>("attributes"),
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
        => CreatePluginAssembly(localAssembly, options.SolutionName);

    internal AssemblyInfo CreatePluginAssembly(AssemblyInfo localAssembly, string solutionName)
    {
        log.LogInformation($"Creating assembly {localAssembly.Name}");
        var assemblyId = pluginWriter.CreatePluginAssembly(localAssembly.Name, solutionName, localAssembly.DllPath, localAssembly.Hash, localAssembly.Version, description.SyncDescription);
        return localAssembly with { Id = assemblyId };
    }

    internal void UpdatePluginAssembly(Guid assemblyId, AssemblyInfo localAssembly)
    {
        log.LogInformation($"Updating assembly {localAssembly.Name}");
        pluginWriter.UpdatePluginAssembly(assemblyId, localAssembly.Name, localAssembly.DllPath, localAssembly.Hash, localAssembly.Version, description.SyncDescription);
    }

    internal List<PluginType> CreateTypes(AssemblyInfo crmAssembly, List<PluginType> types)
    {
        return pluginWriter.CreatePluginTypes(types, crmAssembly.Id, description.SyncDescription);
    }

    internal List<Step> CreateSteps(List<Step> pluginSteps, List<PluginType> types)
    {
        return pluginWriter.CreatePluginSteps(pluginSteps, types, options.SolutionName, description.SyncDescription);
    }

    internal List<Image> CreateImages(List<Image> pluginImages, List<Step> crmPluginSteps)
    {
        return pluginWriter.CreatePluginImages(pluginImages, crmPluginSteps);
    }

    internal void DeletePlugins(CompiledData data)
    {
        pluginWriter.DeletePlugins(data.Types, data.Steps, data.Images, data.CustomApis, data.RequestParameters, data.ResponseProperties);
    }

    internal void UpdatePlugins(CompiledData data)
    {
        pluginWriter.UpdatePlugins(data.Steps, data.Images, description.SyncDescription);
        customApiWriter.UpdateCustomApis(data.CustomApis, description.SyncDescription);
        customApiWriter.UpdateRequestParameters(data.RequestParameters);
        customApiWriter.UpdateResponseProperties(data.ResponseProperties);
    }

    internal void ValidatePlugins(List<PluginDefinition> pluginTypes)
    {
        List<Exception> exceptions = [];
        var pluginSteps = pluginTypes.SelectMany(x => x.PluginSteps);
        var preOperationAsyncPlugins = pluginSteps
            .Where(x =>
            x.ExecutionMode == (int)ExecutionMode.Asynchronous &&
            x.ExecutionStage != (int)ExecutionStage.Post)
            .ToList();
        exceptions.AddRange(preOperationAsyncPlugins.Select(x => new Exception($"Plugin {x.Name}: Pre execution stages does not support asynchronous execution mode")));

        var preOperationWithPostImagesPlugins = pluginSteps
            .Where(x =>
            {
                var postImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PostImage);

                return
                (x.ExecutionStage == (int)ExecutionStage.Pre ||
                 x.ExecutionStage == (int)ExecutionStage.PreValidation) && postImages.Any();
            });
        exceptions.AddRange(preOperationWithPostImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Pre execution stages does not support post-images")));

        var associateDisassociateWithFilterPlugins = pluginSteps
            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
            .Where(x => x.FilteredAttributes != null);
        exceptions.AddRange(associateDisassociateWithFilterPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events can't have filtered attributes")));

        var associateDisassociateWithImagesPlugins = pluginSteps
            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
            .Where(x => x.PluginImages.Any());
        exceptions.AddRange(associateDisassociateWithImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events can't have images")));

        var associateDisassociateNotAllEntitiesPlugins = pluginSteps
            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
            .Where(x => x.LogicalName != "");
        exceptions.AddRange(associateDisassociateNotAllEntitiesPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events must target all entities")));

        var createWithPreImagesPlugins = pluginSteps
            .Where(x =>
            {
                var preImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PreImage);
                return x.EventOperation == "Create" && preImages.Any();
            });
        exceptions.AddRange(createWithPreImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Create events does not support pre-images")));

        var deleteWithPostImagesPLugins = pluginSteps
            .Where(x =>
            {
                var postImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PostImage);
                return x.EventOperation == "Delete" && postImages.Any();
            });
        exceptions.AddRange(deleteWithPostImagesPLugins.Select(x => new Exception($"Plugin {x.Name}: Delete events does not support post-images")));

        var userContextDoesNotExistPlugins = pluginReader.GetMissingUserContexts(pluginSteps);
        exceptions.AddRange(userContextDoesNotExistPlugins.Select(x => new Exception($"Plugin {x.Name}: Defined user context is not in the system")));

        if (exceptions.Count == 1) throw exceptions.First();
        else if (exceptions.Count > 1) throw new AggregateException("Some plugins can't be validated", exceptions);
    }
}
