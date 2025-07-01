using DG.XrmPluginSync.Model;

namespace DG.XrmPluginSync.Dataverse.Interfaces
{
    public interface IPluginWriter
    {
        Guid CreatePluginAssembly(string pluginName, string solutionName, string dllPath, string sourceHash, string assemblyVersion, string description);
        void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);
        void UpdatePlugins(IEnumerable<PluginStepEntity> pluginSteps, IEnumerable<PluginImageEntity> pluginImages, string description);
        void DeletePlugins(IEnumerable<PluginTypeEntity> pluginTypes, IEnumerable<PluginStepEntity> pluginSteps, IEnumerable<PluginImageEntity> pluginImages);
        List<PluginTypeEntity> CreatePluginTypes(List<PluginTypeEntity> pluginTypes, Guid assemblyId, string description);
        List<PluginStepEntity> CreatePluginSteps(List<PluginStepEntity> pluginSteps, List<PluginTypeEntity> pluginTypes, string solutionName, string description);
        List<PluginImageEntity> CreatePluginImages(List<PluginImageEntity> pluginImages, List<PluginStepEntity> pluginSteps);
    }
}