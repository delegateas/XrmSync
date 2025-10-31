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

internal class PluginWriter(IMessageReader messageReader, IDataverseWriter writer, IOptions<PluginSyncOptions> configuration) : IPluginWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", configuration.Value.SolutionName }
    };

    public void DeletePluginImages(IEnumerable<Image> pluginImages)
    {
        writer.DeleteMultiple(pluginImages.ToDeleteRequests(SdkMessageProcessingStepImage.EntityLogicalName));
    }

    public void DeletePluginSteps(IEnumerable<Step> pluginSteps)
    {
        writer.DeleteMultiple(pluginSteps.ToDeleteRequests(SdkMessageProcessingStep.EntityLogicalName));
    }

    public void DeletePluginTypes(IEnumerable<PluginDefinition> pluginTypes)
    {
        writer.DeleteMultiple(pluginTypes.ToDeleteRequests(PluginType.EntityLogicalName));
    }

    public void UpdatePluginSteps(IEnumerable<Step> pluginSteps, string description)
    {
        var pluginStepReqs = pluginSteps
            .Select(x => new SdkMessageProcessingStep()
            {
                Id = x.Id,
                Stage = (sdkmessageprocessingstep_stage)x.ExecutionStage,
                FilteringAttributes = x.FilteredAttributes,
                SupportedDeployment = (sdkmessageprocessingstep_supporteddeployment)x.Deployment,
                Mode = (sdkmessageprocessingstep_mode)x.ExecutionMode,
                Rank = x.ExecutionOrder,
                Description = description,
                ImpersonatingUserId = x.UserContext == Guid.Empty ? null : new EntityReference(SystemUser.EntityLogicalName, x.UserContext),
                AsyncAutoDelete = x.AsyncAutoDelete,
            }).ToList();

        if (pluginStepReqs.Count == 0)
        {
            return;
        }

        writer.UpdateMultiple(pluginStepReqs);
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
                    ImageType = (sdkmessageprocessingstepimage_imagetype)image.ImageType,
                    Attributes1 = image.Attributes,
                    SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, step.Id),
                };
            }).ToList();

        if (pluginImageReqs.Count == 0)
        {
            return;
        }

        writer.UpdateMultiple(pluginImageReqs);
    }

    public ICollection<PluginDefinition> CreatePluginTypes(ICollection<PluginDefinition> pluginTypes, Guid assemblyId, string description)
    {
        if (pluginTypes.Count == 0) return pluginTypes;

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
                Mode = (sdkmessageprocessingstep_mode)step.ExecutionMode,
#pragma warning disable CS0612 // Type or member is obsolete
                PluginTypeId = new EntityReference(PluginType.EntityLogicalName, plugin.Id),
#pragma warning restore CS0612 // Type or member is obsolete
                Stage = (sdkmessageprocessingstep_stage)step.ExecutionStage,
                FilteringAttributes = step.FilteredAttributes,
                SupportedDeployment = (sdkmessageprocessingstep_supporteddeployment)step.Deployment,
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

        foreach (var (image, step) in pluginImages)
        {
            var entity = new SdkMessageProcessingStepImage
            {
                Name = image.Name,
                EntityAlias = image.EntityAlias,
                ImageType = (sdkmessageprocessingstepimage_imagetype)image.ImageType,
                Attributes1 = image.Attributes,
                MessagePropertyName = MessageReader.GetMessagePropertyName(step.EventOperation) ?? string.Empty,
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
