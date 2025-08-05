using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public class CustomApiWriter(IDataverseWriter writer, ILogger log, XrmSyncOptions options) : ICustomApiWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", options.SolutionName }
    };

    public List<CustomApiDefinition> CreateCustomApis(List<CustomApiDefinition> customApis, List<Model.Plugin.PluginType> pluginTypes, string solutionPrefix, string description)
    {
        if (customApis.Count == 0) return customApis;

        log.LogInformation("Creating {Count} Custom APIs in Dataverse.", customApis.Count);
        customApis.ForEach(api =>
        {
            var pluginType = pluginTypes.FirstOrDefault(pt => pt.Name == api.PluginTypeName)
                ?? throw new XrmSyncException($"PluginType '{api.PluginTypeName}' not found for CustomApi '{api.UniqueName}'.");

            api.UniqueName = solutionPrefix + "_" + api.UniqueName;

            var entity = new CustomApi
            {
                Name = api.Name,
                UniqueName = api.UniqueName,
                DisplayName = api.DisplayName,
                Description = GetDescription(api, description),
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType?)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType?)api.AllowedCustomProcessingStepType,
                PluginTypeId = new EntityReference(PluginType.EntityLogicalName, pluginType.Id),
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

    public List<RequestParameter> CreateRequestParameters(List<RequestParameter> requestParameters, List<CustomApiDefinition> customApis)
    {
        if (requestParameters.Count == 0) return requestParameters;

        log.LogInformation("Creating {Count} Custom API Request Parameters in Dataverse.", requestParameters.Count);
        requestParameters.ForEach(param =>
        {
            var api = customApis.FirstOrDefault(a => a.Name == param.CustomApiName)
                ?? throw new XrmSyncException($"CustomApi '{param.CustomApiName}' not found for request parameter '{param.UniqueName}'.");

            var entity = new CustomApiRequestParameter
            {
                Name = param.Name,
                UniqueName = param.UniqueName,
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, api.Id),
                DisplayName = param.DisplayName,
                IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
                IsOptional = param.IsOptional,
                LogicalEntityName = param.LogicalEntityName,
                Type = (CustomApiFieldType?)param.Type
            };

            param.Id = writer.Create(entity, Parameters);
        });

        return requestParameters;
    }

    public List<ResponseProperty> CreateResponseProperties(List<ResponseProperty> responseProperties, List<CustomApiDefinition> customApis)
    {
        if (responseProperties.Count == 0) return responseProperties;

        log.LogInformation("Creating {Count} Custom API Response Properties in Dataverse.", responseProperties.Count);
        responseProperties.ForEach(prop =>
        {
            var api = customApis.FirstOrDefault(a => a.Name == prop.CustomApiName)
                            ?? throw new XrmSyncException($"CustomApi '{prop.CustomApiName}' not found for response property '{prop.UniqueName}'.");

            var entity = new CustomApiResponseProperty
            {
                Name = prop.Name,
                UniqueName = prop.UniqueName,
                CustomApiId = new EntityReference(CustomApi.EntityLogicalName, api.Id),
                DisplayName = prop.DisplayName,
                IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
                LogicalEntityName = prop.LogicalEntityName,
                Type = (CustomApiFieldType?)prop.Type
            };

            prop.Id = writer.Create(entity, Parameters);
        });

        return responseProperties;
    }

    public List<CustomApiDefinition> UpdateCustomApis(List<CustomApiDefinition> customApis, List<Model.Plugin.PluginType> pluginTypes, string description)
    {
        var updateRequests = customApis.ConvertAll(api =>
        {
            var pluginType = pluginTypes.FirstOrDefault(pt => pt.Name == api.PluginTypeName)
                ?? throw new XrmSyncException($"PluginType '{api.PluginTypeName}' not found for CustomApi '{api.UniqueName}'.");

            var definition = new CustomApi
            {
                Id = api.Id,
                DisplayName = api.DisplayName,
                Description = GetDescription(api, description),
                IsFunction = api.IsFunction,
                WorkflowSdkStepEnabled = api.EnabledForWorkflow,
                BindingType = (CustomApi_BindingType?)api.BindingType,
                BoundEntityLogicalName = api.BoundEntityLogicalName,
                AllowedCustomProcessingStepType = (CustomApi_AllowedCustomProcessingStepType?)api.AllowedCustomProcessingStepType,
                PluginTypeId = new EntityReference(PluginType.EntityLogicalName, pluginType.Id),
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
