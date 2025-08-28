using XrmSync.Model;

namespace XrmSync.SyncService.PluginValidator.Rules;

internal interface IValidationRule<TEntity>
{
    string ErrorMessage { get; }

    IEnumerable<TEntity> GetViolations(IEnumerable<TEntity> items);
}
