using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator;

public interface IPluginValidator
{
    void Validate(List<PluginDefinition> pluginTypes);
}