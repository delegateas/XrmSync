using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse;

internal class PluginWriter(IMessageReader messageReader, IDataverseWriter writer, ILogger<PluginWriter> log, IOptions<XrmSyncConfiguration> configuration) : IPluginWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", configuration.Value.Plugin?.Sync?.SolutionName ?? throw new XrmSyncException("No solution name found in configuration") }
    };

    private readonly string LogPrefix = configuration.Value.Plugin?.Sync?.DryRun == true ? "[DRY RUN] " : string.Empty;

    public void DeletePluginImages(IEnumerable<Image> pluginImages)
    {
        var deleteRequests = pluginImages.ToDeleteRequests(SdkMessageProcessingStepImage.EntityLogicalName).ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("{prefix}Deleting {count} plugin images in Dataverse", LogPrefix, deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    public void DeletePluginSteps(IEnumerable<Step> pluginSteps)
    {
        var deleteRequests = pluginSteps.ToDeleteRequests(SdkMessageProcessingStep.EntityLogicalName).ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("{prefix}Deleting {count} plugin steps in Dataverse", LogPrefix, deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    public void DeletePluginTypes(IEnumerable<PluginDefinition> pluginTypes)
    {
        var deleteRequests = pluginTypes.ToDeleteRequests(PluginType.EntityLogicalName).ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("{prefix}Deleting {count} plugin types in Dataverse", LogPrefix, deleteRequests.Count);
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
            log.LogInformation("{prefix}Updating {count} plugin steps in Dataverse", LogPrefix, pluginStepReqs.Count);
            writer.UpdateMultiple(pluginStepReqs);
        }
    }

    public void UpdatePluginImages(IEnumerable<ParentReference<Image, Step>> pluginImages)
    {
        var pluginImageReqs = pluginImages
            .Select(reference =>
            {
                var (image, step) = reference;
                return new SdkMessageProcessingStepImage()
                {
                    Id = image.Id,
                    Name = image.Name,
                    EntityAlias = image.EntityAlias,
                    ImageType = (SdkMessageProcessingStepImage_ImageType)image.ImageType,
                    Attributes1 = image.Attributes,
                    SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, step.Id),
                };
            }).ToList();

        if (pluginImageReqs.Count > 0)
        {
            log.LogInformation("{prefix}Updating {count} plugin images in Dataverse", LogPrefix, pluginImageReqs.Count);
            writer.UpdateMultiple(pluginImageReqs);
        }
    }

    public ICollection<PluginDefinition> CreatePluginTypes(ICollection<PluginDefinition> pluginTypes, Guid assemblyId, string description)
    {
        if (pluginTypes.Count == 0) return pluginTypes;

        log.LogInformation("{prefix}Creating {Count} plugin types in Dataverse.", LogPrefix, pluginTypes.Count);
        foreach (var pluginType in pluginTypes)
        {
            var entity = new PluginType
            {
                Name = pluginType.Name,
                TypeName = pluginType.Name,
                FriendlyName = Guid.NewGuid().ToString(),
                PluginAssemblyId = new EntityReference(PluginAssembly.EntityLogicalName, assemblyId),
                Description = description
            };

            pluginType.Id = writer.Create(entity, Parameters);
        }

        return pluginTypes;
    }

    public ICollection<ParentReference<Step, PluginDefinition>> CreatePluginSteps(ICollection<ParentReference<Step, PluginDefinition>> pluginSteps, string description)
    {
        if (pluginSteps.Count == 0) return pluginSteps;

        log.LogInformation("{prefix}Creating {Count} plugin steps in Dataverse.", LogPrefix, pluginSteps.Count);
        var eventOperations = pluginSteps.Select(step => step.Entity.EventOperation).Distinct().Where(s => !string.IsNullOrWhiteSpace(s));
        var stepLogicalNames = pluginSteps.Select(step => step.Entity.LogicalName).Distinct().Where(s => !string.IsNullOrWhiteSpace(s));
        
        var messageFilterIds = messageReader.GetMessageFilters(eventOperations, stepLogicalNames);

        foreach (var (step, plugin) in pluginSteps)
        {
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
                PluginTypeId = new EntityReference(PluginType.EntityLogicalName, plugin.Id),
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
        }

        return pluginSteps;
    }

    public ICollection<ParentReference<Image, Step>> CreatePluginImages(ICollection<ParentReference<Image, Step>> pluginImages)
    {
        if (pluginImages.Count == 0) return pluginImages;

        log.LogInformation("{prefix}Creating {Count} plugin images in Dataverse.", LogPrefix, pluginImages.Count);
        foreach (var (image, step) in pluginImages)
        {
            var entity = new SdkMessageProcessingStepImage
            {
                Name = image.Name,
                EntityAlias = image.EntityAlias,
                ImageType = (SdkMessageProcessingStepImage_ImageType)image.ImageType,
                Attributes1 = image.Attributes,
                MessagePropertyName = MessageReader.GetMessagePropertyName(step.EventOperation),
                SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, step.Id)
            };

            image.Id = writer.Create(entity, Parameters);
        }

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
}
