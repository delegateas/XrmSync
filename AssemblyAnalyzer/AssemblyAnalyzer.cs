using DG.XrmPluginSync.Model;
using System.Reflection;

// StepConfig           : className, ExecutionStage, EventOperation, LogicalName
using StepConfig = System.Tuple<string?, int, string?, string?>;
// ExtendedStepConfig   : Deployment, ExecutionMode, Name, ExecutionOrder, FilteredAttributes, UserContext
using ExtendedStepConfig = System.Tuple<int, int, string?, int, string?, string?>;
// ImageTuple           : Name, EntityAlias, ImageType, Attributes
using ImageTuple = System.Tuple<string?, string?, int, string?>;

// MainCustomAPIConfig      : UniqueName, IsFunction, EnabledForWorkflow, AllowedCustomProcessingStepType, BindingType, BoundEntityLogicalName
using MainCustomAPIConfig = System.Tuple<string?, bool, int, int, int, string?>;
// ExtendedCustomAPIConfig  : PluginType, OwnerId, OwnerType, IsCustomizable, IsPrivate, ExecutePrivilegeName, Description
using ExtendedCustomAPIConfig = System.Tuple<string?, string?, string?, bool, bool, string?, string?>;
// RequestParameterConfig   : Name, UniqueName, DisplayName, IsCustomizable, IsOptional, LogicalEntityName, Type
using RequestParameterConfig = System.Tuple<string?, string?, string?, bool, bool, string?, int>; // TODO: Add description maybe
// ResponsePropertyConfig   : Name, UniqueName, DisplayName, IsCustomizable, LogicalEntityName, Type
using ResponsePropertyConfig = System.Tuple<string?, string?, string?, bool, string?, int>;
using DG.XrmPluginSync.Model.Plugin;
using DG.XrmPluginSync.Model.CustomApi; // TODO

namespace DG.XrmPluginSync.AssemblyAnalyzer;

internal static class AssemblyAnalyzer
{
    public static AssemblyInfo GetPluginAssembly(string dllPath)
    {
        var dllFullPath = Path.GetFullPath(dllPath);

        var dllTempPath = dllPath;
        var dllname = Path.GetFileNameWithoutExtension(dllPath);
        var hash = File.ReadAllBytes(dllTempPath).Sha1Checksum();

        var assembly = Assembly.LoadFrom(dllTempPath);
        var assemblyVersion = assembly.GetName()?.Version?.ToString() ?? throw new InvalidOperationException("Could not determine assembly version");
        var pluginDefinitions = GetPluginTypesFromAssembly(assembly);
        var customApis = GetCustomApisFromAssembly(assembly);

        return new AssemblyInfo
        {
            Name = dllname,
            Version = assemblyVersion,
            Hash = hash,
            DllPath = dllFullPath,
            Plugins = pluginDefinitions,
            CustomApis = customApis,
        };
    }

    private static List<ApiDefinition> GetCustomApisFromAssembly(Assembly assembly)
    {
        var types = assembly.GetLoadableTypes();
        var customApiType = types.FirstOrDefault(x => x.Name == "CustomAPI");
        if (customApiType == null)
            return [];

        var customApiEntities = new List<ApiDefinition>();

        foreach (var x in types.Where(x => x.IsSubclassOf(customApiType) && !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null))
        {
            var instance = Activator.CreateInstance(x);
            var methodInfo = x.GetMethod("GetCustomAPIConfig") ?? throw new InvalidOperationException($"CustomAPI type '{x.Name}' does not have a GetCustomAPIConfig method.");
            var result = methodInfo.Invoke(instance, null) ?? throw new InvalidOperationException($"GetCustomAPIConfig returned null for type '{x.Name}'.");

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

            var entity = new ApiDefinition
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

            customApiEntities.Add(entity);
        }
        return customApiEntities;
    }

    private static List<PluginDefinition> GetPluginTypesFromAssembly(Assembly assembly)
    {
        var types = assembly.GetLoadableTypes();
        var pluginType = types.First(x => x.Name == "Plugin");
        var plugins = types.Where(x => x.IsSubclassOf(pluginType));
        var validPlugins = plugins.Where(x => !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null);
        var invalidPlugins = plugins.Where(x => !(!x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null));
        foreach (var plugin in invalidPlugins)
        {
            if (plugin.IsAbstract)
                Console.Error.WriteLine($"The plugin '{plugin.Name}' is an abstract type and is therefore not valid. The plugin will not be synchronized");
            //log.LogInformation($"The plugin '{plugin.Name}' is an abstract type and is therefore not valid. The plugin will not be synchronized");
            if (plugin.GetConstructor(Type.EmptyTypes) == null)
                Console.Error.WriteLine($"The plugin '{plugin.Name}' does not contain an empty contructor and is therefore not valid. The plugin will not be synchronized");
            //log.LogInformation($"The plugin '{plugin.Name}' does not contain an empty contructor and is therefore not valid. The plugin will not be synchronized");
        }

        var pluginTypes = validPlugins
        .SelectMany(x =>
        {
            var instance = Activator.CreateInstance(x);
            var methodInfo = x.GetMethod("PluginProcessingStepConfigs");
            if (methodInfo == null)
                throw new InvalidOperationException($"Plugin type '{x.Name}' does not have a PluginProcessingStepConfigs method.");

            var result = methodInfo.Invoke(instance, null);
            if (result == null)
                throw new InvalidOperationException($"PluginProcessingStepConfigs returned null for type '{x.Name}'.");

            var pluginTuples = (IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>)result;
            return pluginTuples
                .Select(tuple =>
                {
                    var (className, stage, eventOp, logicalName) = tuple.Item1;
                    var (deployment, mode, notUsedStepname, executionOrder, filteredAttr, userIdStr) = tuple.Item2;
                    var imageTuples = tuple.Item3.ToList();

                    var entity = string.IsNullOrEmpty(logicalName) ? "any Entity" : logicalName;
                    var stepName = $"{className}: {Enum.GetName(typeof(ExecutionMode), mode)} {Enum.GetName(typeof(ExecutionStage), stage)} {eventOp} of {entity}";

                    var images = imageTuples
                        .Select(image =>
                        {
                            // Replace the deconstruction with explicit access to tuple elements
                            var iName = image.Item1;
                            var iAlias = image.Item2;
                            var iType = image.Item3;
                            var iAttr = image.Item4;

                            return new Image
                            {
                                PluginStepName = stepName,
                                Name = iName ?? string.Empty,
                                EntityAlias = iAlias ?? string.Empty,
                                ImageType = iType,
                                Attributes = iAttr ?? string.Empty
                            };
                        }).ToList();

                    var step = new Step
                    {
                        ExecutionStage = stage,
                        Deployment = deployment,
                        ExecutionMode = mode,
                        ExecutionOrder = executionOrder,
                        FilteredAttributes = filteredAttr ?? string.Empty,
                        UserContext = Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty,
                        PluginTypeName = className ?? string.Empty,
                        Name = stepName,
                        PluginImages = images,

                        EventOperation = eventOp ?? string.Empty,
                        LogicalName = logicalName ?? string.Empty,
                    };

                    return new PluginDefinition
                    {
                        Name = className ?? string.Empty,
                        PluginSteps = [step],
                    };
                });
        }).ToList();

        return pluginTypes;
    }
}
