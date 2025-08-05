using XrmSync.Model.CustomApi;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

public interface ICustomApiAnalyzer
{
    List<CustomApiDefinition> GetCustomApis(IEnumerable<Type> types);
}
