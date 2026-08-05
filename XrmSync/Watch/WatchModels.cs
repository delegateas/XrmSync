using XrmSync.Model;

namespace XrmSync.Watch;

internal enum FileChangeKind
{
	Created,
	Changed,
	Deleted,
	Renamed
}

internal readonly record struct FileChange(string FullPath, FileChangeKind Kind);

/// <summary>
/// Directory, filter and recursion flag handed to the underlying file system watcher.
/// </summary>
internal readonly record struct WatchScope(string Directory, string Filter, bool IncludeSubdirectories);

/// <summary>
/// Resolved watch behaviour. <see cref="Suppressed"/> is true when watching was requested but
/// deliberately disabled (CI mode), so the caller can warn about it.
/// </summary>
internal readonly record struct WatchSettings(bool Enabled, bool Suppressed, TimeSpan Debounce)
{
	public static WatchSettings Disabled => new(false, false, TimeSpan.Zero);

	/// <summary>
	/// The CLI --watch flag wins over the per-item Watch flags; CI mode wins over both, since a
	/// pipeline must never be left with a process that waits for file changes forever.
	/// </summary>
	public static WatchSettings Resolve(bool? cliWatch, bool anyItemWatch, bool ciMode, XrmSyncConfiguration config)
	{
		var requested = cliWatch ?? anyItemWatch;
		if (!requested)
		{
			return Disabled;
		}

		if (ciMode)
		{
			return new WatchSettings(false, true, TimeSpan.Zero);
		}

		// Clamped so an out-of-range value cannot stall the loop, even on the sub-command paths
		// where global configuration validation does not run.
		var debounce = Math.Clamp(config.WatchDebounceMs, 50, 60_000);
		return new WatchSettings(true, false, TimeSpan.FromMilliseconds(debounce));
	}
}

/// <summary>
/// A single watched sync item: what to subscribe to, which events matter, and which file must be
/// readable before the sync is re-run.
/// </summary>
/// <param name="Item">The sync item to re-run when a change is accepted.</param>
/// <param name="Label">Human readable label used in log messages.</param>
/// <param name="Scope">What the underlying watcher subscribes to.</param>
/// <param name="Accept">Event filter, mirroring what the sync itself would read.</param>
/// <param name="ReadinessPath">File that must be readable before re-running (the assembly), or null.</param>
internal sealed record WatchTarget(
	SyncItem Item,
	string Label,
	WatchScope Scope,
	Func<FileChange, bool> Accept,
	string? ReadinessPath);
