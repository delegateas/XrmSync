using DG.XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

// StepConfig           : className, ExecutionStage, EventOperation, LogicalName
using StepConfig = Tuple<string?, int, string?, string?>;
// ExtendedStepConfig   : Deployment, ExecutionMode, Name, ExecutionOrder, FilteredAttributes, UserContext
using ExtendedStepConfig = Tuple<int, int, string?, int, string?, string?>;
// ImageTuple           : Name, EntityAlias, ImageType, Attributes
using ImageTuple = Tuple<string?, string?, int, string?>;

internal class DAXIFPluginAnalyzer : Analyzer, IPluginAnalyzer
{
    public List<PluginDefinition> GetPluginDefinitions(IEnumerable<Type> types)
    {
        var pluginBaseType = types.FirstOrDefault(x => x.Name == "Plugin");
        if (pluginBaseType == null)
        {
            return [];
        }

        // Check if the plugin base type is valid
        const string MethodName = "PluginProcessingStepConfigs";
        if (pluginBaseType.GetMethod(MethodName) is null)
        {
            return [];
        }

        static bool IsValid(Type x) => !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null;

        var plugins = types.Where(x => x.IsSubclassOf(pluginBaseType));
        var validPlugins = plugins.Where(IsValid);

        foreach (var plugin in plugins.Where(x => !IsValid(x)))
        {
            if (plugin.IsAbstract)
                throw new AnalysisException($"The plugin '{plugin.Name}' is an abstract type and is therefore not valid. The plugin will not be synchronized");
            if (plugin.GetConstructor(Type.EmptyTypes) == null)
                throw new AnalysisException($"The plugin '{plugin.Name}' does not contain an empty contructor and is therefore not valid. The plugin will not be synchronized");
        }

        return [.. validPlugins
            .SelectMany(pluginType =>
            {
                var pluginTuples = GetRegistrationFromType<IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>>(MethodName, pluginType);

                return pluginTuples
                    .Select(tuple =>
                    {
                        var (className, stage, eventOp, logicalName) = tuple.Item1;
                        var (deployment, mode, notUsedStepname, executionOrder, filteredAttr, userIdStr) = tuple.Item2;
                        var imageTuples = tuple.Item3;

                        var stepName = StepName(className ?? string.Empty, (ExecutionMode)mode, (ExecutionStage)stage, eventOp ?? string.Empty, logicalName);

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
                                    ImageType = (ImageType)iType,
                                    Attributes = iAttr ?? string.Empty
                                };
                            })
                            .ToList();

                        return new Step
                        {
                            ExecutionStage = (ExecutionStage)stage,
                            Deployment = (Deployment)deployment,
                            ExecutionMode = (ExecutionMode)mode,
                            ExecutionOrder = executionOrder,
                            FilteredAttributes = filteredAttr ?? string.Empty,
                            UserContext = Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty,
                            PluginTypeName = className ?? string.Empty,
                            Name = stepName,
                            EventOperation = eventOp ?? string.Empty,
                            LogicalName = logicalName ?? string.Empty,
                            AsyncAutoDelete = false,
                            PluginImages = images,
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
