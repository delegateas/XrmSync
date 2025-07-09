using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse;

public class PluginWriter(IMessageReader messageReader, IDataverseWriter writer) : IPluginWriter
{
    public Guid CreatePluginAssembly(string pluginName, string solutionName, string dllPath, string sourceHash, string assemblyVersion, string description)
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

        var parameters = new ParameterCollection
        {
            { "SolutionUniqueName", solutionName }
        };

        return writer.Create(entity, parameters);
    }

    public void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description)
    {
        var entity = new PluginAssembly(assemblyId)
        {
            Name = pluginName,
            Content = GetBase64StringFromFile(dllPath),
            SourceHash = sourceHash,
            IsolationMode = PluginAssembly_IsolationMode.Sandbox,
            Version = assemblyVersion,
            Description = description
        };

        writer.Update(entity);
    }

    public void DeletePlugins(IEnumerable<Model.Plugin.PluginType> pluginTypes, IEnumerable<Step> pluginSteps, IEnumerable<Image> pluginImages, IEnumerable<ApiDefinition> customApis, IEnumerable<RequestParameter> requestParameters, IEnumerable<ResponseProperty> responseProperties)
    {
        var pluginTypeReqs = pluginTypes.ToDeleteRequests(Context.PluginType.EntityLogicalName);
        var pluginStepReqs = pluginSteps.ToDeleteRequests(SdkMessageProcessingStep.EntityLogicalName);
        var pluginImageReqs = pluginImages.ToDeleteRequests(SdkMessageProcessingStepImage.EntityLogicalName);
        var customApiReqs = customApis.ToDeleteRequests(CustomApi.EntityLogicalName);
        var paramReqs = requestParameters.ToDeleteRequests(CustomApiRequestParameter.EntityLogicalName);
        var responseReqs = responseProperties.ToDeleteRequests(CustomApiResponseProperty.EntityLogicalName);

        List<DeleteRequest> deleteRequests = [..pluginImageReqs, ..pluginStepReqs, ..pluginTypeReqs, ..customApiReqs, ..paramReqs, ..responseReqs];

        if (deleteRequests.Count > 0)
        {
            writer.PerformAsBulkWithOutput(deleteRequests, r => r.Target.LogicalName);
        }
    }

    public void UpdatePlugins(IEnumerable<Step> pluginSteps, IEnumerable<Image> pluginImages, string description)
    {
        var pluginStepReqs = pluginSteps
            .Select(x =>
            {
                var impersonatingUser = x.UserContext == Guid.Empty
                    ? null
                    : new EntityReference(SystemUser.EntityLogicalName, x.UserContext);

                var entity = new SdkMessageProcessingStep(x.Id)
                {
                    Stage = (SdkMessageProcessingStep_Stage)x.ExecutionStage,
                    FilteringAttributes = x.FilteredAttributes,
                    SupportedDeployment = (SdkMessageProcessingStep_SupportedDeployment)x.Deployment,
                    Mode = (SdkMessageProcessingStep_Mode)x.ExecutionMode,
                    Rank = x.ExecutionOrder,
                    Description = description,
                    ImpersonatingUserId = impersonatingUser
                };

                return new UpdateRequest
                {
                    Target = entity
                };
            });

        var pluginImageReqs = pluginImages
            .Select(x =>
            {
                var entity = new SdkMessageProcessingStepImage(x.Id)
                {
                    Name = x.Name,
                    EntityAlias = x.EntityAlias,
                    ImageType = (SdkMessageProcessingStepImage_ImageType)x.ImageType,
                    Attributes1 = x.Attributes
                };

                return new UpdateRequest
                {
                    Target = entity
                };
            });

        List<UpdateRequest> updateRequests = [.. pluginImageReqs, .. pluginStepReqs];

        if (updateRequests.Count > 0)
        {
            writer.PerformAsBulkWithOutput(updateRequests, r => r.Target.LogicalName);
        }
    }

    public List<Model.Plugin.PluginType> CreatePluginTypes(List<Model.Plugin.PluginType> pluginTypes, Guid assemblyId, string description)
    {
        return pluginTypes.ConvertAll(x =>
        {
            var entity = new Context.PluginType
            {
                Name = x.Name,
                TypeName = x.Name,
                FriendlyName = Guid.NewGuid().ToString(),
                PluginAssemblyId = new EntityReference(PluginAssembly.EntityLogicalName, assemblyId),
                Description = description
            };

            x.Id = writer.Create(entity);

            return x;
        });
    }

    public List<Step> CreatePluginSteps(List<Step> pluginSteps, List<Model.Plugin.PluginType> pluginTypes, string solutionName, string description)
    {
        var eventOperations = pluginSteps.Select(step => step.EventOperation).Distinct();
        var stepLogicalNames = pluginSteps.Select(step => step.LogicalName).Distinct();
        
        var messageFilterIds = messageReader.GetMessageFilters(eventOperations, stepLogicalNames);

        // TODO: Can we use CreateMultiple instead?
        return pluginSteps.ConvertAll(step =>
        {
            var pluginType = pluginTypes.First(type => type.Name == step.PluginTypeName);

            var (messageId, messageFilterReference) = GetMessageFilterReference(step, messageFilterIds);

            var impersonatingUserReference = step.UserContext == Guid.Empty
                ? null
                : new EntityReference(SystemUser.EntityLogicalName, step.UserContext);

#pragma warning disable CS0612 // Type or member is obsolete
            var entity = new SdkMessageProcessingStep
            {
                Name = step.Name,
                AsyncAutoDelete = false, // TODO: This should be configurable
                Rank = step.ExecutionOrder,
                Mode = (SdkMessageProcessingStep_Mode)step.ExecutionMode,
                PluginTypeId = new EntityReference(Context.PluginType.EntityLogicalName, pluginType.Id),
                Stage = (SdkMessageProcessingStep_Stage)step.ExecutionStage,
                FilteringAttributes = step.FilteredAttributes,
                SupportedDeployment = (SdkMessageProcessingStep_SupportedDeployment)step.Deployment,
                Description = description,
                ImpersonatingUserId = impersonatingUserReference,
                SdkMessageId = new EntityReference(SdkMessage.EntityLogicalName, messageId),
                SdkMessageFilterId = messageFilterReference
            };
#pragma warning restore CS0612 // Type or member is obsolete

            var parameters = new ParameterCollection
            {
                { "SolutionUniqueName", solutionName }
            };

            step.Id = writer.Create(entity, parameters);
            return step;
        });
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

    public List<Image> CreatePluginImages(List<Image> pluginImages, List<Step> pluginSteps)
    {
        // TODO: Can we use CreateMultiple instead?
        return pluginImages.ConvertAll(image =>
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
            
            image.Id = writer.Create(entity);
            return image;
        });
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
