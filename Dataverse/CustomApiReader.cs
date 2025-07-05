using DG.XrmSync.Dataverse.Context;
using DG.XrmSync.Dataverse.Interfaces;
using DG.XrmSync.Model.CustomApi;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmSync.Dataverse;

public class CustomApiReader(IDataverseReader reader) : ICustomApiReader
{
    public List<ApiDefinition> GetCustomApis(Guid solutionId)
    {
        var query = new QueryExpression(CustomApi.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                CustomApi.Fields.Name,
                CustomApi.Fields.UniqueName,
                CustomApi.Fields.DisplayName,
                CustomApi.Fields.Description,
                CustomApi.Fields.IsFunction,
                CustomApi.Fields.WorkflowSdkStepEnabled,
                CustomApi.Fields.BindingType,
                CustomApi.Fields.BoundEntityLogicalName,
                CustomApi.Fields.AllowedCustomProcessingStepType,
                CustomApi.Fields.OwnerId,
                CustomApi.Fields.IsCustomizable,
                CustomApi.Fields.IsPrivate,
                CustomApi.Fields.ExecutePrivilegeName
            )
        };

        var solutionComponent = query.AddLink(SolutionComponent.EntityLogicalName, CustomApi.PrimaryIdAttribute, SolutionComponent.Fields.ObjectId);
        solutionComponent.LinkCriteria.Conditions.Add(new ConditionExpression(SolutionComponent.Fields.SolutionId, ConditionOperator.Equal, solutionId));

        var pluginType = query.AddLink(PluginType.EntityLogicalName, CustomApi.Fields.PluginTypeId, PluginType.Fields.PluginTypeId, JoinOperator.LeftOuter);
        pluginType.Columns = new ColumnSet(PluginType.Fields.Name);
        pluginType.EntityAlias = "pt";

        var requestParameterLink = query.AddLink(CustomApiRequestParameter.EntityLogicalName, CustomApi.PrimaryIdAttribute, CustomApiRequestParameter.Fields.CustomApiId, JoinOperator.LeftOuter);
        requestParameterLink.Columns = new ColumnSet(
            CustomApiRequestParameter.PrimaryIdAttribute,
            CustomApiRequestParameter.Fields.Name,
            CustomApiRequestParameter.Fields.DisplayName,
            CustomApiRequestParameter.Fields.UniqueName,
            CustomApiRequestParameter.Fields.LogicalEntityName,
            CustomApiRequestParameter.Fields.Type,
            CustomApiRequestParameter.Fields.IsOptional,
            CustomApiRequestParameter.Fields.IsCustomizable
        );
        requestParameterLink.EntityAlias = "req";

        var responsePropertyLink = query.AddLink(CustomApiResponseProperty.EntityLogicalName, CustomApi.PrimaryIdAttribute, CustomApiResponseProperty.Fields.CustomApiId, JoinOperator.LeftOuter);
        responsePropertyLink.Columns = new ColumnSet(
            CustomApiResponseProperty.PrimaryIdAttribute,
            CustomApiResponseProperty.Fields.Name,
            CustomApiResponseProperty.Fields.DisplayName,
            CustomApiResponseProperty.Fields.UniqueName,
            CustomApiResponseProperty.Fields.LogicalEntityName,
            CustomApiResponseProperty.Fields.Type,
            CustomApiResponseProperty.Fields.IsCustomizable
        );
        responsePropertyLink.EntityAlias = "resp";

