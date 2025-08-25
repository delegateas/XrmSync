using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.PluginValidator.Rules.CustomApi;

internal class UnboundApiEntityRule : IValidationRule<CustomApiDefinition>
{
    public string ErrorMessage => "Unbound Custom API cannot specify an entity type";

    public IEnumerable<CustomApiDefinition> GetViolations(IEnumerable<CustomApiDefinition> items)
    {
        return items.Where(x => x.BindingType == 0 && !string.IsNullOrWhiteSpace(x.BoundEntityLogicalName));
    }
}