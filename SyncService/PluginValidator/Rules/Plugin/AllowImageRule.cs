using XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class AllowImageRule : IValidationRule<Step>
{
    public string ErrorMessage(Step item) => item.EventOperation + " message does not support entity images";

    private readonly EventOperation[] allowedOperations = [
        EventOperation.Create,
        EventOperation.Delete,
        EventOperation.DeliverIncoming,
        EventOperation.DeliverPromote,
        EventOperation.Merge,
        EventOperation.Route,
        EventOperation.Send,
        EventOperation.SetState,
        EventOperation.Update,
    ];

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        return items.Where(i => i.PluginImages.Count > 0 && !allowedOperations.Contains(Enum.Parse<EventOperation>(i.EventOperation, true)));
    }
}
