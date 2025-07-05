using DG.XrmPluginSync.Model;
using System.Linq.Expressions;

namespace DG.XrmPluginSync.SyncService.Comparers
{
    public interface IEntityComparer<TEntity> : IEqualityComparer<TEntity> where TEntity : EntityBase
    {
        public IEnumerable<Expression<Func<TEntity, object>>> GetDifferentPropertyNames(TEntity x, TEntity y);
    }
}