using DG.XrmPluginSync.Model;
using System.Diagnostics.CodeAnalysis;

namespace DG.XrmPluginSync.SyncService.Comparers;

public abstract class BaseComparer<TEntity> : IEqualityComparer<TEntity> where TEntity : EntityBase
{
    public bool Equals(TEntity? x, TEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return EqualsInternal(x, y);
    }

    protected abstract bool EqualsInternal(TEntity x, TEntity y);

    public int GetHashCode([DisallowNull] TEntity obj)
    {
        return (obj.Name?.GetHashCode()) ?? 0;
    }
}
