using System.Linq.Expressions;
using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public record EntityDifference<TEntity>(TEntity Local, TEntity? Remote, IEnumerable<Expression<Func<TEntity, object?>>> DifferentProperties) where TEntity : EntityBase
{
    public static EntityDifference<TEntity> FromLocal(TEntity localEntity) => new (localEntity, default, []);
}

public record EntityDifference<TEntity, TParent>(
    ParentReference<TEntity, TParent> Local,
    ParentReference<TEntity, TParent>? Remote,
    IEnumerable<Expression<Func<TEntity, object?>>> DifferentProperties)
    where TEntity : EntityBase
    where TParent : EntityBase
{
    public static EntityDifference<TEntity, TParent> FromLocal(ParentReference<TEntity, TParent> localEntity) => 
        new(localEntity, default, []);
}