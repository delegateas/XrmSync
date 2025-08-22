using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.Extensions;

namespace XrmSync.SyncService.Difference;

public class DifferenceUtility(ILogger log,
    IEntityComparer<PluginDefinition> pluginDefinitionComparer,
    IEntityComparer<Step> pluginStepComparer,
    IEntityComparer<Image> pluginImageComparer,
    IEntityComparer<CustomApiDefinition> customApiComparer,
    IEntityComparer<RequestParameter> requestComparer,
    IEntityComparer<ResponseProperty> responseComparer) : IDifferenceUtility
{
    public Differences CalculateDifferences(AssemblyInfo localData, AssemblyInfo? remoteData)
    {
        var pluginTypeDifference = GetDifference(localData.Plugins, remoteData?.Plugins ?? [], pluginDefinitionComparer);
        log.Print(pluginTypeDifference, "Types", x => x.Name);

        var pluginStepsDifference =
            GetDifferences(localData, remoteData, d => d?.Plugins, p => p?.PluginSteps, pluginStepComparer, true);
        log.Print(pluginStepsDifference, "Plugin Steps", x => x.Name);

        var pluginImagesDifference =
            localData.Plugins
            .Select(localPlugin =>
                GetDifferences(localPlugin, remoteData?.Plugins.FirstOrDefault(r => r.Id == localPlugin.Id), d => d?.PluginSteps, s => s?.PluginImages, pluginImageComparer, true)
            )
            .Concat(GetRemotesToDelete(localData, remoteData, d => d?.Plugins, p => p?.PluginSteps?.SelectMany(s => s.PluginImages)))
            .Flatten();
        log.Print(pluginImagesDifference, "Plugin Images", x => $"[{x.Name}] {x.Step.Name}");

        var customApiDifference =
            GetDifference(localData.CustomApis, remoteData?.CustomApis ?? [], customApiComparer);
        log.Print(customApiDifference, "Custom APIs", x => x.Name);

        var customApiRequestDifference =
            GetDifferences(localData, remoteData, d => d?.CustomApis, c => c?.RequestParameters, requestComparer, true);
        log.Print(customApiRequestDifference, "Custom API Request Parameters", x => x.Name);

        var customApiResponseDifference =
            GetDifferences(localData, remoteData, d => d?.CustomApis, c => c?.ResponseProperties, responseComparer, true);
        log.Print(customApiResponseDifference, "Custom API Response Properties", x => x.Name);

        return new(pluginTypeDifference,
            pluginStepsDifference, pluginImagesDifference,
            customApiDifference, customApiRequestDifference, customApiResponseDifference);
    }

    private static IEnumerable<Difference<T>> GetRemotesToDelete<TInput, TEntity, T>(TInput localData, TInput? remoteData, Func<TInput?, IEnumerable<TEntity>?> dataSelector, Func<TEntity?, IEnumerable<T>?> selector)
        where TEntity : EntityBase
        where T : EntityBase
    {
        var localDataEntities = dataSelector(localData)?.ToList() ?? [];
        return dataSelector(remoteData)?
            .Select(remoteData =>
            {
                var localEntity = localDataEntities.FirstOrDefault(l => l.Id == remoteData.Id);
                // If no local entity can be found, we consider all remote request parameters as deletes
                return localEntity is null
                    ? new Difference<T>([], [], [.. selector(remoteData) ?? []])
                    : null;
            })
            .Where(s => s is not null)
            .Select(s => s!) ?? [];
    }

    private static Difference<TData> GetDifferences<TInput, TEntity, TData>(TInput localData, TInput remoteData, Func<TInput?, IEnumerable<TEntity>?> entitySelector, Func<TEntity?, IEnumerable<TData>?> dataSelector, IEntityComparer<TData> comparer, bool includeDeletes = false)
        where TEntity : EntityBase
        where TData : EntityBase
    {
        var differences = entitySelector(localData)?
            .Select(local => GetDifference([.. dataSelector(local) ?? []], [.. dataSelector(entitySelector(remoteData)?.FirstOrDefault(r => r.Id == local.Id)) ?? []], comparer))
            ?? [];

        return (includeDeletes ? differences.Concat(GetRemotesToDelete(localData, remoteData, entitySelector, dataSelector)) : differences).Flatten();
    }

    private static Difference<T> GetDifference<T>(List<T> localData, List<T> remoteData, IEntityComparer<T> comparer) where T : EntityBase
    {
        var creates = localData.Where(local => local.Id == Guid.Empty).Select(EntityDifference<T>.FromLocal);
        var deletes = remoteData.ExceptBy(localData.Select(x => x.Id), x => x.Id);

        var matched = localData
            .Join(remoteData,
                  local => local.Id,
                  remote => remote.Id,
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
                if (differentProperties.Count == 0)
                {
                    return new EntityDifference<T>(localEntity, remoteEntity, []);
                }

                // Check if there's an update
                // If we have recreates, we should not consider them as updates, however we do want their properties to be included in the recreate
                var update = updates.FirstOrDefault(update => update.LocalEntity.Id == localEntity.Id);
                if (update is not null)
                {
                    differentProperties = [.. differentProperties, .. update.DifferentProperties];
                    updates.Remove(update);
                }

                return new EntityDifference<T>(localEntity, remoteEntity, differentProperties);
            })
            .Where(x => x.DifferentProperties.Any())
            .ToList();

        // Recreates are considered as both creates and deletes
        creates = [.. creates, .. recreates];
        deletes = [.. deletes, ..recreates.Where(x => x.RemoteEntity is not null).Select(x => x.RemoteEntity!)];

        return new ([.. creates], updates, [.. deletes]);
    }
}
