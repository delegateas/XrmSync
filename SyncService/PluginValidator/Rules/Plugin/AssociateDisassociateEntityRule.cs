using XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class AssociateDisassociateEntityRule : IValidationRule<Step>
{
    public string ErrorMessage => "Associate/Disassociate events must target all entities";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        var adSteps = items.Where(x => x.EventOperation == nameof(EventOperation.Associate) ||
            x.EventOperation == nameof(EventOperation.Disassociate));
        
        return adSteps.Where(x => !string.IsNullOrWhiteSpace(x.LogicalName));
    }
}