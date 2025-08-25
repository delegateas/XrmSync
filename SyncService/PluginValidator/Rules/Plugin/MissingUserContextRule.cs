using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class MissingUserContextRule(IPluginReader pluginReader) : IValidationRule<Step>
{
    public string ErrorMessage => "Defined user context is not in the system";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        return pluginReader.GetMissingUserContexts(items);
    }
}