using System.Linq.Expressions;
using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public record EntityDifference<TEntity>(TEntity LocalEntity, TEntity RemoteEntity, IEnumerable<Expression<Func<TEntity, object>>> DifferentProperties) where TEntity : EntityBase;
