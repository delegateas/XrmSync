using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public record Difference<TEntity>(List<EntityDifference<TEntity>> Creates, List<EntityDifference<TEntity>> Updates, List<TEntity> Deletes)
    where TEntity : EntityBase
{
    public static Difference<TEntity> Empty => new([], [], []);
}


public record Difference<TEntity, TParent>(
    List<EntityDifference<TEntity, TParent>> Creates,
    List<EntityDifference<TEntity, TParent>> Updates,
    List<ParentReference<TEntity, TParent>> Deletes)
    where TEntity : EntityBase
    where TParent : EntityBase
{
    public static Difference<TEntity, TParent> Empty => new([], [], []);
}
