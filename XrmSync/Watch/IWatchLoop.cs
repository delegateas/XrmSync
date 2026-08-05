namespace XrmSync.Watch;

/// <summary>
/// Re-runs the sync for a watched target. Returns the exit code of that run.
/// </summary>
internal delegate Task<int> WatchRun(WatchTarget target, CancellationToken cancellationToken);

internal interface IWatchLoop
{
	/// <summary>
	/// Watches every target until cancelled. The initial sync pass is the caller's responsibility —
	/// this only reacts to changes, and never runs two syncs concurrently.
	/// </summary>
	Task RunAsync(IReadOnlyList<WatchTarget> targets, WatchRun run, CancellationToken cancellationToken);
}