        var customApis = reader.RetrieveMultiple(query);
        var grouped = customApis.GroupBy(c => c.Id);
        return grouped.Select(group =>
        {
            var customApi = group.First();
            var customApiName = customApi.GetAttributeValue<string>(CustomApi.Fields.Name);
            var requestParameters = group
                .Where(e => e.Contains($"req.{CustomApiRequestParameter.PrimaryIdAttribute}"))
                .GroupBy(e => (e.GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.PrimaryIdAttribute}").Value as Guid?) ?? Guid.Empty)
                .Select(e => new RequestParameter
                {
                    Id = (e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.PrimaryIdAttribute}").Value as Guid?) ?? Guid.Empty,
                    CustomApiName = customApiName,
                    Name = (e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.Fields.Name}").Value as string) ?? string.Empty,
                    DisplayName = (e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.Fields.DisplayName}").Value as string) ?? string.Empty,
                    UniqueName = (e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.Fields.UniqueName}").Value as string) ?? string.Empty,
                    LogicalEntityName = (e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.Fields.LogicalEntityName}")?.Value as string) ?? string.Empty,
                    Type = ((e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.Fields.Type}").Value as OptionSetValue)?.Value) ?? 0,
                    IsOptional = (e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.Fields.IsOptional}").Value as bool?) ?? false,
                    IsCustomizable = (e.First().GetAttributeValue<AliasedValue>($"req.{CustomApiRequestParameter.Fields.IsCustomizable}").Value as bool?) ?? false,
                }).ToList();

            var responseProperties = group
                .Where(e => e.Contains($"resp.{CustomApiResponseProperty.PrimaryIdAttribute}"))
                .GroupBy(e => (e.GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.PrimaryIdAttribute}").Value as Guid?) ?? Guid.Empty)
                .Select(e => new ResponseProperty
                {
                    Id = (e.First().GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.PrimaryIdAttribute}").Value as Guid?) ?? Guid.Empty,
                    CustomApiName = customApiName,
                    Name = (e.First().GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.Fields.Name}").Value as string) ?? string.Empty,
                    DisplayName = (e.First().GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.Fields.DisplayName}").Value as string) ?? string.Empty,
                    UniqueName = (e.First().GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.Fields.UniqueName}").Value as string) ?? string.Empty,
                    LogicalEntityName = (e.First().GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.Fields.LogicalEntityName}")?.Value as string) ?? string.Empty,
                    Type = ((e.First().GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.Fields.Type}").Value as OptionSetValue)?.Value) ?? 0,
                    IsCustomizable = (e.First().GetAttributeValue<AliasedValue>($"resp.{CustomApiResponseProperty.Fields.IsCustomizable}").Value as bool?) ?? false,
                }).ToList();

            return new ApiDefinition
            {
                Id = customApi.Id,
                Name = customApiName,
                UniqueName = customApi.GetAttributeValue<string?>(CustomApi.Fields.UniqueName) ?? string.Empty,
                DisplayName = customApi.GetAttributeValue<string?>(CustomApi.Fields.DisplayName) ?? string.Empty,
                Description = customApi.GetAttributeValue<string?>(CustomApi.Fields.Description) ?? string.Empty,
                IsFunction = customApi.GetAttributeValue<bool>(CustomApi.Fields.IsFunction),
                EnabledForWorkflow = customApi.GetAttributeValue<bool>(CustomApi.Fields.WorkflowSdkStepEnabled),
                BindingType = customApi.GetAttributeValue<OptionSetValue>(CustomApi.Fields.BindingType)?.Value ?? 0,
                BoundEntityLogicalName = customApi.GetAttributeValue<string?>(CustomApi.Fields.BoundEntityLogicalName) ?? string.Empty,
                AllowedCustomProcessingStepType = customApi.GetAttributeValue<OptionSetValue>(CustomApi.Fields.AllowedCustomProcessingStepType)?.Value ?? 0,
                PluginTypeName = (customApi.GetAttributeValue<AliasedValue>($"pt.{PluginType.Fields.Name}").Value as string) ?? string.Empty,
                OwnerId = customApi.GetAttributeValue<EntityReference>(CustomApi.Fields.OwnerId)?.Id ?? Guid.Empty,
                IsCustomizable = customApi.GetAttributeValue<BooleanManagedProperty>(CustomApi.Fields.IsCustomizable).Value,
                IsPrivate = customApi.GetAttributeValue<bool>(CustomApi.Fields.IsPrivate),
                ExecutePrivilegeName = customApi.GetAttributeValue<string?>(CustomApi.Fields.ExecutePrivilegeName) ?? string.Empty,
                RequestParameters = requestParameters,
                ResponseProperties = responseProperties
            };
        })
        .ToList();
    }
}
