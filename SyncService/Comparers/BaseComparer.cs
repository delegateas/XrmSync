using DG.XrmPluginSync.Model;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DG.XrmPluginSync.SyncService.Comparers;

public abstract class BaseComparer<TEntity> : IEntityComparer<TEntity> where TEntity : EntityBase
{
    public abstract IEnumerable<Expression<Func<TEntity, object>>> GetDifferentPropertyNames(TEntity x, TEntity y);

    public bool Equals(TEntity? x, TEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return !GetDifferentPropertyNames(x, y).Any();
    }

    public int GetHashCode([DisallowNull] TEntity obj)
    {
        return (obj.Name?.GetHashCode()) ?? 0;
    }
}
