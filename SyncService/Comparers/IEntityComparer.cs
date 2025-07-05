using DG.XrmSync.Model;
using System.Linq.Expressions;

namespace DG.XrmSync.SyncService.Comparers
{
    public interface IEntityComparer<TEntity> : IEqualityComparer<TEntity> where TEntity : EntityBase
    {
        public IEnumerable<Expression<Func<TEntity, object>>> GetDifferentPropertyNames(TEntity x, TEntity y);
    }
}