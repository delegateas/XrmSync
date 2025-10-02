using XrmPluginCore.Enums;
using System;
using XrmSync.Model.Plugin;

namespace XrmSync.AssemblyAnalyzer.Analyzers.DAXIF;

// ExtendedStepConfig   : Deployment, ExecutionMode, Name, ExecutionOrder, FilteredAttributes, UserContext
using ExtendedStepConfig = Tuple<int, int, string?, int, string?, string?>;
// ImageTuple           : Name, EntityAlias, ImageType, Attributes
using ImageTuple = Tuple<string?, string?, int, string?>;
// StepConfig           : className, ExecutionStage, EventOperation, LogicalName
using StepConfig = Tuple<string?, int, string?, string?>;

internal class DAXIFPluginAnalyzer : Analyzer, IAnalyzer<PluginDefinition>
{
    public List<PluginDefinition> AnalyzeTypes(IEnumerable<Type> types, string prefix)
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
            .Select(pluginType => {
                var pluginTuples =
                    GetRegistrationFromType<IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>>(MethodName, pluginType)
                    ?? throw new AnalysisException($"{MethodName}() returned null for type {pluginType.FullName}");

                return new PluginDefinition {
                    Name = pluginType.FullName ?? string.Empty,
                    PluginSteps = [.. GetSteps(pluginTuples)]
                };
            })];
    }

    private static IEnumerable<Step> GetSteps(IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>> pluginTuples)
    {
        return pluginTuples.Select(tuple =>
        {
            var (className, stage, eventOp, logicalName) = tuple.Item1;
            var (deployment, mode, notUsedStepname, executionOrder, filteredAttr, userIdStr) = tuple.Item2;
            var stepName = StepName(className ?? string.Empty, (ExecutionMode)mode, (ExecutionStage)stage, eventOp ?? string.Empty, logicalName);

            return new Step
            {
                Name = stepName,
                ExecutionStage = (ExecutionStage)stage,
                Deployment = (Deployment)deployment,
                ExecutionMode = (ExecutionMode)mode,
                ExecutionOrder = executionOrder,
                FilteredAttributes = filteredAttr ?? string.Empty,
                UserContext = Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty,
                EventOperation = eventOp ?? string.Empty,
                LogicalName = logicalName ?? string.Empty,
                AsyncAutoDelete = false,
                PluginImages = [.. GetImages(tuple.Item3)]
            };
        });
    }

    private static IEnumerable<Image> GetImages(IEnumerable<ImageTuple> imageTuples)
    {
        return imageTuples
            .Select(image =>
            {
                var (iName, iAlias, iType, iAttr) = image;

                return new Image
                {
                    Name = iName ?? string.Empty,
                    EntityAlias = iAlias ?? string.Empty,
                    ImageType = (ImageType)iType,
                    Attributes = iAttr ?? string.Empty
                };
            });
    }
}
