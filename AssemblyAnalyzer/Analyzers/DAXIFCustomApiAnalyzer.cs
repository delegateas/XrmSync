using XrmSync.Model.CustomApi;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

// MainCustomAPIConfig      : UniqueName, IsFunction, EnabledForWorkflow, AllowedCustomProcessingStepType, BindingType, BoundEntityLogicalName
using MainCustomAPIConfig = Tuple<string?, bool, int, int, int, string?>;
// ExtendedCustomAPIConfig  : PluginType, OwnerId, OwnerType, IsCustomizable, IsPrivate, ExecutePrivilegeName, Description
using ExtendedCustomAPIConfig = Tuple<string?, string?, string?, bool, bool, string?, string?>;
// RequestParameterConfig   : Name, UniqueName, DisplayName, IsCustomizable, IsOptional, LogicalEntityName, Type
using RequestParameterConfig = Tuple<string?, string?, string?, bool, bool, string?, int>;
// ResponsePropertyConfig   : Name, UniqueName, DisplayName, IsCustomizable, LogicalEntityName, Type
using ResponsePropertyConfig = Tuple<string?, string?, string?, bool, string?, int>;
internal class DAXIFCustomApiAnalyzer : ICustomApiAnalyzer
{
    public List<CustomApiDefinition> GetCustomApis(IEnumerable<Type> types)
    {
        var customApiType = types.FirstOrDefault(x => x.Name == "CustomAPI");
        if (customApiType == null)
            return [];

        if (customApiType.GetMethod("PluginProcessingStepConfigs") is null)
            return [];

        var customApiTypes = types.Where(x => x.IsSubclassOf(customApiType) && !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null);
        return [..customApiTypes
        .Select(x =>
        {
            var instance = Activator.CreateInstance(x);
            var methodInfo = x.GetMethod("GetCustomAPIConfig") ?? throw new AnalysisException($"CustomAPI type '{x.Name}' does not have a GetCustomAPIConfig method.");
            var result = methodInfo.Invoke(instance, null) ?? throw new AnalysisException($"GetCustomAPIConfig returned null for type '{x.Name}'.");

            var tuple = (Tuple<
                MainCustomAPIConfig,
                ExtendedCustomAPIConfig,
                IEnumerable<RequestParameterConfig>,
                IEnumerable<ResponsePropertyConfig>
            >)result;

            var apiDef = tuple.Item1;
            var apiMeta = tuple.Item2;
            var reqParams = tuple.Item3;
            var resProps = tuple.Item4;

            return new CustomApiDefinition
            {
                UniqueName = apiDef.Item1 ?? string.Empty,
                Name = apiDef.Item1 ?? string.Empty,
                IsFunction = apiDef.Item2,
                EnabledForWorkflow = apiDef.Item3 == 1,
                AllowedCustomProcessingStepType = apiDef.Item4,
                BindingType = apiDef.Item5,
                BoundEntityLogicalName = apiDef.Item6 ?? string.Empty,

                PluginTypeName = apiMeta.Item1 ?? string.Empty,
                OwnerId = Guid.TryParse(apiMeta.Item2, out var ownerId) ? ownerId : Guid.Empty,
                IsCustomizable = apiMeta.Item4,
                IsPrivate = apiMeta.Item5,
                ExecutePrivilegeName = apiMeta.Item6 ?? string.Empty,
                Description = apiMeta.Item7 ?? string.Empty,
                DisplayName = apiDef.Item1 ?? string.Empty, // No explicit display name in tuple, fallback to name

                RequestParameters = reqParams?.Select(p => new RequestParameter
                {
                    Name = p.Item1 ?? string.Empty,
                    UniqueName = p.Item2 ?? string.Empty,
                    DisplayName = p.Item3 ?? string.Empty,
                    IsCustomizable = p.Item4,
                    IsOptional = p.Item5,
                    LogicalEntityName = p.Item6 ?? string.Empty,
                    Type = p.Item7,
                    CustomApiName = apiDef.Item1 ?? string.Empty
                }).ToList() ?? [],

                ResponseProperties = resProps?.Select(r => new ResponseProperty
                {
                    Name = r.Item1 ?? string.Empty,
                    UniqueName = r.Item2 ?? string.Empty,
                    DisplayName = r.Item3 ?? string.Empty,
                    IsCustomizable = r.Item4,
                    LogicalEntityName = r.Item5 ?? string.Empty,
                    Type = r.Item6,
                    CustomApiName = apiDef.Item1 ?? string.Empty
                }).ToList() ?? []
            };
        })];
    }
}
