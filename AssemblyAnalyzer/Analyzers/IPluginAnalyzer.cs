using XrmSync.Model.Plugin;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

public interface IPluginAnalyzer
{
    List<PluginDefinition> GetPluginDefinitions(IEnumerable<Type> types);
}
