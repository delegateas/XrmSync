using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.Extensions;

namespace XrmSync.SyncService.Difference;

internal class DifferenceCalculator(
	IEntityComparer<PluginDefinition> pluginDefinitionComparer,
	IEntityComparer<Step> pluginStepComparer,
	IEntityComparer<Image> pluginImageComparer,
	IEntityComparer<CustomApiDefinition> customApiComparer,
	IEntityComparer<RequestParameter> requestComparer,
	IEntityComparer<ResponseProperty> responseComparer,
	IPrintService printer) : IDifferenceCalculator
{
	public Differences CalculateDifferences(AssemblyInfo localData, AssemblyInfo? remoteData)
	{
		var pluginTypeDifference = GetDifference(localData.Plugins, remoteData?.Plugins ?? [], pluginDefinitionComparer);
		printer.Print(pluginTypeDifference, "Types", x => x.Name);

		var pluginStepsDifference =
			GetDifferences(localData, remoteData,
				d => d?.Plugins,
				plugin => plugin?.PluginSteps.Select(step => new ParentReference<Step, PluginDefinition>(step, plugin)),
				pluginStepComparer,
				true);

		printer.Print(pluginStepsDifference, "Plugin Steps", x => x.Entity.Name);

		// Reset image IDs for recreated steps so images are re-created alongside their parent
		var recreatedStepIds = pluginStepsDifference.Creates
			.Where(c => c.Remote != null)
			.Select(c => c.Local.Entity.Id)
			.ToHashSet();

		foreach (var plugin in localData.Plugins)
		{
			foreach (var step in plugin.PluginSteps.Where(s => recreatedStepIds.Contains(s.Id)))
			{
				foreach (var image in step.PluginImages)
					image.Id = Guid.Empty;
			}
		}

		var pluginImagesDifference =
			localData.Plugins
			.Select(localPlugin =>
				GetDifferences(localPlugin,
					remoteData?.Plugins.FirstOrDefault(r => r.Id == localPlugin.Id),
					d => d?.PluginSteps,
					step => step?.PluginImages.Select(img => new ParentReference<Image, Step>(img, step)),
					pluginImageComparer,
					true)
			)
			.Concat(GetRemotesToDelete(localData, remoteData,
				d => d?.Plugins,
				p => p?.PluginSteps?.SelectMany(step => step.PluginImages.Select(img => new ParentReference<Image, Step>(img, step)))))
			.Flatten();

		printer.Print(pluginImagesDifference, "Plugin Images", x => $"[{x.Entity.Name}] {x.Parent.Name}");

		var customApiDifference =
			GetDifference(localData.CustomApis, remoteData?.CustomApis ?? [], customApiComparer);
		printer.Print(customApiDifference, "Custom APIs", x => x.Name);

		// Reset child IDs for recreated CustomAPIs so children are re-created alongside their parent
		var recreatedApiIds = customApiDifference.Creates
			.Where(c => c.Remote != null)
			.Select(c => c.Local.Id)
			.ToHashSet();

		foreach (var api in localData.CustomApis.Where(a => recreatedApiIds.Contains(a.Id)))
		{
			foreach (var param in api.RequestParameters)
			{
				param.Id = Guid.Empty;
			}

			foreach (var prop in api.ResponseProperties)
			{
				prop.Id = Guid.Empty;
			}
		}

		var customApiRequestDifference =
			GetDifferences(localData, remoteData,
				d => d?.CustomApis,
				api => api?.RequestParameters.Select(param => new ParentReference<RequestParameter, CustomApiDefinition>(param, api)),
				requestComparer, true);
		printer.Print(customApiRequestDifference, "Custom API Request Parameters", x => x.Entity.Name);

		var customApiResponseDifference =
			GetDifferences(localData, remoteData,
				d => d?.CustomApis,
				api => api?.ResponseProperties.Select(prop => new ParentReference<ResponseProperty, CustomApiDefinition>(prop, api)),
				responseComparer, true);
		printer.Print(customApiResponseDifference, "Custom API Response Properties", x => x.Entity.Name);

		return new(pluginTypeDifference,
			pluginStepsDifference, pluginImagesDifference,
			customApiDifference, customApiRequestDifference, customApiResponseDifference);
	}

	private static IEnumerable<Difference<TOutput, TParent>> GetRemotesToDelete<TInput, TEntity, TOutput, TParent>(TInput localData, TInput? remoteData, Func<TInput?, IEnumerable<TEntity>?> dataSelector, Func<TEntity?, IEnumerable<ParentReference<TOutput, TParent>>?> selector)
		where TEntity : EntityBase
		where TOutput : EntityBase
		where TParent : EntityBase
	{
		var localDataEntities = dataSelector(localData)?.ToList() ?? [];
		return dataSelector(remoteData)?
			.Select(remoteData =>
			{
				var localEntity = localDataEntities.FirstOrDefault(l => l.Id == remoteData.Id);
				// If no local entity can be found, we consider all remote request parameters as deletes
				return localEntity is null
					? new Difference<TOutput, TParent>(
						[],
						[],
						[.. selector(remoteData) ?? []])
					: null;
			})
			.Where(s => s is not null)
			.Select(s => s!) ?? [];
	}

	private static Difference<TData, TParent> GetDifferences<TInput, TEntity, TData, TParent>(TInput localData, TInput remoteData, Func<TInput?, IEnumerable<TEntity>?> entitySelector, Func<TEntity?, IEnumerable<ParentReference<TData, TParent>>?> dataSelector, IEntityComparer<TData> comparer, bool includeDeletes = false)
		where TEntity : EntityBase
		where TData : EntityBase
		where TParent : EntityBase
	{
		var differences = entitySelector(localData)?
			.Select(local => GetDifference(
				[.. dataSelector(local) ?? []],
				[.. dataSelector(entitySelector(remoteData)?.FirstOrDefault(r => r.Id == local.Id)) ?? []],
				comparer))
			?? [];

		var deletes = includeDeletes
			? GetRemotesToDelete(localData, remoteData, entitySelector, dataSelector)
			: [];

		return differences.Concat(deletes).Flatten();
	}

	private static Difference<TEntity> GetDifference<TEntity>(List<TEntity> localData, List<TEntity> remoteData, IEntityComparer<TEntity> comparer)
		where TEntity : EntityBase
	{
		var creates = localData.Where(local => local.Id == Guid.Empty).Select(EntityDifference<TEntity>.FromLocal);
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
				return new EntityDifference<TEntity>(localEntity, remoteEntity, differentProperties);
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
					return new EntityDifference<TEntity>(localEntity, remoteEntity, []);
				}

				// Check if there's an update
				// If we have recreates, we should not consider them as updates, however we do want their properties to be included in the recreate
				var update = updates.FirstOrDefault(update => update.Local.Id == localEntity.Id);
				if (update is not null)
				{
					differentProperties = [.. differentProperties, .. update.DifferentProperties];
					updates.Remove(update);
				}

				return new EntityDifference<TEntity>(localEntity, remoteEntity, differentProperties);
			})
			.Where(x => x.DifferentProperties.Any())
			.ToList();

		// Recreates are considered as both creates and deletes
		creates = [.. creates, .. recreates];
		deletes = [.. deletes, .. recreates.Where(x => x.Remote is not null).Select(x => x.Remote!)];

		return new([.. creates], updates, [.. deletes]);
	}

	private static Difference<TEntity, TParent> GetDifference<TEntity, TParent>(List<ParentReference<TEntity, TParent>> localData, List<ParentReference<TEntity, TParent>> remoteData, IEntityComparer<TEntity> comparer)
		where TEntity : EntityBase
		where TParent : EntityBase
	{
		// Create anything that's missing an ID locally (i.e. we couldn't map it to a remote entity)
		var creates = localData
			.Where(local => local.Entity.Id == Guid.Empty)
			.Select(EntityDifference<TEntity, TParent>.FromLocal);

		// Delete anything that doesn't have a matching entry in the local data (i.e. it doesn't exist locally)
		var deletes = remoteData
			.ExceptBy(localData.Select(x => x.Entity.Id), x => x.Entity.Id);

		// Join local and remote data to find potential updates.
		// Filter away those that are identical
		var matched = localData
			.Join(remoteData,
				  local => local.Entity.Id,
				  remote => remote.Entity.Id,
				  (local, remote) => (Local: local, Remote: remote))
			.Where(x => !comparer.Equals(x.Local.Entity, x.Remote.Entity))
			.ToList();

		// Updates can be handled by calling an update, this is different from entity to entity
		var updates = matched
			.Select(match =>
			{
				var (localEntity, remoteEntity) = match;
				var differentProperties = comparer.GetDifferentPropertyNames(localEntity.Entity, remoteEntity.Entity).ToList();
				return new EntityDifference<TEntity, TParent>(localEntity, remoteEntity, differentProperties);
			})
			.Where(x => x.DifferentProperties.Any())
			.ToList();

		// If we can't update the field in Dataverse we have to Delete/Create instead
		var recreates = matched
			.Select(match =>
			{
				var (localEntity, remoteEntity) = match;
				var differentProperties = comparer.GetRequiresRecreate(localEntity.Entity, remoteEntity.Entity).ToList();
				if (differentProperties.Count == 0)
				{
					return new EntityDifference<TEntity, TParent>(localEntity, remoteEntity, []);
				}

				// Check if there's an update
				// If we have recreates, we should not consider them as updates, however we do want their properties to be included in the recreate
				var update = updates.FirstOrDefault(update => update.Local.Entity.Id == localEntity.Entity.Id);
				if (update is not null)
				{
					differentProperties = [.. differentProperties, .. update.DifferentProperties];
					updates.Remove(update);
				}

				return new EntityDifference<TEntity, TParent>(localEntity, remoteEntity, differentProperties);
			})
			.Where(x => x.DifferentProperties.Any())
			.ToList();

		// Recreates are considered as both creates and deletes
		creates = [.. creates, .. recreates];
		deletes = [.. deletes, .. recreates.Where(x => x.Remote is not null).Select(x => x.Remote!)];

		return new([.. creates], updates, [.. deletes]);
	}
}
