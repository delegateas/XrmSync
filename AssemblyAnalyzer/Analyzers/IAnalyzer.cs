namespace XrmSync.AssemblyAnalyzer.Analyzers;

public interface IAnalyzer<T>
{
    List<T> AnalyzeTypes(IEnumerable<Type> types, string prefix);
}
