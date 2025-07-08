using DG.XrmSync.Model.Plugin;

namespace DG.XrmSync.SyncService.PluginValidator;

public interface IPluginValidator
{
    void Validate(List<PluginDefinition> pluginTypes);
}