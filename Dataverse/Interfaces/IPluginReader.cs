using DG.XrmPluginSync.Model.Plugin;
using Microsoft.Xrm.Sdk;

namespace DG.XrmPluginSync.Dataverse.Interfaces;

public interface IPluginReader
{
    Entity GetPluginAssembly(Guid id);
    Entity GetPluginAssembly(Guid solutionId, string assemblyName);
    Entity GetPluginAssembly(string name, string version);
    List<Entity> GetPluginImages(Guid stepId);
    List<Entity> GetPluginSteps(Guid solutionId);
    List<Entity> GetPluginSteps(Guid solutionId, Guid pluginTypeId);
    List<Entity> GetPluginTypes(Guid assemblyId);
    IEnumerable<Step> GetMissingUserContexts(IEnumerable<Step> pluginSteps);
}