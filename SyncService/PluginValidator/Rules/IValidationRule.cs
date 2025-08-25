using XrmSync.Model;

namespace XrmSync.SyncService.PluginValidator.Rules;

internal interface IValidationRule<T> where T : EntityBase
{
    string ErrorMessage { get; }

    IEnumerable<T> GetViolations(IEnumerable<T> items);
}
