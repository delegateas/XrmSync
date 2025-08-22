using XrmSync.Model;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse.Interfaces;

public interface IPluginReader
{
    AssemblyInfo? GetPluginAssembly(Guid solutionId, string assemblyName);
    List<PluginDefinition> GetPluginTypes(Guid assemblyId);
    List<Step> GetPluginSteps(IEnumerable<PluginDefinition> pluginTypes, Guid solutionId);
    IEnumerable<Step> GetMissingUserContexts(IEnumerable<Step> pluginSteps);
}