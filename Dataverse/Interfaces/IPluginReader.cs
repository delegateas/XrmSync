using DG.XrmSync.Model;
using DG.XrmSync.Model.Plugin;
using Microsoft.Xrm.Sdk;

namespace DG.XrmSync.Dataverse.Interfaces;

public interface IPluginReader
{
    AssemblyInfo GetPluginAssembly(Guid solutionId, string assemblyName);
    ILookup<Guid, Step> GetPluginSteps(Guid solutionId, IEnumerable<Guid> pluginTypeIds);
    List<PluginType> GetPluginTypes(Guid assemblyId);
    IEnumerable<Step> GetMissingUserContexts(IEnumerable<Step> pluginSteps);
}