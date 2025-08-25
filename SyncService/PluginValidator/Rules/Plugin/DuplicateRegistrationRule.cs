using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class DuplicateRegistrationRule : IValidationRule<Step>
{
    public string ErrorMessage => "Multiple registrations on the same message, stage and entity are not allowed in the same plugin type";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        return items
            .GroupBy(x => (x.PluginType, x.EventOperation, x.ExecutionStage, x.LogicalName))
            .Where(g => g.Count() > 1)
            .Select(g => g.First());
    }
}