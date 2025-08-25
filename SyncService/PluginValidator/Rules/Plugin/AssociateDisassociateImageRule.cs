using DG.XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class AssociateDisassociateImageRule : IValidationRule<Step>
{
    public string ErrorMessage => "Associate/Disassociate events can't have images";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        var adSteps = items.Where(x => x.EventOperation == nameof(EventOperation.Associate) || x.EventOperation == nameof(EventOperation.Disassociate));
        
        return adSteps.Where(x => x.PluginImages.Any());
    }
}