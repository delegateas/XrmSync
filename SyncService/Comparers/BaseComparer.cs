using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using XrmSync.Model;

namespace XrmSync.SyncService.Comparers;

internal abstract class BaseComparer<TEntity> : IEntityComparer<TEntity> where TEntity : EntityBase
{
	public abstract IEnumerable<Expression<Func<TEntity, object?>>> GetDifferentPropertyNames(TEntity local, TEntity remote);
	public virtual IEnumerable<Expression<Func<TEntity, object?>>> GetRequiresRecreate(TEntity local, TEntity remote) => [];

	public bool Equals(TEntity? local, TEntity? remote)
	{
		if (ReferenceEquals(local, remote))
			return true;
		if (local is null)
			return false;
		if (remote is null)
			return false;
		if (local.GetType() != remote.GetType())
			return false;

		return !GetDifferentPropertyNames(local, remote).Any() && !GetRequiresRecreate(local, remote).Any();
	}

	public int GetHashCode([DisallowNull] TEntity obj)
	{
		return (obj.Name?.GetHashCode()) ?? 0;
	}
}
