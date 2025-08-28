using DG.XrmPluginCore.Enums;
using System.Xml.Linq;
using XrmSync.Model.CustomApi;

namespace XrmSync.AssemblyAnalyzer.Analyzers.DAXIF;

// ExtendedCustomAPIConfig  : PluginType, OwnerId, OwnerType, IsCustomizable, IsPrivate, ExecutePrivilegeName, Description
using ExtendedCustomAPIConfig = Tuple<string?, string?, string?, bool, bool, string?, string?>;
// MainCustomAPIConfig      : UniqueName, IsFunction, EnabledForWorkflow, AllowedCustomProcessingStepType, BindingType, BoundEntityLogicalName
using MainCustomAPIConfig = Tuple<string?, bool, int, int, int, string?>;
// RequestParameterConfig   : Name, UniqueName, DisplayName, IsCustomizable, IsOptional, LogicalEntityName, Type
using RequestParameterConfig = Tuple<string?, string?, string?, bool, bool, string?, int>;
// ResponsePropertyConfig   : Name, UniqueName, DisplayName, IsCustomizable, LogicalEntityName, Type
using ResponsePropertyConfig = Tuple<string?, string?, string?, bool, string?, int>;

internal class DAXIFCustomApiAnalyzer : Analyzer, IAnalyzer<CustomApiDefinition>
{
    public List<CustomApiDefinition> AnalyzeTypes(IEnumerable<Type> types, string prefix)
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
            var (mainConfig, extendedConfig, reqParams, resProps) = GetRegistrationFromType<Tuple<MainCustomAPIConfig, ExtendedCustomAPIConfig, IEnumerable<RequestParameterConfig>, IEnumerable<ResponsePropertyConfig>>>(MethodName, pluginType);

            var (UniqueName, IsFunction, EnabledForWorkflow, AllowedCustomProcessingStepType, BindingType, BoundEntityLogicalName) = mainConfig;
            var (PluginType, OwnerId, OwnerType, IsCustomizable, IsPrivate, ExecutePrivilegeName, Description) = extendedConfig;

            PluginType ??= string.Empty;

            return new CustomApiDefinition
            {
                PluginType = new PluginType { Name = PluginType },

                UniqueName = prefix + "_" + UniqueName,
                Name = UniqueName ?? string.Empty,
                DisplayName = UniqueName ?? string.Empty, // No explicit display name in tuple, fallback to name
                
                IsFunction = IsFunction,
                EnabledForWorkflow = EnabledForWorkflow == 1,
                AllowedCustomProcessingStepType = (AllowedCustomProcessingStepType)AllowedCustomProcessingStepType,
                BindingType = (BindingType)BindingType,
                BoundEntityLogicalName = BoundEntityLogicalName ?? string.Empty,

                OwnerId = Guid.TryParse(OwnerId, out var ownerId) ? ownerId : Guid.Empty,
                IsCustomizable = IsCustomizable,
                IsPrivate = IsPrivate,
                ExecutePrivilegeName = ExecutePrivilegeName ?? string.Empty,
                Description = Description ?? string.Empty,

                RequestParameters = GetRequestParameters(reqParams),
                ResponseProperties = GetResponseProperties(resProps)
            };
        })];
    }

    private static List<RequestParameter> GetRequestParameters(IEnumerable<RequestParameterConfig> reqParams)
    {
        return reqParams?.Select(p =>
        {
            var (Name, UniqueName, DisplayName, IsCustomizable, IsOptional, LogicalEntityName, Type) = p;
            return new RequestParameter
            {
                Name = Name ?? string.Empty,
                UniqueName = UniqueName ?? string.Empty,
                DisplayName = DisplayName ?? string.Empty,
                IsCustomizable = IsCustomizable,
                IsOptional = IsOptional,
                LogicalEntityName = LogicalEntityName ?? string.Empty,
                Type = (CustomApiParameterType)p.Item7
            };
        }).ToList() ?? [];
    }

    private static List<ResponseProperty> GetResponseProperties(IEnumerable<ResponsePropertyConfig> resProps)
    {
        return resProps?.Select(r => {
            var (Name, UniqueName, DisplayName, IsCustomizable, LogicalEntityName, Type) = r;
            return new ResponseProperty
            {
                Name = Name ?? string.Empty,
                UniqueName = UniqueName ?? string.Empty,
                DisplayName = DisplayName ?? string.Empty,
                IsCustomizable = IsCustomizable,
                LogicalEntityName = LogicalEntityName ?? string.Empty,
                Type = (CustomApiParameterType)Type
            };
        }).ToList() ?? [];
    }
}
