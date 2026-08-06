using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

internal static class DifferenceExtensions
{
	/// <summary>
	/// Drops orphan deletes — remote items that have no local counterpart — and keeps only the deletes that
	/// belong to a recreate (a delete + create pair emitted when an immutable property changed). This is what
	/// backs the no-delete mode: nothing is removed because it disappeared locally, but a record that must be
	/// replaced to pick up an immutable change still is.
	/// Returns the filtered difference along with the deletes that were held back, for reporting.
	/// </summary>
	public static (Difference<TEntity> Result, List<TEntity> Suppressed) WithoutOrphanDeletes<TEntity>(this Difference<TEntity> diff)
		where TEntity : EntityBase
	{
		if (diff.Deletes.Count == 0)
			return (diff, []);

		var recreatedIds = RecreatedRemoteIds(diff.Creates, c => c.Remote?.Id);

		var kept = diff.Deletes.Where(d => recreatedIds.Contains(d.Id)).ToList();
		var suppressed = diff.Deletes.Where(d => !recreatedIds.Contains(d.Id)).ToList();

		return (diff with { Deletes = kept }, suppressed);
	}

	/// <inheritdoc cref="WithoutOrphanDeletes{TEntity}(Difference{TEntity})"/>
	/// <param name="recreatedParentIds">
	/// Parents that are themselves being recreated. Their children were reset to fresh creates by
	/// DifferenceCalculator.ResetChildIdsForRecreated*, which leaves the remote children looking like orphans —
	/// they must still be deleted, since they belong to the version of the parent that is going away.
	/// </param>
	public static (Difference<TEntity, TParent> Result, List<ParentReference<TEntity, TParent>> Suppressed)
		WithoutOrphanDeletes<TEntity, TParent>(this Difference<TEntity, TParent> diff, IReadOnlySet<Guid>? recreatedParentIds = null)
		where TEntity : EntityBase
		where TParent : EntityBase
	{
		if (diff.Deletes.Count == 0)
			return (diff, []);

		var recreatedIds = RecreatedRemoteIds(diff.Creates, c => c.Remote?.Entity.Id);

		bool Keep(ParentReference<TEntity, TParent> delete) =>
			recreatedIds.Contains(delete.Entity.Id)
			|| (recreatedParentIds?.Contains(delete.Parent.Id) ?? false);

		var kept = diff.Deletes.Where(Keep).ToList();
		var suppressed = diff.Deletes.Where(d => !Keep(d)).ToList();

		return (diff with { Deletes = kept }, suppressed);
	}

	/// <summary>
	/// A recreate is a create that carries the remote item it replaces — see DifferenceCalculator.ComputeDiff.
	/// Orphan deletes can never collide with these ids, since a recreate always has a local counterpart.
	/// </summary>
	private static HashSet<Guid> RecreatedRemoteIds<TCreate>(List<TCreate> creates, Func<TCreate, Guid?> getRemoteId)
	{
		return creates
			.Select(getRemoteId)
			.OfType<Guid>()
			.ToHashSet();
	}
}
