using XrmSync.Model;
using XrmSync.SyncService.Difference;

namespace XrmSync.SyncService.Extensions;

internal static class EnumerableExtensions
{
	public static Difference<TEntity, TParent> Flatten<TEntity, TParent>(this IEnumerable<Difference<TEntity, TParent>> differences)
		where TEntity : EntityBase
		where TParent : EntityBase
	{
		return differences.Aggregate(Difference<TEntity, TParent>.Empty,
			(acc, diff) => new Difference<TEntity, TParent>(
				[.. acc.Creates, .. diff.Creates],
				[.. acc.Updates, .. diff.Updates],
				[.. acc.Deletes, .. diff.Deletes]
				)
			);
	}
}
