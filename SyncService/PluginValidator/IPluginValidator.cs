using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator;

public interface IPluginValidator
{
    void Validate(List<PluginDefinition> pluginTypes);

    // TODO: Implement validation for custom APIs
    //void Validate(List<ApiDefinition> customApis);
}