using XrmSync.Model;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse.Interfaces;

public interface IPluginReader
{
    AssemblyInfo? GetPluginAssembly(Guid solutionId, string assemblyName);
    List<PluginDefinition> GetPluginTypes(Guid assemblyId);
    List<ParentReference<Step, PluginDefinition>> GetPluginSteps(IEnumerable<PluginDefinition> pluginTypes, Guid solutionId);
    IEnumerable<Step> GetMissingUserContexts(IEnumerable<Step> pluginSteps);
}