using DG.XrmPluginSync.Model;
using System.Reflection;
using ExtendedStepConfig = System.Tuple<int, int, string, int, string, string>;
using ImageTuple = System.Tuple<string, string, int, string>;
using StepConfig = System.Tuple<string, int, string, string>;

namespace DG.XrmPluginSync.AssemblyAnalyzer;

internal static class AssemblyAnalyzer
{
    public static PluginAssembly GetPluginAssembly(string dllPath)
    {
        var dllFullPath = Path.GetFullPath(dllPath);

        var dllTempPath = dllPath;
        var dllname = Path.GetFileNameWithoutExtension(dllPath);
        var hash = CryptographyUtility.Sha1Checksum(File.ReadAllBytes(dllTempPath));

        var assembly = Assembly.LoadFrom(dllTempPath);
        var assemblyVersion = assembly.GetName()?.Version?.ToString() ?? throw new InvalidOperationException("Could not determine assembly version");
        var pluginTypes = GetPluginTypesFromAssembly(assembly);

        return new PluginAssembly
        {
            Name = dllname,
            Version = assemblyVersion,
            Hash = hash,
            DllPath = dllFullPath,
            PluginTypes = pluginTypes,
        };
    }

    private static List<PluginTypeEntity> GetPluginTypesFromAssembly(Assembly assembly)
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
            var methodInfo = x.GetMethod(@"PluginProcessingStepConfigs");
            var pluginTuples = (IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>)methodInfo.Invoke(instance, null);
            return pluginTuples
                .Select(tuple =>
                {
                    var (className, stage, eventOp, logicalName) = tuple.Item1;
                    var (deployment, mode, notUsedStepname, executionOrder, filteredAttr, userId) = tuple.Item2;
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

                            return new PluginImageEntity
                            {
                                PluginStepName = stepName,
                                Name = iName,
                                EntityAlias = iAlias,
                                ImageType = iType,
                                Attributes = iAttr,
                                EventOperation = eventOp,
                            };
                        }).ToList();

                    var step = new PluginStepEntity
                    {
                        ExecutionStage = stage,
                        Deployment = deployment,
                        ExecutionMode = mode,
                        ExecutionOrder = executionOrder,
                        FilteredAttributes = filteredAttr,
                        UserContext = new Guid(userId),
                        PluginTypeName = className,
                        Name = stepName,
                        PluginImages = images,

                        EventOperation = eventOp,
                        LogicalName = logicalName,
                    };

                    return new PluginTypeEntity
                    {
                        Name = className,
                        PluginSteps = [step],
                    };
                });
        }).ToList();

        return pluginTypes;
    }
}