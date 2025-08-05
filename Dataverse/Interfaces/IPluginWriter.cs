using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse.Interfaces;

public interface IPluginWriter
{
    Guid CreatePluginAssembly(string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);
    void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);
    void UpdatePluginSteps(IEnumerable<Step> pluginSteps, string description);
    void UpdatePluginImages(IEnumerable<Image> pluginImages, List<Step> pluginSteps);
    void DeletePluginImages(IEnumerable<Image> pluginImages);
    void DeletePluginSteps(IEnumerable<Step> pluginSteps);
    void DeletePluginTypes(IEnumerable<PluginType> pluginTypes);
    List<PluginType> CreatePluginTypes(List<PluginType> pluginTypes, Guid assemblyId, string description);
    List<Step> CreatePluginSteps(List<Step> pluginSteps, List<PluginType> pluginTypes, string description);
    List<Image> CreatePluginImages(List<Image> pluginImages, List<Step> pluginSteps);
}