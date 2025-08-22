using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse.Interfaces;

public interface IPluginWriter
{
    Guid CreatePluginAssembly(string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);
    void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);

    List<PluginDefinition> CreatePluginTypes(List<PluginDefinition> pluginTypes, Guid assemblyId, string description);
    void DeletePluginTypes(IEnumerable<PluginDefinition> pluginTypes);

    List<Step> CreatePluginSteps(List<Step> pluginSteps, string description);
    void UpdatePluginSteps(IEnumerable<Step> pluginSteps, string description);
    void DeletePluginSteps(IEnumerable<Step> pluginSteps);

    List<Image> CreatePluginImages(List<Image> pluginImages);
    void UpdatePluginImages(IEnumerable<Image> pluginImages);
    void DeletePluginImages(IEnumerable<Image> pluginImages);

}