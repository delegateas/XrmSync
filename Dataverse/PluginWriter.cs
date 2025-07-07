using DG.XrmSync.Dataverse.Extensions;
using DG.XrmSync.Dataverse.Interfaces;
using DG.XrmSync.Model.CustomApi;
using DG.XrmSync.Model.Plugin;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DG.XrmSync.Dataverse;

public class PluginWriter(IMessageReader messageReader, IDataverseWriter writer) : IPluginWriter
{
    public Guid CreatePluginAssembly(string pluginName, string solutionName, string dllPath, string sourceHash, string assemblyVersion, string description)
    {
        var entity = new Entity(EntityTypeNames.PluginAssembly);
        entity.Attributes.Add("name", pluginName);
        entity.Attributes.Add("content", GetBase64StringFromFile(dllPath));
        entity.Attributes.Add("sourcehash", sourceHash);
        entity.Attributes.Add("isolationmode", new OptionSetValue(2));
        entity.Attributes.Add("version", assemblyVersion);
        entity.Attributes.Add("description", description);

        var parameters = new ParameterCollection
    {
        { "SolutionUniqueName", solutionName }
    };

        return writer.Create(entity);
    }

    public void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description)
    {
        var entity = new Entity(EntityTypeNames.PluginAssembly, assemblyId);
        entity.Attributes.Add("name", pluginName);
        entity.Attributes.Add("content", GetBase64StringFromFile(dllPath));
        entity.Attributes.Add("sourcehash", sourceHash);
        entity.Attributes.Add("isolationmode", new OptionSetValue(2));
        entity.Attributes.Add("version", assemblyVersion);
        entity.Attributes.Add("description", description);

        writer.Update(entity);
    }

    public void DeletePlugins(IEnumerable<PluginType> pluginTypes, IEnumerable<Step> pluginSteps, IEnumerable<Image> pluginImages, IEnumerable<ApiDefinition> customApis, IEnumerable<RequestParameter> requestParameters, IEnumerable<ResponseProperty> responseProperties)
    {
        var pluginTypeReqs = pluginTypes.ToDeleteRequests(EntityTypeNames.PluginType);
        var pluginStepReqs = pluginSteps.ToDeleteRequests(EntityTypeNames.PluginStep);
        var pluginImageReqs = pluginImages.ToDeleteRequests(EntityTypeNames.PluginStepImage);
        var customApiReqs = customApis.ToDeleteRequests(EntityTypeNames.CustomApi);
        var paramReqs = requestParameters.ToDeleteRequests(EntityTypeNames.RequestParameter);
        var responseReqs = responseProperties.ToDeleteRequests(EntityTypeNames.ResponseProperty);

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
                var entity = new Entity(EntityTypeNames.PluginStep, x.Id);
                entity.Attributes.Add("stage", new OptionSetValue(x.ExecutionStage));
                entity.Attributes.Add("filteringattributes", x.FilteredAttributes);
                entity.Attributes.Add("supporteddeployment", new OptionSetValue(x.Deployment));
                entity.Attributes.Add("mode", new OptionSetValue(x.ExecutionMode));
                entity.Attributes.Add("rank", x.ExecutionOrder);
                entity.Attributes.Add("description", description);
                entity.Attributes.Add("impersonatinguserid", x.UserContext == Guid.Empty ? null : new EntityReference(EntityTypeNames.SystemUser, x.Id));

                return new UpdateRequest
                {
                    Target = entity
                };
            });

        var pluginImageReqs = pluginImages
            .Select(x =>
            {
                var entity = new Entity(EntityTypeNames.PluginStepImage, x.Id);
                entity.Attributes.Add("name", x.Name);
                entity.Attributes.Add("entityalias", x.EntityAlias);
                entity.Attributes.Add("imagetype", new OptionSetValue(x.ImageType));
                entity.Attributes.Add("attributes", x.ImageType);

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

    public List<PluginType> CreatePluginTypes(List<PluginType> pluginTypes, Guid assemblyId, string description)
    {
        return pluginTypes.ConvertAll(x =>
        {
            var entity = new Entity("plugintype");
            entity.Attributes.Add("name", x.Name);
            entity.Attributes.Add("typename", x.Name);
            entity.Attributes.Add("friendlyname", Guid.NewGuid().ToString());
            entity.Attributes.Add("pluginassemblyid", new EntityReference("pluginassembly", assemblyId));
            entity.Attributes.Add("description", description);

            x.Id = writer.Create(entity);

            return x;
        });
    }

    public List<Step> CreatePluginSteps(List<Step> pluginSteps, List<PluginType> pluginTypes, string solutionName, string description)
    {
        var eventOperations = pluginSteps.Select(step => step.EventOperation).Distinct();
        var messageIds = messageReader.GetMessages(eventOperations);

        return pluginSteps.ConvertAll(step =>
        {
            var pluginType = pluginTypes.First(type => type.Name == step.PluginTypeName);

            if (!messageIds.TryGetValue(step.EventOperation, out var messageId))
            {
                throw new InvalidOperationException($"Message operation '{step.EventOperation}' not found in Dataverse.");
            }
            
            var messageFilter = messageReader.GetMessageFilter(step.LogicalName, messageId);

            var entity = new Entity(EntityTypeNames.PluginStep);
            entity.Attributes.Add("name", step.Name);
            entity.Attributes.Add("asyncautodelete", false);
            entity.Attributes.Add("rank", step.ExecutionOrder);
            entity.Attributes.Add("mode", new OptionSetValue(step.ExecutionMode));
            entity.Attributes.Add("plugintypeid", new EntityReference(EntityTypeNames.PluginType, pluginType.Id));
            entity.Attributes.Add("sdkmessageid", new EntityReference(EntityTypeNames.Message, messageId));
            entity.Attributes.Add("stage", new OptionSetValue(step.ExecutionStage));
            entity.Attributes.Add("filteringattributes", step.FilteredAttributes);
            entity.Attributes.Add("supporteddeployment", new OptionSetValue(step.Deployment));
            entity.Attributes.Add("description", description);
            entity.Attributes.Add("impersonatinguserid", step.UserContext == Guid.Empty ? null : new EntityReference(EntityTypeNames.SystemUser, step.UserContext));
            entity.Attributes.Add("sdkmessagefilterid", string.IsNullOrEmpty(step.LogicalName) || messageFilter is null ? null : new EntityReference(EntityTypeNames.MessageFilter, messageFilter.Id));

            var parameters = new ParameterCollection
            {
                { "SolutionUniqueName", solutionName }
            };

            step.Id = writer.Create(entity, parameters);
            return step;
        });
    }

    public List<Image> CreatePluginImages(List<Image> pluginImages, List<Step> pluginSteps)
    {
        return pluginImages.ConvertAll(image =>
        {
            var pluginStep = pluginSteps.First(step => step.Name == image.PluginStepName);
            var messagePropertyName = MessageReader.GetMessagePropertyName(pluginStep.EventOperation);

            var entity = new Entity(EntityTypeNames.PluginStepImage);
            entity.Attributes.Add("name", image.Name);
            entity.Attributes.Add("entityalias", image.EntityAlias);
            entity.Attributes.Add("imagetype", new OptionSetValue(image.ImageType));
            entity.Attributes.Add("attributes", image.Attributes);
            entity.Attributes.Add("messagepropertyname", messagePropertyName);
            entity.Attributes.Add("sdkmessageprocessingstepid", new EntityReference(EntityTypeNames.PluginStep, pluginStep.Id));

            image.Id = writer.Create(entity);
            return image;
        });
    }

    private static string GetBase64StringFromFile(string dllPath)
    {
        // Reads the file at dllPath and returns its contents as a Base64 string
        if (string.IsNullOrWhiteSpace(dllPath))
            throw new ArgumentException("DLL path must not be null or empty.", nameof(dllPath));
        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"DLL file not found: {dllPath}", dllPath);

        byte[] fileBytes = File.ReadAllBytes(dllPath);
        return Convert.ToBase64String(fileBytes);
    }
}
