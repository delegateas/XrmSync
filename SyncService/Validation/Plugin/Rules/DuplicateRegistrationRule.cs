using XrmSync.Model;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Validation.Plugin.Rules;

internal class DuplicateRegistrationRule : IValidationRule<ParentReference<Step, PluginDefinition>>
{
    public string ErrorMessage(ParentReference<Step, PluginDefinition> _) =>
        "Multiple registrations on the same message, stage and entity are not allowed in the same plugin type";

    public IEnumerable<ParentReference<Step, PluginDefinition>> GetViolations(IEnumerable<ParentReference<Step, PluginDefinition>> items)
    {
        return items
            .GroupBy(x => (x.Parent, x.Entity.EventOperation, x.Entity.ExecutionStage, x.Entity.LogicalName))
            .Where(g => g.Count() > 1)
            .Select(g => g.First());
    }
}