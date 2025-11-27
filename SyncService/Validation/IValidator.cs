namespace XrmSync.SyncService.Validation;

public interface IValidator<T>
{
	void ValidateOrThrow(IEnumerable<T> items);
}
