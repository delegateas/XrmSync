using XrmSync.Model;
using XrmSync.SyncService.Difference;

namespace XrmSync.SyncService.Extensions;

internal static class EnumerableExtensions
{
    public static Difference<TEntity> Flatten<TEntity>(this IEnumerable<Difference<TEntity>> differences) where TEntity : EntityBase
    {
        return differences.Aggregate(Difference<TEntity>.Empty,
            (acc, diff) => new Difference<TEntity>(
                [.. acc.Creates, .. diff.Creates],
                [.. acc.Updates, .. diff.Updates],
                [.. acc.Deletes, .. diff.Deletes]
                )
            );
    }
}
