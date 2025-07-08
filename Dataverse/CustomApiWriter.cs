using DG.XrmSync.Dataverse.Interfaces;
using DG.XrmSync.Model.CustomApi;
using DG.XrmSync.Model.Exceptions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DG.XrmSync.Dataverse;

public class CustomApiWriter(IDataverseWriter writer) : ICustomApiWriter
{
    public List<ApiDefinition> CreateCustomApis(List<ApiDefinition> customApis, string solutionName, string description)
    {
        foreach (var api in customApis)
        {
            var entity = new Entity(EntityTypeNames.CustomApi);
            entity["name"] = api.UniqueName;
            entity["uniquename"] = api.UniqueName;
            entity["displayname"] = api.DisplayName;
            entity["description"] = api.Description ?? description;
            entity["isfunction"] = api.IsFunction;
            entity["enabledforworkflow"] = api.EnabledForWorkflow;
            entity["bindingtype"] = api.BindingType;
            entity["boundentitylogicalname"] = api.BoundEntityLogicalName;
            entity["allowedcustomprocessingsteptype"] = api.AllowedCustomProcessingStepType;
            entity["plugintypename"] = api.PluginTypeName;
            entity["ownerid"] = api.OwnerId;
            entity["iscustomizable"] = api.IsCustomizable;
            entity["isprivate"] = api.IsPrivate;
            entity["executeprivilegename"] = api.ExecutePrivilegeName;

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
            var entity = new Entity(EntityTypeNames.RequestParameter);
            entity["name"] = param.UniqueName;
            entity["uniquename"] = param.UniqueName;
            entity["customapiid"] = new EntityReference(EntityTypeNames.CustomApi, api.Id);
            entity["displayname"] = param.DisplayName;
            entity["iscustomizable"] = param.IsCustomizable;
            entity["isoptional"] = param.IsOptional;
            entity["logicalentityname"] = param.LogicalEntityName;
            entity["type"] = param.Type;

            param.Id = writer.Create(entity);
        }
        return requestParameters;
    }

    public List<ResponseProperty> CreateResponseProperties(List<ResponseProperty> responseProperties, List<ApiDefinition> customApis)
    {
        foreach (var prop in responseProperties)
        {
            var api = customApis.FirstOrDefault(a => a.UniqueName == prop.CustomApiName);
            if (api == null)
                throw new XrmSyncException($"CustomApi '{prop.CustomApiName}' not found for response property '{prop.UniqueName}'.");

            var entity = new Entity(EntityTypeNames.ResponseProperty);
            entity["name"] = prop.UniqueName;
            entity["uniquename"] = prop.UniqueName;
            entity["customapiid"] = new EntityReference(EntityTypeNames.CustomApi, api.Id);
            entity["displayname"] = prop.DisplayName;
            entity["iscustomizable"] = prop.IsCustomizable;
            entity["logicalentityname"] = prop.LogicalEntityName;
            entity["type"] = prop.Type;

            prop.Id = writer.Create(entity);
        }
        return responseProperties;
    }

    public List<ApiDefinition> UpdateCustomApis(List<ApiDefinition> customApis, string description)
    {
        var updateRequests = customApis.Select(api =>
        {
            var entity = new Entity(EntityTypeNames.CustomApi, api.Id);
            entity["displayname"] = api.DisplayName;
            entity["description"] = api.Description ?? description;
            entity["isfunction"] = api.IsFunction;
            entity["enabledforworkflow"] = api.EnabledForWorkflow;
            entity["bindingtype"] = api.BindingType;
            entity["boundentitylogicalname"] = api.BoundEntityLogicalName;
            entity["allowedcustomprocessingsteptype"] = api.AllowedCustomProcessingStepType;
            entity["plugintypename"] = api.PluginTypeName;
            entity["ownerid"] = api.OwnerId;
            entity["iscustomizable"] = api.IsCustomizable;
            entity["isprivate"] = api.IsPrivate;
            entity["executeprivilegename"] = api.ExecutePrivilegeName;

            return new UpdateRequest { Target = entity };
        }).ToList();

        if (updateRequests.Count > 0)
            writer.PerformAsBulkWithOutput(updateRequests, r => r.Target.LogicalName);

        return customApis;
    }

    public List<RequestParameter> UpdateRequestParameters(List<RequestParameter> requestParameters)
    {
        var updateRequests = requestParameters.Select(param =>
        {
            var entity = new Entity(EntityTypeNames.RequestParameter, param.Id);
            entity["displayname"] = param.DisplayName;
            entity["iscustomizable"] = param.IsCustomizable;
            entity["isoptional"] = param.IsOptional;
            entity["logicalentityname"] = param.LogicalEntityName;
            entity["type"] = param.Type;

            return new UpdateRequest { Target = entity };
        }).ToList();

        if (updateRequests.Count > 0)
            writer.PerformAsBulkWithOutput(updateRequests, r => r.Target.LogicalName);

        return requestParameters;
    }

    public List<ResponseProperty> UpdateResponseProperties(List<ResponseProperty> responseProperties)
    {
        var updateRequests = responseProperties.Select(prop =>
        {
            var entity = new Entity(EntityTypeNames.ResponseProperty, prop.Id);
            entity["displayname"] = prop.DisplayName;
            entity["iscustomizable"] = prop.IsCustomizable;
            entity["logicalentityname"] = prop.LogicalEntityName;
            entity["type"] = prop.Type;

            return new UpdateRequest { Target = entity };
        }).ToList();

        if (updateRequests.Count > 0)
            writer.PerformAsBulkWithOutput(updateRequests, r => r.Target.LogicalName);

        return responseProperties;
    }
}
