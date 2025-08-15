using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse;

public class PluginWriter(IMessageReader messageReader, IDataverseWriter writer, ILogger log, XrmSyncOptions options) : IPluginWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", options.SolutionName }
    };

    public Guid CreatePluginAssembly(string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description)
    {
        var entity = new PluginAssembly
        {
            Name = pluginName,
            Content = GetBase64StringFromFile(dllPath),
            SourceHash = sourceHash,
            IsolationMode = PluginAssembly_IsolationMode.Sandbox,
            Version = assemblyVersion,
            Description = description
        };

        return writer.Create(entity, Parameters);
    }

    public void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description)
    {
        var entity = new PluginAssembly()
        {
            Id = assemblyId,
            Name = pluginName,
            Content = GetBase64StringFromFile(dllPath),
            SourceHash = sourceHash,
            IsolationMode = PluginAssembly_IsolationMode.Sandbox,
            Version = assemblyVersion,
            Description = description
        };

        writer.Update(entity);
    }

    public void DeletePluginImages(IEnumerable<Image> pluginImages)
    {
        var deleteRequests = pluginImages.ToDeleteRequests(SdkMessageProcessingStepImage.EntityLogicalName).ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("Deleting {count} plugin images in Dataverse", deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    public void DeletePluginSteps(IEnumerable<Step> pluginSteps)
    {
        var deleteRequests = pluginSteps.ToDeleteRequests(SdkMessageProcessingStep.EntityLogicalName).ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("Deleting {count} plugin steps in Dataverse", deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    public void DeletePluginTypes(IEnumerable<Model.Plugin.PluginType> pluginTypes)
    {
        var deleteRequests = pluginTypes.ToDeleteRequests(Context.PluginType.EntityLogicalName).ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("Deleting {count} plugin types in Dataverse", deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    public void UpdatePluginSteps(IEnumerable<Step> pluginSteps, string description)
    {
        var pluginStepReqs = pluginSteps
            .Select(x => new SdkMessageProcessingStep()
            {
                Id = x.Id,
                Stage = (SdkMessageProcessingStep_Stage)x.ExecutionStage,
                FilteringAttributes = x.FilteredAttributes,
                SupportedDeployment = (SdkMessageProcessingStep_SupportedDeployment)x.Deployment,
                Mode = (SdkMessageProcessingStep_Mode)x.ExecutionMode,
                Rank = x.ExecutionOrder,
                Description = description,
                ImpersonatingUserId = x.UserContext == Guid.Empty ? null : new EntityReference(SystemUser.EntityLogicalName, x.UserContext),
                AsyncAutoDelete = x.AsyncAutoDelete,
            }).ToList();

        if (pluginStepReqs.Count > 0)
        {
            log.LogInformation("Updating {count} plugin steps in Dataverse", pluginStepReqs.Count);
            writer.UpdateMultiple(pluginStepReqs);
        }
    }

    public void UpdatePluginImages(IEnumerable<Image> pluginImages, List<Step> pluginSteps)
    {
        var pluginImageReqs = pluginImages
            .Select(x =>
            {
                var stepRef = pluginSteps.FirstOrDefault(s => s.Name == x.PluginStepName)
                    ?? throw new XrmSyncException($"Plugin step '{x.PluginStepName}' not found in the provided steps.");
                
                return new SdkMessageProcessingStepImage()
                {
                    Id = x.Id,
                    Name = x.Name,
                    EntityAlias = x.EntityAlias,
                    ImageType = (SdkMessageProcessingStepImage_ImageType)x.ImageType,
                    Attributes1 = x.Attributes,
                    SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, stepRef.Id),
                };
            }).ToList();

        if (pluginImageReqs.Count > 0)
        {
            log.LogInformation("Updating {count} plugin images in Dataverse", pluginImageReqs.Count);
            writer.UpdateMultiple(pluginImageReqs);
        }
    }

    public List<Model.Plugin.PluginType> CreatePluginTypes(List<Model.Plugin.PluginType> pluginTypes, Guid assemblyId, string description)
    {
        if (pluginTypes.Count == 0) return pluginTypes;

        log.LogInformation("Creating {Count} plugin types in Dataverse.", pluginTypes.Count);
        pluginTypes.ForEach(pluginType =>
        {
            var entity = new Context.PluginType
            {
                Name = pluginType.Name,
                TypeName = pluginType.Name,
                FriendlyName = Guid.NewGuid().ToString(),
                PluginAssemblyId = new EntityReference(PluginAssembly.EntityLogicalName, assemblyId),
                Description = description
            };

            pluginType.Id = writer.Create(entity, Parameters);
        });

        return pluginTypes;
    }

    public List<Step> CreatePluginSteps(List<Step> pluginSteps, List<Model.Plugin.PluginType> pluginTypes, string description)
    {
        if (pluginSteps.Count == 0) return pluginSteps;

        log.LogInformation("Creating {Count} plugin steps in Dataverse.", pluginSteps.Count);
        var eventOperations = pluginSteps.Select(step => step.EventOperation).Distinct();
        var stepLogicalNames = pluginSteps.Select(step => step.LogicalName).Distinct();
        
        var messageFilterIds = messageReader.GetMessageFilters(eventOperations, stepLogicalNames);

        pluginSteps.ForEach(step =>
        {
            var pluginType = pluginTypes.First(type => type.Name == step.PluginTypeName);

            var (messageId, messageFilterReference) = GetMessageFilterReference(step, messageFilterIds);

            var impersonatingUserReference = step.UserContext == Guid.Empty
                ? null
                : new EntityReference(SystemUser.EntityLogicalName, step.UserContext);

            var entity = new SdkMessageProcessingStep
            {
                Name = step.Name,
                AsyncAutoDelete = step.AsyncAutoDelete,
                Rank = step.ExecutionOrder,
                Mode = (SdkMessageProcessingStep_Mode)step.ExecutionMode,
#pragma warning disable CS0612 // Type or member is obsolete
                PluginTypeId = new EntityReference(Context.PluginType.EntityLogicalName, pluginType.Id),
#pragma warning restore CS0612 // Type or member is obsolete
                Stage = (SdkMessageProcessingStep_Stage)step.ExecutionStage,
                FilteringAttributes = step.FilteredAttributes,
                SupportedDeployment = (SdkMessageProcessingStep_SupportedDeployment)step.Deployment,
                Description = description,
                ImpersonatingUserId = impersonatingUserReference,
                SdkMessageId = new EntityReference(SdkMessage.EntityLogicalName, messageId),
                SdkMessageFilterId = messageFilterReference
            };

            step.Id = writer.Create(entity, Parameters);
        });

        return pluginSteps;
    }

    public List<Image> CreatePluginImages(List<Image> pluginImages, List<Step> pluginSteps)
    {
        if (pluginImages.Count == 0) return pluginImages;

        log.LogInformation("Creating {Count} plugin images in Dataverse.", pluginImages.Count);
        pluginImages.ForEach(image =>
        {
            var pluginStep = pluginSteps.First(step => step.Name == image.PluginStepName);
            var messagePropertyName = MessageReader.GetMessagePropertyName(pluginStep.EventOperation);

            var entity = new SdkMessageProcessingStepImage
            {
                Name = image.Name,
                EntityAlias = image.EntityAlias,
                ImageType = (SdkMessageProcessingStepImage_ImageType)image.ImageType,
                Attributes1 = image.Attributes,
                MessagePropertyName = messagePropertyName,
                SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, pluginStep.Id)
            };

            image.Id = writer.Create(entity, Parameters);
        });

        return pluginImages;
    }

    private static (Guid messageId, EntityReference? messageFilterReference) GetMessageFilterReference(Step step, Dictionary<string, MessageFilterMap> messages)
    {
        if (!messages.TryGetValue(step.EventOperation, out var opMessageFilters))
        {
            throw new XrmSyncException($"Message operation '{step.EventOperation}' not found in Dataverse.");
        }

        if (string.IsNullOrEmpty(step.LogicalName))
        {
            // If no logical name is provided, we assume the message filter is not needed
            return (opMessageFilters.MessageId, null);
        }

        if (!opMessageFilters.FilterMap.TryGetValue(step.LogicalName, out var messageFilterId))
        {
            throw new XrmSyncException($"Message operation '{step.EventOperation}' for logical name '{step.LogicalName}' not found in Dataverse.");
        }

        return (opMessageFilters.MessageId, new EntityReference(SdkMessageFilter.EntityLogicalName, messageFilterId));
    }

    private static string GetBase64StringFromFile(string dllPath)
    {
        // Reads the file at dllPath and returns its contents as a Base64 string
        if (string.IsNullOrWhiteSpace(dllPath))
            throw new XrmSyncException("DLL path must not be null or empty.");
        if (!File.Exists(dllPath))
            throw new XrmSyncException($"DLL file not found: {dllPath}");

        byte[] fileBytes = File.ReadAllBytes(dllPath);
        return Convert.ToBase64String(fileBytes);
    }
}
