using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.PluginValidator.Rules.CustomApi;

internal class BoundApiEntityRule : IValidationRule<CustomApiDefinition>
{
    public string ErrorMessage(CustomApiDefinition _) => "Bound Custom API must specify an entity type";

    public IEnumerable<CustomApiDefinition> GetViolations(IEnumerable<CustomApiDefinition> items)
    {
        return items.Where(x => x.BindingType != 0 && string.IsNullOrWhiteSpace(x.BoundEntityLogicalName));
    }
}