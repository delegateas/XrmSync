using DG.XrmPluginSync.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssemblyAnalyzer
{
    using StepConfig = System.Tuple<string, int, string, string>;
    using ExtendedStepConfig = System.Tuple<int, int, string, int, string, string>;
    using ImageTuple = System.Tuple<string, string, int, string>;

    public enum ExecutionMode
    {
        Synchronous,
        Asynchronous
    }

    public enum ExecutionStage
    {
        PreValidation = 10,
        Pre = 20,
        Post = 40
    }

    public enum ImageType
    {
        PreImage = 0,
        PostImage = 1,
        Both = 2
    }

    public static class Program
    {
        static int Main(string[] args)
        {
            var assemblyLocation = Path.GetFullPath(args[0]);

            if (!File.Exists(assemblyLocation))
            {
                Console.Error.WriteLine($"Assembly not found at {assemblyLocation}");
                return 1;
            }

            if (!Path.GetExtension(assemblyLocation).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine($"Invalid assembly file type: {Path.GetExtension(assemblyLocation)}, expected DLL");
                return 1;
            }

            var pluginDto = GetPluginAssembly(assemblyLocation);
            var jsonOutput = JsonConvert.SerializeObject(pluginDto, Formatting.Indented);
            Console.WriteLine(jsonOutput);

            return 0;
        }

        private static PluginAssembly GetPluginAssembly(string dllPath)
        {
            var dllFullPath = Path.GetFullPath(dllPath);
            var assemblyWriteTime = File.GetLastWriteTimeUtc(dllFullPath);

            //var dllTempPath = FileUtility.CopyFileToTempPath(dllPath, ".dll");
            var dllTempPath = dllPath;
            var dllname = Path.GetFileNameWithoutExtension(dllPath);
            var hash = CryptographyUtility.Sha1Checksum(File.ReadAllBytes(dllTempPath));

            var assembly = Assembly.LoadFrom(dllTempPath);
            var pluginTypes = GetPluginTypesFromAssembly(assembly);

            return new PluginAssembly
            {
                Name = dllname,
                Version = assembly.GetName().Version.ToString(),
                Hash = hash,
                DllPath = dllFullPath,
                PluginTypes = pluginTypes,
            };
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        private static List<PluginTypeEntity> GetPluginTypesFromAssembly(Assembly assembly)
        {
            var types = GetLoadableTypes(assembly);
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
                        // Replace the deconstruction with explicit access to tuple elements
                        var className = tuple.Item1.Item1;
                        var stage = tuple.Item1.Item2;
                        var eventOp = tuple.Item1.Item3;
                        var logicalName = tuple.Item1.Item4;

                        var deployment = tuple.Item2.Item1;
                        var mode = tuple.Item2.Item2;
                        var notUsedStepname = tuple.Item2.Item3;
                        var executionOrder = tuple.Item2.Item4;
                        var filteredAttr = tuple.Item2.Item5;
                        var userId = tuple.Item2.Item6;
                        List<ImageTuple> imageTuples = tuple.Item3.ToList();

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
                                    Id = Guid.Empty,
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
                            Id = Guid.Empty,
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
                            Id = Guid.Empty,
                            Name = className,
                            PluginSteps = new List<PluginStepEntity> { step },
                        };
                    });
            }).ToList();

            return pluginTypes;
        }
    }
}
