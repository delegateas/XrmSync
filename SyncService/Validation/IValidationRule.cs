namespace XrmSync.SyncService.Validation;

internal interface IValidationRule<TEntity>
{
    string ErrorMessage(TEntity item);

    IEnumerable<TEntity> GetViolations(IEnumerable<TEntity> items);
}
