using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public interface IPrintService
{
	void Print<TEntity, TParent>(Difference<TEntity, TParent> differences, string title, Func<ParentReference<TEntity, TParent>, string> namePicker)
		where TEntity : EntityBase
		where TParent : EntityBase;
	void Print<TEntity>(Difference<TEntity> differences, string title, Func<TEntity, string> namePicker) where TEntity : EntityBase;

	/// <summary>
	/// Reports the deletes that no-delete mode held back, so they don't silently disappear from the output.
	/// </summary>
	void PrintSuppressedDeletes<T>(string title, List<T> suppressed, Func<T, string> namePicker);
}
