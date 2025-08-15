using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.Extensions;

namespace XrmSync.SyncService.Difference;

public class DifferenceUtility(ILogger log,
    IEntityComparer<PluginType> pluginTypeComparer,
    IEntityComparer<Step> pluginStepComparer,
    IEntityComparer<Image> pluginImageComparer,
    IEntityComparer<CustomApiDefinition> customApiComparer,
    IEntityComparer<RequestParameter> requestComparer,
    IEntityComparer<ResponseProperty> responseComparer) : IDifferenceUtility
{
    internal static Difference<T> GetDifference<T>(List<T> localData, List<T> remoteData, IEntityComparer<T> comparer, Func<T, string>? nameSelector = null) where T : EntityBase
    {
        nameSelector ??= (x) => x.Name;

        var creates = localData
            .Where(local => !remoteData.Any(remote => nameSelector(local) == nameSelector(remote)))
            .ToList();

        var deletes = remoteData
            .Where(remote => !localData.Any(local => nameSelector(remote) == nameSelector(local)))
            .ToList();

        var matched = localData
            .Join(remoteData,
                  local => nameSelector(local),
                  remote => nameSelector(remote),
                  (local, remote) => (Local: local, Remote: remote))
            .Where(x => !comparer.Equals(x.Local, x.Remote))
            .ToList();

        var updates = matched
            .Select(match =>
            {
                var (localEntity, remoteEntity) = match;
                var differentProperties = comparer.GetDifferentPropertyNames(localEntity, remoteEntity).ToList();
                return new EntityDifference<T>(localEntity, remoteEntity, differentProperties);
            })
            .Where(x => x.DifferentProperties.Any())
            .ToList();

        var recreates = matched
            .Select(match =>
            {
                var (localEntity, remoteEntity) = match;
                var differentProperties = comparer.GetRequiresRecreate(localEntity, remoteEntity).ToList();
                return new EntityDifference<T>(localEntity, remoteEntity, differentProperties);
            })
            .Where(x => x.DifferentProperties.Any())
            .ToList();

        // Recreates are considered as both creates and deletes
        creates.AddRange(recreates.Select(x => x.LocalEntity));
        deletes.AddRange(recreates.Select(x => x.RemoteEntity));

        // If we have recreates, we should not consider them as updates
        var updatesWithRecreates = updates
            .Where(x => recreates.Any(r => r.LocalEntity.Id == x.LocalEntity.Id))
            .ToList();
        updates = [.. updates.Except(updatesWithRecreates)];

        // We want to add the update fields to the recreates
        recreates = recreates.ConvertAll(recreate =>
        {
            var matched = updatesWithRecreates.FirstOrDefault(update => nameSelector(recreate.LocalEntity) == nameSelector(update.LocalEntity));
            if (matched != null)
            {
                return new EntityDifference<T>(
                    recreate.LocalEntity,
                    recreate.RemoteEntity,
                    [.. recreate.DifferentProperties, .. matched.DifferentProperties]);
            }

            return recreate;
        });

        return new (creates, updates, deletes, recreates);
    }

    public Differences CalculateDifferences(CompiledData localData, CompiledData remoteData)
    {
        var pluginTypeDifference = GetDifference(localData.Types, remoteData.Types, pluginTypeComparer);
        log.Print(pluginTypeDifference, "Types", x => x.Name);

        var pluginStepsDifference = GetDifference(localData.Steps, remoteData.Steps, pluginStepComparer);
        log.Print(pluginStepsDifference, "Plugin Steps", x => x.Name);

        var pluginImagesDifference = GetDifference(localData.Images, remoteData.Images, pluginImageComparer, x => $"[{x.Name}] {x.PluginStepName}");
        log.Print(pluginImagesDifference, "Plugin Images", x => $"[{x.Name}] {x.PluginStepName}");

        var customApiDifference = GetDifference(localData.CustomApis, remoteData.CustomApis, customApiComparer);
        log.Print(customApiDifference, "Custom APIs", x => x.Name);

        var requestDifference = GetDifference(localData.RequestParameters, remoteData.RequestParameters, requestComparer);
        log.Print(requestDifference, "Custom API Request Parameters", x => x.Name);

        var responseDifference = GetDifference(localData.ResponseProperties, remoteData.ResponseProperties, responseComparer);
        log.Print(responseDifference, "Custom API Response Properties", x => x.Name);

        return new(pluginTypeDifference,
            pluginStepsDifference, pluginImagesDifference,
            customApiDifference, requestDifference, responseDifference);
    }
}
