using System.Reflection;
using XrmSync.Model;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

internal abstract class Analyzer
{
    protected static T GetRegistrationFromType<T>(string methodName, Type pluginType) where T : class
    {
        var getRegistrationMethod = pluginType.GetMethod(methodName)
            ?? throw new AnalysisException($"Type {pluginType.FullName} does not have a {methodName} method");

        var instance = Activator.CreateInstance(pluginType)
            ?? throw new AnalysisException($"Failed to create instance of type {pluginType.FullName}");

        return getRegistrationMethod.Invoke(instance, null) as T
            ?? throw new AnalysisException($"{methodName}() returned null for type {pluginType.FullName}");
    }

    protected static string StepName(string className, int executionMode, int executionStage, string eventOperation, string? entityLogicalName)
    {
        var entity = string.IsNullOrEmpty(entityLogicalName) ? "any Entity" : entityLogicalName;
        return $"{className}: {Enum.GetName(typeof(ExecutionMode), executionMode)} {Enum.GetName(typeof(ExecutionStage), executionStage)} {eventOperation} of {entity}";
    }
}
