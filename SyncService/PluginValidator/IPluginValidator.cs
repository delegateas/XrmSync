using XrmSync.Model.Plugin;
using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.PluginValidator;

public interface IPluginValidator
{
    void Validate(List<PluginDefinition> pluginTypes);
    void Validate(List<CustomApiDefinition> customApis);
}