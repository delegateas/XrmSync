using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public class CustomApiWriter(IDataverseWriter writer) : ICustomApiWriter
{
    public List<ApiDefinition> CreateCustomApis(List<ApiDefinition> customApis, string solutionName, string description)
    {
        foreach (var api in customApis)
        {
            var ownerId = api.OwnerId == Guid.Empty
                ? null
                : new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);

            var entity = new CustomApi {
                Name = api.UniqueName,
                UniqueName = api.UniqueName, // TODO: ADD PREFIX FROM SOLUTION
                DisplayName = api.DisplayName,
                Description = api.Description ?? description,
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType?)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType?)api.AllowedCustomProcessingStepType,
                PluginTypeId = null, // TODO: FIND PLUGIN TYPE ID
                OwnerId = ownerId,
                IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
                IsPrivate = api.IsPrivate,
                ExecutePrivilegeName = api.ExecutePrivilegeName
            };

            var parameters = new ParameterCollection
            {
                { "SolutionUniqueName", solutionName }
            };

            api.Id = writer.Create(entity, parameters);
        }

        return customApis;
    }

    public List<RequestParameter> CreateRequestParameters(List<RequestParameter> requestParameters, List<ApiDefinition> customApis)
    {
        foreach (var param in requestParameters)
        {
            var api = customApis.FirstOrDefault(a => a.UniqueName == param.CustomApiName)
                ?? throw new XrmSyncException($"CustomApi '{param.CustomApiName}' not found for request parameter '{param.UniqueName}'.");
            var entity = new CustomApiRequestParameter {
                Name = param.UniqueName,
                UniqueName = param.UniqueName,
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, api.Id),
                DisplayName = param.DisplayName,
                IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
                IsOptional = param.IsOptional,
                LogicalEntityName = param.LogicalEntityName,
                Type = (CustomApiFieldType?)param.Type
            };

            param.Id = writer.Create(entity);
        }
        return requestParameters;
    }

    public List<ResponseProperty> CreateResponseProperties(List<ResponseProperty> responseProperties, List<ApiDefinition> customApis)
    {
        foreach (var prop in responseProperties)
        {
            var api = customApis.FirstOrDefault(a => a.UniqueName == prop.CustomApiName)
                ?? throw new XrmSyncException($"CustomApi '{prop.CustomApiName}' not found for response property '{prop.UniqueName}'.");

            var entity = new CustomApiResponseProperty
            {
                Name = prop.UniqueName,
                UniqueName = prop.UniqueName,
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, api.Id),
                DisplayName = prop.DisplayName,
                IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
                LogicalEntityName = prop.LogicalEntityName,
                Type = (CustomApiFieldType?)prop.Type
            };

            prop.Id = writer.Create(entity);
        }
        return responseProperties;
    }

    public List<ApiDefinition> UpdateCustomApis(List<ApiDefinition> customApis, string description)
    {
        var updateRequests = customApis.ConvertAll(api =>
        {
            var ownerId = api.OwnerId == Guid.Empty
                ? null
                : new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);

            var entity = new CustomApi(api.Id)
            {
                DisplayName = api.DisplayName,
                Description = api.Description ?? description,
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType?)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType?)api.AllowedCustomProcessingStepType,
                PluginTypeId = null, // TODO: FIND PLUGIN TYPE ID
                OwnerId = ownerId,
                IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
                IsPrivate = api.IsPrivate,
                ExecutePrivilegeName = api.ExecutePrivilegeName
            };

            return new UpdateRequest { Target = entity };
        });

        if (updateRequests.Count > 0)
            writer.PerformAsBulkWithOutput(updateRequests, r => r.Target.LogicalName);

        return customApis;
    }

    public List<RequestParameter> UpdateRequestParameters(List<RequestParameter> requestParameters)
    {
        var updateRequests = requestParameters.ConvertAll(param =>
        {
            var entity = new CustomApiRequestParameter(param.Id)
            {
                DisplayName = param.DisplayName,
                IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
                IsOptional = param.IsOptional,
                LogicalEntityName = param.LogicalEntityName,
                Type = (CustomApiFieldType?)param.Type
            };

            return new UpdateRequest { Target = entity };
        });

        if (updateRequests.Count > 0)
            writer.PerformAsBulkWithOutput(updateRequests, r => r.Target.LogicalName);

        return requestParameters;
    }

    public List<ResponseProperty> UpdateResponseProperties(List<ResponseProperty> responseProperties)
    {
        var updateRequests = responseProperties.ConvertAll(prop =>
        {
            var entity = new CustomApiResponseProperty(prop.Id)
            {
                DisplayName = prop.DisplayName,
                IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
                LogicalEntityName = prop.LogicalEntityName,
                Type = (CustomApiFieldType?)prop.Type
            };

            return new UpdateRequest { Target = entity };
        });

        if (updateRequests.Count > 0)
            writer.PerformAsBulkWithOutput(updateRequests, r => r.Target.LogicalName);

        return responseProperties;
    }
}
