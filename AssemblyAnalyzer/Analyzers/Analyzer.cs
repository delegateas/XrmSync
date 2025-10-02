using XrmPluginCore.Enums;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

internal abstract class Analyzer
{
    protected static T? GetRegistrationFromType<T>(string methodName, Type pluginType) where T : class
    {
        var getRegistrationMethod = pluginType.GetMethod(methodName)
            ?? throw new AnalysisException($"Type {pluginType.FullName} does not have a {methodName} method");

        var instance = Activator.CreateInstance(pluginType)
            ?? throw new AnalysisException($"Failed to create instance of type {pluginType.FullName}");

        return getRegistrationMethod.Invoke(instance, null) as T;
    }

    protected static string StepName(string className, ExecutionMode executionMode, ExecutionStage executionStage, string eventOperation, string? entityLogicalName)
    {
        var entity = string.IsNullOrEmpty(entityLogicalName) ? "any Entity" : entityLogicalName;

        var executionModeName = executionMode.ToString();
        var executionStageName = executionStage switch
        {
            ExecutionStage.PreValidation => "PreValidation",
            ExecutionStage.PreOperation => "Pre",
            ExecutionStage.PostOperation => "Post",
            _ => "Unknown"
        };

        return $"{className}: {executionModeName} {executionStageName} {eventOperation} of {entity}";
    }
}
