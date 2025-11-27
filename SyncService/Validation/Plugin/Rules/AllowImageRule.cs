using XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Validation.Plugin.Rules;

internal class AllowImageRule : IValidationRule<Step>
{
	public string ErrorMessage(Step item) => item.EventOperation + " message does not support entity images";

	private readonly string[] allowedOperations = [
		nameof(EventOperation.Create),
		nameof(EventOperation.Delete),
		nameof(EventOperation.DeliverIncoming),
		nameof(EventOperation.DeliverPromote),
		nameof(EventOperation.Merge),
		nameof(EventOperation.Route),
		nameof(EventOperation.Send),
		nameof(EventOperation.SetState),
		nameof(EventOperation.Update),
	];

	public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
	{
		return items.Where(i => i.PluginImages.Count > 0 && !allowedOperations.Contains(i.EventOperation));
	}
}
