using System.Linq.Expressions;
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
		var pluginTypes = ComputePluginTypeDiffs(localData, remoteData);
		var pluginSteps = ComputePluginStepDiffs(localData, remoteData);
		ResetChildIdsForRecreatedSteps(localData, pluginSteps);
		var pluginImages = ComputePluginImageDiffs(localData, remoteData);

		var customApis = ComputeCustomApiDiffs(localData, remoteData);
		ResetChildIdsForRecreatedApis(localData, customApis);
		var customApiRequestParams = ComputeRequestParameterDiffs(localData, remoteData);
		var customApiResponseProps = ComputeResponsePropertyDiffs(localData, remoteData);

		return new(pluginTypes, pluginSteps, pluginImages, customApis, customApiRequestParams, customApiResponseProps);
	}

	#region Phase methods

	private Difference<PluginDefinition> ComputePluginTypeDiffs(AssemblyInfo localData, AssemblyInfo? remoteData)
	{
		var result = CompareFlatEntities(localData.Plugins, remoteData?.Plugins ?? [], pluginDefinitionComparer);
		printer.Print(result, "Types", x => x.Name);
		return result;
	}

	private Difference<Step, PluginDefinition> ComputePluginStepDiffs(AssemblyInfo localData, AssemblyInfo? remoteData)
	{
		var result = CompareChildrenAcrossParents(localData, remoteData,
			getParents: d => d?.Plugins,
			getChildren: plugin => plugin?.PluginSteps.Select(step => new ParentReference<Step, PluginDefinition>(step, plugin)),
			pluginStepComparer);
		printer.Print(result, "Plugin Steps", x => x.Entity.Name);
		return result;
	}

	/// <summary>
	/// When a step is recreated (delete + create), its images must also be re-created.
	/// Reset image IDs to Guid.Empty so they flow through the diff as new creates.
	/// </summary>
	private static void ResetChildIdsForRecreatedSteps(AssemblyInfo localData, Difference<Step, PluginDefinition> stepDiffs)
	{
		var recreatedStepIds = stepDiffs.Creates
			.Where(c => c.Remote != null)
			.Select(c => c.Local.Entity.Id)
			.ToHashSet();

		foreach (var plugin in localData.Plugins)
			foreach (var step in plugin.PluginSteps.Where(s => recreatedStepIds.Contains(s.Id)))
				foreach (var image in step.PluginImages)
					image.Id = Guid.Empty;
	}

	private Difference<Image, Step> ComputePluginImageDiffs(AssemblyInfo localData, AssemblyInfo? remoteData)
	{
		// For each local plugin, compute image diffs against the matching remote plugin
		var imageDiffsPerPlugin = localData.Plugins
			.Select(localPlugin => CompareChildrenAcrossParents(
				localPlugin,
				remoteData?.Plugins.FirstOrDefault(r => r.Id == localPlugin.Id),
				getParents: p => p?.PluginSteps,
				getChildren: step => step?.PluginImages.Select(img => new ParentReference<Image, Step>(img, step)),
				pluginImageComparer));

		// For remote-only plugins (deleted), all their images are deletes
		var orphanedImageDeletes = FindOrphanedChildDeletes(localData, remoteData,
			getParents: d => d?.Plugins,
			getChildren: p => p?.PluginSteps?.SelectMany(
				step => step.PluginImages.Select(img => new ParentReference<Image, Step>(img, step))));

		var result = imageDiffsPerPlugin.Concat(orphanedImageDeletes).Flatten();
		printer.Print(result, "Plugin Images", x => $"[{x.Entity.Name}] {x.Parent.Name}");
		return result;
	}

	private Difference<CustomApiDefinition> ComputeCustomApiDiffs(AssemblyInfo localData, AssemblyInfo? remoteData)
	{
		var result = CompareFlatEntities(localData.CustomApis, remoteData?.CustomApis ?? [], customApiComparer);
		printer.Print(result, "Custom APIs", x => x.Name);
		return result;
	}

	/// <summary>
	/// When a CustomAPI is recreated (delete + create), its children must also be re-created.
	/// Reset child IDs to Guid.Empty so they flow through the diff as new creates.
	/// </summary>
	private static void ResetChildIdsForRecreatedApis(AssemblyInfo localData, Difference<CustomApiDefinition> apiDiffs)
	{
		var recreatedApiIds = apiDiffs.Creates
			.Where(c => c.Remote != null)
			.Select(c => c.Local.Id)
			.ToHashSet();

		foreach (var api in localData.CustomApis.Where(a => recreatedApiIds.Contains(a.Id)))
		{
			foreach (var param in api.RequestParameters)
				param.Id = Guid.Empty;
			foreach (var prop in api.ResponseProperties)
				prop.Id = Guid.Empty;
		}
	}

	private Difference<RequestParameter, CustomApiDefinition> ComputeRequestParameterDiffs(AssemblyInfo localData, AssemblyInfo? remoteData)
	{
		var result = CompareChildrenAcrossParents(localData, remoteData,
			getParents: d => d?.CustomApis,
			getChildren: api => api?.RequestParameters.Select(param => new ParentReference<RequestParameter, CustomApiDefinition>(param, api)),
			requestComparer);
		printer.Print(result, "Custom API Request Parameters", x => x.Entity.Name);
		return result;
	}

	private Difference<ResponseProperty, CustomApiDefinition> ComputeResponsePropertyDiffs(AssemblyInfo localData, AssemblyInfo? remoteData)
	{
		var result = CompareChildrenAcrossParents(localData, remoteData,
			getParents: d => d?.CustomApis,
			getChildren: api => api?.ResponseProperties.Select(prop => new ParentReference<ResponseProperty, CustomApiDefinition>(prop, api)),
			responseComparer);
		printer.Print(result, "Custom API Response Properties", x => x.Entity.Name);
		return result;
	}

	#endregion

	#region Diff computation

	/// <summary>
	/// Intermediate result from <see cref="ComputeDiff{TItem, TEntity}"/>.
	/// Holds the local/remote pair and which properties differ.
	/// </summary>
	private record DiffResult<TItem, TEntity>(
		TItem Local,
		TItem? Remote,
		List<Expression<Func<TEntity, object?>>> DifferentProperties)
		where TEntity : EntityBase;

	/// <summary>
	/// Compare two flat lists of top-level entities (e.g. PluginDefinitions, CustomApiDefinitions).
	/// </summary>
	private static Difference<TEntity> CompareFlatEntities<TEntity>(
		List<TEntity> localData, List<TEntity> remoteData, IEntityComparer<TEntity> comparer)
		where TEntity : EntityBase
	{
		var (creates, updates, deletes) = ComputeDiff(localData, remoteData, comparer,
			getId: x => x.Id, getEntity: x => x);

		return new(
			creates.ConvertAll(d => new EntityDifference<TEntity>(d.Local, d.Remote, d.DifferentProperties)),
			updates.ConvertAll(d => new EntityDifference<TEntity>(d.Local, d.Remote, d.DifferentProperties)),
			deletes);
	}

	/// <summary>
	/// Compare two lists of child entities wrapped in ParentReference (e.g. Steps under a Plugin).
	/// </summary>
	private static Difference<TEntity, TParent> CompareChildEntities<TEntity, TParent>(
		List<ParentReference<TEntity, TParent>> localData,
		List<ParentReference<TEntity, TParent>> remoteData,
		IEntityComparer<TEntity> comparer)
		where TEntity : EntityBase
		where TParent : EntityBase
	{
		var (creates, updates, deletes) = ComputeDiff(localData, remoteData, comparer,
			getId: x => x.Entity.Id, getEntity: x => x.Entity);

		return new(
			creates.ConvertAll(d => new EntityDifference<TEntity, TParent>(d.Local, d.Remote, d.DifferentProperties)),
			updates.ConvertAll(d => new EntityDifference<TEntity, TParent>(d.Local, d.Remote, d.DifferentProperties)),
			deletes);
	}

	/// <summary>
	/// Core diff algorithm: given local and remote items, compute creates, updates, deletes, and recreates.
	/// Parameterized by accessor functions so it works for both flat entities and parent-wrapped children.
	/// </summary>
	private static (List<DiffResult<TItem, TEntity>> Creates, List<DiffResult<TItem, TEntity>> Updates, List<TItem> Deletes)
		ComputeDiff<TItem, TEntity>(
			List<TItem> localData,
			List<TItem> remoteData,
			IEntityComparer<TEntity> comparer,
			Func<TItem, Guid> getId,
			Func<TItem, TEntity> getEntity)
		where TEntity : EntityBase
	{
		// New items (no remote ID assigned yet)
		var creates = localData
			.Where(local => getId(local) == Guid.Empty)
			.Select(local => new DiffResult<TItem, TEntity>(local, default, []))
			.ToList();

		// Remote items not present locally
		var deletes = remoteData
			.ExceptBy(localData.Select(getId), getId)
			.ToList();

		// Items that exist on both sides but differ
		var matched = localData
			.Join(remoteData, getId, getId, (local, remote) => (Local: local, Remote: remote))
			.Where(x => !comparer.Equals(getEntity(x.Local), getEntity(x.Remote)))
			.ToList();

		// Updatable property changes
		var updates = matched
			.Select(m => new DiffResult<TItem, TEntity>(m.Local, m.Remote,
				comparer.GetDifferentPropertyNames(getEntity(m.Local), getEntity(m.Remote)).ToList()))
			.Where(x => x.DifferentProperties.Count > 0)
			.ToList();

		// Immutable property changes require delete + create (recreation)
		var recreates = matched
			.Select(m =>
			{
				var props = comparer.GetRequiresRecreate(getEntity(m.Local), getEntity(m.Remote)).ToList();
				if (props.Count == 0)
					return new DiffResult<TItem, TEntity>(m.Local, m.Remote, []);

				// Absorb any update properties into the recreate so we don't lose them
				var update = updates.FirstOrDefault(u => getId(u.Local) == getId(m.Local));
				if (update is not null)
				{
					props = [.. props, .. update.DifferentProperties];
					updates.Remove(update);
				}

				return new DiffResult<TItem, TEntity>(m.Local, m.Remote, props);
			})
			.Where(x => x.DifferentProperties.Count > 0)
			.ToList();

		// Recreates are both creates (new version) and deletes (old version)
		creates.AddRange(recreates);
		deletes.AddRange(recreates
			.Where(x => x.Remote is not null)
			.Select(x => x.Remote!));

		return (creates, updates, deletes);
	}

	/// <summary>
	/// For each parent entity, compare its children against the matching remote parent's children, then flatten.
	/// Also includes orphaned child deletes (children of remote-only parents).
	/// </summary>
	private static Difference<TChild, TParent> CompareChildrenAcrossParents<TSource, TParent, TChild>(
		TSource localSource,
		TSource? remoteSource,
		Func<TSource?, IEnumerable<TParent>?> getParents,
		Func<TParent?, IEnumerable<ParentReference<TChild, TParent>>?> getChildren,
		IEntityComparer<TChild> comparer)
		where TParent : EntityBase
		where TChild : EntityBase
	{
		var differences = getParents(localSource)?
			.Select(localParent => CompareChildEntities(
				[.. getChildren(localParent) ?? []],
				[.. getChildren(getParents(remoteSource)?.FirstOrDefault(r => r.Id == localParent.Id)) ?? []],
				comparer))
			?? [];

		var orphanedDeletes = FindOrphanedChildDeletes<TSource, TParent, TChild, TParent>(localSource, remoteSource, getParents, getChildren);

		return differences.Concat(orphanedDeletes).Flatten();
	}

	/// <summary>
	/// Find children of remote-only parents (parents that don't exist locally).
	/// All such children are marked for deletion.
	/// TSelector is the parent type used for matching (e.g. PluginDefinition),
	/// TChildParent is the parent type in the child's ParentReference (e.g. Step for images).
	/// </summary>
	private static IEnumerable<Difference<TChild, TChildParent>> FindOrphanedChildDeletes<TSource, TSelector, TChild, TChildParent>(
		TSource localSource,
		TSource? remoteSource,
		Func<TSource?, IEnumerable<TSelector>?> getParents,
		Func<TSelector?, IEnumerable<ParentReference<TChild, TChildParent>>?> getChildren)
		where TSelector : EntityBase
		where TChild : EntityBase
		where TChildParent : EntityBase
	{
		var localParents = getParents(localSource)?.ToList() ?? [];
		return getParents(remoteSource)?
			.Where(remoteParent => !localParents.Any(l => l.Id == remoteParent.Id))
			.Select(remoteParent => new Difference<TChild, TChildParent>([], [], [.. getChildren(remoteParent) ?? []]))
			?? [];
	}

	#endregion
}
