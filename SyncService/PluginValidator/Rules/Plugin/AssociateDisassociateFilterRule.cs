using XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class AssociateDisassociateFilterRule : IValidationRule<Step>
{
    public string ErrorMessage(Step item) => item.EventOperation + " event can't have filtered attributes";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        var adSteps = items.Where(x => x.EventOperation == nameof(EventOperation.Associate)
            || x.EventOperation == nameof(EventOperation.Disassociate));

        return adSteps.Where(x => !string.IsNullOrWhiteSpace(x.FilteredAttributes));
    }
}