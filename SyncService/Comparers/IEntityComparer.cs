using System.Linq.Expressions;
using XrmSync.Model;

namespace XrmSync.SyncService.Comparers
{
    public interface IEntityComparer<TEntity> : IEqualityComparer<TEntity> where TEntity : EntityBase
    {
        public IEnumerable<Expression<Func<TEntity, object?>>> GetDifferentPropertyNames(TEntity local, TEntity remote);
        public IEnumerable<Expression<Func<TEntity, object?>>> GetRequiresRecreate(TEntity local, TEntity remote);
    }
}