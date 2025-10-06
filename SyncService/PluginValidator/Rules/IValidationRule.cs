namespace XrmSync.SyncService.PluginValidator.Rules;

internal interface IValidationRule<TEntity>
{
    string ErrorMessage(TEntity item);

    IEnumerable<TEntity> GetViolations(IEnumerable<TEntity> items);
}
