using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public class CustomApiWriter(IDataverseWriter writer, XrmSyncOptions options) : ICustomApiWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", options.SolutionName }
    };

    public List<ApiDefinition> CreateCustomApis(List<ApiDefinition> customApis, List<Model.Plugin.PluginType> pluginTypes, string solutionPrefix, string description)
    {
        var entities = customApis.ConvertAll(api =>
        {
            var pluginType = pluginTypes.FirstOrDefault(pt => pt.Name == api.PluginTypeName)
                ?? throw new XrmSyncException($"PluginType '{api.PluginTypeName}' not found for CustomApi '{api.UniqueName}'.");

            var ownerId = api.OwnerId == Guid.Empty
                ? null
                : new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);

            return new CustomApi
            {
                Name = api.UniqueName,
                UniqueName = solutionPrefix + "_" + api.UniqueName,
                DisplayName = api.DisplayName,
                Description = GetDescription(api, description),
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType?)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType?)api.AllowedCustomProcessingStepType,
                PluginTypeId = new EntityReference(PluginType.EntityLogicalName, pluginType.Id),
                OwnerId = ownerId,
                IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
                IsPrivate = api.IsPrivate,
                ExecutePrivilegeName = api.ExecutePrivilegeName
            };
        });

        var created = writer.CreateMultiple(entities, Parameters);

        for (var i = 0; i < customApis.Count; i++)
        {
            customApis[i].Id = created[i].Id;
            customApis[i].UniqueName = solutionPrefix + "_" + customApis[i].UniqueName;

            customApis[i].RequestParameters.ForEach(r => r.CustomApiName = customApis[i].UniqueName);
            customApis[i].ResponseProperties.ForEach(r => r.CustomApiName = customApis[i].UniqueName);
        }

        return customApis;
    }

    public List<RequestParameter> CreateRequestParameters(List<RequestParameter> requestParameters, List<ApiDefinition> customApis)
    {
        var entities = requestParameters.ConvertAll(param =>
        {
            var api = customApis.FirstOrDefault(a => a.UniqueName == param.CustomApiName)
                ?? throw new XrmSyncException($"CustomApi '{param.CustomApiName}' not found for request parameter '{param.UniqueName}'.");

            return new CustomApiRequestParameter
            {
                Name = param.UniqueName,
                UniqueName = param.UniqueName,
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, api.Id),
                DisplayName = param.DisplayName,
                IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
                IsOptional = param.IsOptional,
                LogicalEntityName = param.LogicalEntityName,
                Type = (CustomApiFieldType?)param.Type
            };
        });

        var created = writer.CreateMultiple(entities, Parameters);

        for (var i = 0; i < requestParameters.Count; i++)
        {
            requestParameters[i].Id = created[i].Id;
        }

        return requestParameters;
    }

    public List<ResponseProperty> CreateResponseProperties(List<ResponseProperty> responseProperties, List<ApiDefinition> customApis)
    {
        var entities = responseProperties.ConvertAll(prop =>
        {
            var api = customApis.FirstOrDefault(a => a.UniqueName == prop.CustomApiName)
                            ?? throw new XrmSyncException($"CustomApi '{prop.CustomApiName}' not found for response property '{prop.UniqueName}'.");

            return new CustomApiResponseProperty
            {
                Name = prop.UniqueName,
                UniqueName = prop.UniqueName,
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, api.Id),
                DisplayName = prop.DisplayName,
                IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
                LogicalEntityName = prop.LogicalEntityName,
                Type = (CustomApiFieldType?)prop.Type
            };
        });

        var created = writer.CreateMultiple(entities, Parameters);

        for (var i = 0; i < responseProperties.Count; i++)
        {
            responseProperties[i].Id = created[i].Id;
        }

        return responseProperties;
    }

    public List<ApiDefinition> UpdateCustomApis(List<ApiDefinition> customApis, List<Model.Plugin.PluginType> pluginTypes, string description)
    {
        var updateRequests = customApis.ConvertAll(api =>
        {
            var pluginType = pluginTypes.FirstOrDefault(pt => pt.Name == api.PluginTypeName)
                ?? throw new XrmSyncException($"PluginType '{api.PluginTypeName}' not found for CustomApi '{api.UniqueName}'.");

            var ownerId = api.OwnerId == Guid.Empty
                ? null
                : new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);

            return new CustomApi(api.Id)
            {
                DisplayName = api.DisplayName,
                Description = GetDescription(api, description),
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType?)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType?)api.AllowedCustomProcessingStepType,
                PluginTypeId = new EntityReference(PluginType.EntityLogicalName, pluginType.Id),
                OwnerId = ownerId,
                IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
                IsPrivate = api.IsPrivate,
                ExecutePrivilegeName = api.ExecutePrivilegeName
            };
        });

        if (updateRequests.Count > 0)
        {
            writer.UpdateMultiple(updateRequests);
        }

        return customApis;
    }

    public List<RequestParameter> UpdateRequestParameters(List<RequestParameter> requestParameters)
    {
        var updateRequests = requestParameters.ConvertAll(param => new CustomApiRequestParameter(param.Id)
        {
            DisplayName = param.DisplayName,
            IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
            IsOptional = param.IsOptional,
            LogicalEntityName = param.LogicalEntityName,
            Type = (CustomApiFieldType?)param.Type
        });

        if (updateRequests.Count > 0)
            writer.UpdateMultiple(updateRequests);

        return requestParameters;
    }

    public List<ResponseProperty> UpdateResponseProperties(List<ResponseProperty> responseProperties)
    {
        var updateRequests = responseProperties.ConvertAll(prop => new CustomApiResponseProperty(prop.Id)
        {
            DisplayName = prop.DisplayName,
            IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
            LogicalEntityName = prop.LogicalEntityName,
            Type = (CustomApiFieldType?)prop.Type
        });

        if (updateRequests.Count > 0)
            writer.UpdateMultiple(updateRequests);

        return responseProperties;
    }

    private static string GetDescription(ApiDefinition api, string description)
    {
        return !string.IsNullOrEmpty(api.Description) && !api.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase)
            ? api.Description
            : description;
    }
}
