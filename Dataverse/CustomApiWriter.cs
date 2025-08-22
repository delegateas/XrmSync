using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public class CustomApiWriter(IDataverseWriter writer, ILogger log, XrmSyncConfiguration configuration) : ICustomApiWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", configuration.Plugin?.Sync?.SolutionName ?? throw new XrmSyncException("No solution name found in configuration") }
    };

    public List<CustomApiDefinition> CreateCustomApis(List<CustomApiDefinition> customApis, string description)
    {
        if (customApis.Count == 0) return customApis;

        log.LogInformation("Creating {Count} Custom APIs in Dataverse.", customApis.Count);
        customApis.ForEach(api =>
        {
            var entity = new CustomApi
            {
                Name = api.Name,
                UniqueName = api.UniqueName,
                DisplayName = api.DisplayName,
                Description = GetDescription(api, description),
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType)api.AllowedCustomProcessingStepType,
                PluginTypeId = new EntityReference(Context.PluginType.EntityLogicalName, api.PluginType.Id),
                IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
                IsPrivate = api.IsPrivate,
                ExecutePrivilegeName = api.ExecutePrivilegeName
            };

            if (api.OwnerId != Guid.Empty)
            {
                entity.OwnerId = new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);
            }

            api.Id = writer.Create(entity, Parameters);
        });

        return customApis;
    }

    public List<RequestParameter> CreateRequestParameters(List<RequestParameter> requestParameters)
    {
        if (requestParameters.Count == 0) return [];

        log.LogInformation("Creating {Count} Custom API Request Parameters in Dataverse.", requestParameters.Count);
        requestParameters.ForEach(param =>
        {
            var entity = new CustomApiRequestParameter
            {
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, param.CustomApi.Id),
                Name = param.Name,
                UniqueName = param.UniqueName,
                DisplayName = param.DisplayName,
                IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
                IsOptional = param.IsOptional,
                LogicalEntityName = param.LogicalEntityName,
                Type = (CustomApiFieldType)param.Type
            };

            param.Id = writer.Create(entity, Parameters);
        });

        return requestParameters;
    }

    public List<ResponseProperty> CreateResponseProperties(List<ResponseProperty> responseProperties)
    {
        if (responseProperties.Count == 0) return responseProperties;

        log.LogInformation("Creating {Count} Custom API Response Properties in Dataverse.", responseProperties.Count);
        responseProperties.ForEach(prop =>
        {
            var entity = new CustomApiResponseProperty
            {
                Name = prop.Name,
                UniqueName = prop.UniqueName,
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, prop.CustomApi.Id),
                DisplayName = prop.DisplayName,
                IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
                LogicalEntityName = prop.LogicalEntityName,
                Type = (CustomApiFieldType)prop.Type
            };

            prop.Id = writer.Create(entity, Parameters);
        });

        return responseProperties;
    }

    public List<CustomApiDefinition> UpdateCustomApis(List<CustomApiDefinition> customApis, string description)
    {
        var updateRequests = customApis.ConvertAll(api =>
        {
            var definition = new CustomApi
            {
                Id = api.Id,
                DisplayName = api.DisplayName,
                Description = GetDescription(api, description),
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType)api.AllowedCustomProcessingStepType,
                PluginTypeId = new EntityReference(Context.PluginType.EntityLogicalName, api.PluginType.Id),
                IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
                IsPrivate = api.IsPrivate,
                ExecutePrivilegeName = api.ExecutePrivilegeName
            };

            if (api.OwnerId != Guid.Empty)
            {
                definition.OwnerId = new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);
            }

            return definition;
        });

        if (updateRequests.Count > 0)
        {
            writer.UpdateMultiple(updateRequests);
        }

        return customApis;
    }

    public List<RequestParameter> UpdateRequestParameters(List<RequestParameter> requestParameters)
    {
        var updateRequests = requestParameters.ConvertAll(param => new CustomApiRequestParameter()
        {
            Id = param.Id,
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
        var updateRequests = responseProperties.ConvertAll(prop => new CustomApiResponseProperty()
        {
            Id = prop.Id,
            DisplayName = prop.DisplayName,
            IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
            LogicalEntityName = prop.LogicalEntityName,
            Type = (CustomApiFieldType?)prop.Type
        });

        if (updateRequests.Count > 0)
            writer.UpdateMultiple(updateRequests);

        return responseProperties;
    }

    public void DeleteCustomApiDefinitions(IEnumerable<CustomApiDefinition> customApis)
    {
        var deleteRequests = customApis.ToDeleteRequests(CustomApi.EntityLogicalName).ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("Deleting {count} custom api definitions in Dataverse", deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    public void DeleteCustomApiRequestParameters(IEnumerable<RequestParameter> requestParameters)
    {
        var deleteRequests = requestParameters.ToDeleteRequests("customapirequestparameter").ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("Deleting {count} custom api request parameters in Dataverse", deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    public void DeleteCustomApiResponseProperties(IEnumerable<ResponseProperty> responseProperties)
    {
        var deleteRequests = responseProperties.ToDeleteRequests("customapiresponseproperty").ToList();

        if (deleteRequests.Count > 0)
        {
            log.LogInformation("Deleting {count} custom api response properties in Dataverse", deleteRequests.Count);
            writer.PerformAsBulk(deleteRequests);
        }
    }

    private static string GetDescription(CustomApiDefinition api, string description)
    {
        return !string.IsNullOrEmpty(api.Description) && !api.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase)
            ? api.Description
            : description;
    }
}
