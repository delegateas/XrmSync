using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.Model.Plugin;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

// StepConfig           : className, ExecutionStage, EventOperation, LogicalName
using StepConfig = Tuple<string?, int, string?, string?>;
// ExtendedStepConfig   : Deployment, ExecutionMode, Name, ExecutionOrder, FilteredAttributes, UserContext
using ExtendedStepConfig = Tuple<int, int, string?, int, string?, string?>;
// ImageTuple           : Name, EntityAlias, ImageType, Attributes
using ImageTuple = Tuple<string?, string?, int, string?>;

internal class DAXIFPluginAnalyzer(ILogger logger) : IPluginAnalyzer
{
    public List<PluginDefinition> GetPluginDefinitions(IEnumerable<Type> types)
    {
        var pluginType = types.FirstOrDefault(x => x.Name == "Plugin");
        if (pluginType == null)
        {
            return [];
        }

        static bool IsValid(Type pluginType) => !pluginType.IsAbstract && pluginType.GetConstructor(Type.EmptyTypes) != null;

        var plugins = types.Where(x => x.IsSubclassOf(pluginType));
        var validPlugins = plugins.Where(IsValid);

        foreach (var plugin in plugins.Where(x => !IsValid(x)))
        {
            if (plugin.IsAbstract)
                logger.LogError("The plugin '{pluginName}' is an abstract type and is therefore not valid. The plugin will not be synchronized", plugin.Name);
            if (plugin.GetConstructor(Type.EmptyTypes) == null)
                logger.LogError("The plugin '{pluginName}' does not contain an empty contructor and is therefore not valid. The plugin will not be synchronized", plugin.Name);
        }

        return [.. validPlugins
            .SelectMany(x =>
            {
                var instance = Activator.CreateInstance(x);
                var methodInfo = x.GetMethod("PluginProcessingStepConfigs") ?? throw new AnalysisException($"Plugin type '{x.Name}' does not have a PluginProcessingStepConfigs method.");
                var result = methodInfo.Invoke(instance, null) ?? throw new AnalysisException($"PluginProcessingStepConfigs returned null for type '{x.Name}'.");
                var pluginTuples = (IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>)result;

                return pluginTuples
                    .Select(tuple =>
                    {
                        var (className, stage, eventOp, logicalName) = tuple.Item1;
                        var (deployment, mode, notUsedStepname, executionOrder, filteredAttr, userIdStr) = tuple.Item2;
                        var imageTuples = tuple.Item3;

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
                            })
                            .ToList();

                        return new Step
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
                    })
                    .GroupBy(s => s.PluginTypeName)
                    .Select(p => new PluginDefinition
                    {
                        Name = p.Key,
                        PluginSteps = [.. p],
                    });
            })];
    }
}
