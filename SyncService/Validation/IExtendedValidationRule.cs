namespace XrmSync.SyncService.Validation;

internal interface IExtendedValidationRule<TEntity> : IValidationRule<TEntity>
{
	IEnumerable<(TEntity Entity, string Error)> GetErrorMessages(IEnumerable<TEntity> items);
}
