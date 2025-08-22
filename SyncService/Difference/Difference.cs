using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public record Difference<TEntity>(List<EntityDifference<TEntity>> Creates, List<EntityDifference<TEntity>> Updates, List<TEntity> Deletes) where TEntity : EntityBase
{
    public static Difference<TEntity> Empty => new([], [], []);
}
