using DG.XrmPluginSync.Model;

namespace DG.XrmPluginSync.SyncService.Extensions;

internal static class EntityExtensions
{
    public static void TransferIdsTo<TEntity>(this List<TEntity> source, List<TEntity> dest, Func<TEntity, string> keySelector) where TEntity : EntityBase
    {
        var sourceLookup = source.ToLookup(keySelector, y => y);

        // Set ids on steps if they exist in crm
        dest.ForEach(dest =>
        {
            var entities = sourceLookup[keySelector(dest)].ToList();
            if (entities.Count == 1)
            {
                dest.Id = entities[0].Id;
            }
            else if (entities.Count > 1)
            {
                throw new InvalidOperationException($"Multiple entities with the same name '{dest.Name}' found in Dataverse. This is not allowed.");
            }
            else if (entities.Count == 0)
            {
                // If no entity with the same name exists in CRM, set Id to Guid.Empty
                dest.Id = Guid.Empty;
            }
        });
    }
}
