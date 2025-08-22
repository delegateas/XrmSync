using System.Linq.Expressions;
using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public record EntityDifference<TEntity>(TEntity LocalEntity, TEntity? RemoteEntity, IEnumerable<Expression<Func<TEntity, object>>> DifferentProperties) where TEntity : EntityBase
{
    public static EntityDifference<TEntity> FromLocal(TEntity localEntity) => new (localEntity, default, Enumerable.Empty<Expression<Func<TEntity, object>>>());
}
