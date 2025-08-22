using DG.XrmPluginCore.Enums;
using XrmSync.Model.CustomApi;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

// ExtendedCustomAPIConfig  : PluginType, OwnerId, OwnerType, IsCustomizable, IsPrivate, ExecutePrivilegeName, Description
using ExtendedCustomAPIConfig = Tuple<string?, string?, string?, bool, bool, string?, string?>;
// MainCustomAPIConfig      : UniqueName, IsFunction, EnabledForWorkflow, AllowedCustomProcessingStepType, BindingType, BoundEntityLogicalName
using MainCustomAPIConfig = Tuple<string?, bool, int, int, int, string?>;
// RequestParameterConfig   : Name, UniqueName, DisplayName, IsCustomizable, IsOptional, LogicalEntityName, Type
using RequestParameterConfig = Tuple<string?, string?, string?, bool, bool, string?, int>;
// ResponsePropertyConfig   : Name, UniqueName, DisplayName, IsCustomizable, LogicalEntityName, Type
using ResponsePropertyConfig = Tuple<string?, string?, string?, bool, string?, int>;

internal class DAXIFCustomApiAnalyzer : Analyzer, ICustomApiAnalyzer
{
    public List<CustomApiDefinition> GetCustomApis(IEnumerable<Type> types, string prefix)
    {
        var customApiType = types.FirstOrDefault(x => x.Name == "CustomAPI");
        if (customApiType == null)
            return [];

        const string MethodName = "GetCustomAPIConfig";

        if (customApiType.GetMethod(MethodName) is null)
            return [];

        var customApiTypes = types.Where(x => x.IsSubclassOf(customApiType) && !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null);
        return [..customApiTypes
        .Select(pluginType =>
        {
            var (apiDef, apiMeta, reqParams, resProps) = GetRegistrationFromType<Tuple<MainCustomAPIConfig, ExtendedCustomAPIConfig, IEnumerable<RequestParameterConfig>, IEnumerable<ResponsePropertyConfig>>>(MethodName, pluginType);

            var (pluginTypeName, isFunction, enabledForWorkflow, allowedCustomProcessingStepType, bindingType, boundLogicalEntityName) = apiDef;
            var (_, ownerIdStr, _, isCustomizable, isPrivate, executePrivilegeName, description) = apiMeta;

            pluginTypeName ??= string.Empty;

            var definition = new CustomApiDefinition
            {
                PluginType = new PluginType { Name = pluginTypeName },

                UniqueName = prefix + "_" + pluginTypeName,
                Name = pluginTypeName,
                DisplayName = pluginTypeName, // No explicit display name in tuple, fallback to name
                
                IsFunction = isFunction,
                EnabledForWorkflow = enabledForWorkflow == 1,
                AllowedCustomProcessingStepType = (AllowedCustomProcessingStepType)allowedCustomProcessingStepType,
                BindingType = (BindingType)bindingType,
                BoundEntityLogicalName = boundLogicalEntityName ?? string.Empty,

                OwnerId = Guid.TryParse(ownerIdStr, out var ownerId) ? ownerId : Guid.Empty,
                IsCustomizable = isCustomizable,
                IsPrivate = isPrivate,
                ExecutePrivilegeName = executePrivilegeName ?? string.Empty,
                Description = description ?? string.Empty,


            };

            definition.RequestParameters = reqParams?.Select(p => new RequestParameter
            {
                CustomApi = definition,
                Name = p.Item1 ?? string.Empty,
                UniqueName = p.Item2 ?? string.Empty,
                DisplayName = p.Item3 ?? string.Empty,
                IsCustomizable = p.Item4,
                IsOptional = p.Item5,
                LogicalEntityName = p.Item6 ?? string.Empty,
                Type = (CustomApiParameterType)p.Item7
            }).ToList() ?? [];

            definition.ResponseProperties = resProps?.Select(r => new ResponseProperty
            {
                CustomApi = definition,
                Name = r.Item1 ?? string.Empty,
                UniqueName = r.Item2 ?? string.Empty,
                DisplayName = r.Item3 ?? string.Empty,
                IsCustomizable = r.Item4,
                LogicalEntityName = r.Item5 ?? string.Empty,
                Type = (CustomApiParameterType)r.Item6
            }).ToList() ?? [];

            return definition;
        })];
    }
}
