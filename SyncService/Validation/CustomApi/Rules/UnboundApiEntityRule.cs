using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.Validation.CustomApi.Rules;

internal class UnboundApiEntityRule : IValidationRule<CustomApiDefinition>
{
	public string ErrorMessage(CustomApiDefinition _) => "Unbound Custom API cannot specify an entity type";

	public IEnumerable<CustomApiDefinition> GetViolations(IEnumerable<CustomApiDefinition> items)
	{
		return items.Where(x => x.BindingType == 0 && !string.IsNullOrWhiteSpace(x.BoundEntityLogicalName));
	}
}
