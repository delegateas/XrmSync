using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.Model.Plugin;
using Microsoft.Xrm.Sdk;

namespace DG.XrmPluginSync.Dataverse.Interfaces;

public interface IPluginReader
{
    AssemblyInfo GetPluginAssembly(Guid solutionId, string assemblyName);
    ILookup<Guid, Step> GetPluginSteps(Guid solutionId, IEnumerable<Guid> pluginTypeIds);
    List<PluginType> GetPluginTypes(Guid assemblyId);
    IEnumerable<Step> GetMissingUserContexts(IEnumerable<Step> pluginSteps);
}