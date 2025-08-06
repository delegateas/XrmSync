using XrmSync.Model;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

internal abstract class Analyzer
{
    protected static string StepName(string className, int executionMode, int executionStage, string eventOperation, string? entityLogicalName)
    {
        var entity = string.IsNullOrEmpty(entityLogicalName) ? "any Entity" : entityLogicalName;
        return $"{className}: {Enum.GetName(typeof(ExecutionMode), executionMode)} {Enum.GetName(typeof(ExecutionStage), executionStage)} {eventOperation} of {entity}";
    }
}
