using DG.XrmPluginSync.Model.CustomApi;
using DG.XrmPluginSync.Model.Plugin;

namespace DG.XrmPluginSync.Dataverse.Interfaces
{
    public interface IPluginWriter
    {
        Guid CreatePluginAssembly(string pluginName, string solutionName, string dllPath, string sourceHash, string assemblyVersion, string description);
        void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);
        void UpdatePlugins(IEnumerable<Step> pluginSteps, IEnumerable<Image> pluginImages, string description);
        void DeletePlugins(IEnumerable<PluginType> pluginTypes, IEnumerable<Step> pluginSteps, IEnumerable<Image> pluginImages, IEnumerable<ApiDefinition> customApis, IEnumerable<RequestParameter> requestParameters, IEnumerable<ResponseProperty> responseProperties);
        List<PluginType> CreatePluginTypes(List<PluginType> pluginTypes, Guid assemblyId, string description);
        List<Step> CreatePluginSteps(List<Step> pluginSteps, List<PluginType> pluginTypes, string solutionName, string description);
        List<Image> CreatePluginImages(List<Image> pluginImages, List<Step> pluginSteps);
    }
}