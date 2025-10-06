using XrmPluginCore.Enums;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.CustomApi;

namespace XrmSync.Dataverse;

public class CustomApiReader(IDataverseReader reader) : ICustomApiReader
{
    public List<CustomApiDefinition> GetCustomApis(Guid solutionId)
    {
        // Get CustomAPIs that are part of the solution
        List<CustomApiDefinition> data = [..
            from api in reader.CustomApis
            join sc in reader.SolutionComponents on api.CustomApiId equals sc.ObjectId
            where sc.SolutionId != null && sc.SolutionId.Id == solutionId
            select new CustomApiDefinition(api.Name ?? string.Empty)
            {
                Id = api.Id,
                UniqueName = api.UniqueName ?? string.Empty,
                DisplayName = api.DisplayName ?? string.Empty,
                Description = api.Description ?? string.Empty,
                IsFunction = api.IsFunction ?? false,
                EnabledForWorkflow = api.WorkflowSdkStepEnabled ?? false,
                BindingType = (BindingType)(api.BindingType ?? CustomApi_BindingType.Global),
                BoundEntityLogicalName = api.BoundEntityLogicalName ?? string.Empty,
                AllowedCustomProcessingStepType = (AllowedCustomProcessingStepType)(api.AllowedCustomProcessingStepType ?? CustomApi_AllowedCustomProcessingStepType.None),
                OwnerId = api.OwnerId != null ? api.OwnerId.Id : Guid.Empty,
                IsCustomizable = api.IsCustomizable != null ? api.IsCustomizable.Value : false,
                IsPrivate = api.IsPrivate ?? false,
                ExecutePrivilegeName = api.ExecutePrivilegeName ?? string.Empty,

                PluginType = new Model.CustomApi.PluginType(string.Empty) { // Name will be populated later
                    Id = api.PluginTypeId != null ? api.PluginTypeId.Id : Guid.Empty
                },
            }];

        // If no Custom APIs found, return empty list
        if (data.Count == 0)
        {
            return [];
        }

        var pluginTypeNames = reader.RetrieveByColumn<Context.PluginType, Guid?>(
            pt => pt.PluginTypeId,
            [.. data.Select(d => d.PluginType.Id).Distinct()],
            pt => pt.Name
        ).ToDictionary(pt => pt.Id, pt => pt.Name ?? string.Empty);

        var reqs = reader.RetrieveByColumn<CustomApiRequestParameter>(
            r => r.CustomApiId,
            [.. data.Select(d => d.Id).Distinct()],
            r => r.Name,
            r => r.DisplayName,
            r => r.UniqueName,
            r => r.LogicalEntityName,
            r => r.Type,
            r => r.IsOptional,
            r => r.IsCustomizable,
            r => r.CustomApiId
        ).ToLookup(r => r.CustomApiId?.Id ?? Guid.Empty, r => new RequestParameter(r.Name ?? string.Empty) {
            Id = r.Id,
            DisplayName = r.DisplayName ?? string.Empty,
            UniqueName = r.UniqueName ?? string.Empty,
            LogicalEntityName = r.LogicalEntityName ?? string.Empty,
            Type = (CustomApiParameterType)(r.Type ?? 0),
            IsOptional = r.IsOptional ?? false,
            IsCustomizable = r.IsCustomizable?.Value ?? false
        });

        var resps = reader.RetrieveByColumn<CustomApiResponseProperty>(
            r => r.CustomApiId,
            [.. data.Select(d => d.Id).Distinct()],
            r => r.Name,
            r => r.DisplayName,
            r => r.UniqueName,
            r => r.LogicalEntityName,
            r => r.Type,
            r => r.IsCustomizable,
            r => r.CustomApiId
        ).ToLookup(r => r.CustomApiId?.Id ?? Guid.Empty, r => new ResponseProperty(r.Name ?? string.Empty) {
            Id = r.Id,
            DisplayName = r.DisplayName ?? string.Empty,
            UniqueName = r.UniqueName ?? string.Empty,
            LogicalEntityName = r.LogicalEntityName ?? string.Empty,
            Type = (CustomApiParameterType)(r.Type ?? 0),
            IsCustomizable = r.IsCustomizable?.Value ?? false
        });

        // Enrich data with PluginTypes, RequestParameters, and ResponseProperties
        return [.. data.Select(api =>
        {
            return api with
            {
                PluginType = pluginTypeNames.TryGetValue(api.PluginType.Id, out var pluginTypeName) ? api.PluginType with { Name = pluginTypeName } : api.PluginType,
                RequestParameters = [.. reqs[api.Id]],
                ResponseProperties = [.. resps[api.Id]]
            };
        })];
    }
}