using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public record Difference<T>(List<T> Creates, List<EntityDifference<T>> UpdatesWithDifferences, List<T> Deletes, List<EntityDifference<T>> Recreates) where T : EntityBase
{
    public List<T> Updates { get; } = UpdatesWithDifferences.ConvertAll(x => x.LocalEntity);
}
