using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace XrmSync.Watch;

/// <summary>
/// Watches a set of targets and re-runs their sync when files change.
/// <para>
/// File system events arrive on arbitrary threads and in bursts — a build rewrites an assembly
/// several times, an editor saves a file in two steps. Each event only sets a per-target dirty flag
/// and pushes a wake-up token onto a bounded channel; a single consumer loop waits out a quiet
/// period, drains the coalesced tokens and then runs the syncs one at a time. That keeps runs
/// strictly sequential (as they are in a normal profile execution), makes an event storm O(1), and
/// lets a change arriving during a sync queue exactly one follow-up run.
/// </para>
/// Dropping a wake-up token when the channel is full is lossless: the dirty flags are the source of
/// truth and the drain step snapshots all of them, and a drop can only happen while another token is
/// still queued — so a drain is always still coming.
/// </summary>
internal sealed class WatchLoop : IWatchLoop
{
	private static readonly TimeSpan ReadinessTimeout = TimeSpan.FromSeconds(10);
	private static readonly TimeSpan[] ResubscribeBackoff = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)];

	private readonly IWatchFileSystem fileSystem;
	private readonly ILogger logger;
	private readonly WatchSettings settings;
	private readonly Func<TimeSpan, CancellationToken, Task> delay;

	private readonly Lock gate = new();
	private readonly Channel<int> wakeups = Channel.CreateBounded<int>(new BoundedChannelOptions(8)
	{
		SingleReader = true,
		FullMode = BoundedChannelFullMode.DropWrite
	});

	private IReadOnlyList<WatchTarget> targets = [];
	private IDisposable?[] subscriptions = [];
	private bool[] dirty = [];
	private bool[] resubscribe = [];
	private bool[] dropped = [];

	public WatchLoop(
		IWatchFileSystem fileSystem,
		ILogger logger,
		WatchSettings settings,
		Func<TimeSpan, CancellationToken, Task>? delay = null)
	{
		this.fileSystem = fileSystem;
		this.logger = logger;
		this.settings = settings;
		this.delay = delay ?? Task.Delay;
	}

	public async Task RunAsync(IReadOnlyList<WatchTarget> targets, WatchRun run, CancellationToken cancellationToken)
	{
		if (targets.Count == 0)
		{
			return;
		}

		this.targets = targets;
		subscriptions = new IDisposable?[targets.Count];
		dirty = new bool[targets.Count];
		resubscribe = new bool[targets.Count];
		dropped = new bool[targets.Count];

		try
		{
			for (var i = 0; i < targets.Count; i++)
			{
				TrySubscribe(i);
			}

			if (AllDropped())
			{
				logger.LogCritical("No watchable target could be subscribed to — stopping watch mode.");
				return;
			}

			var watching = 0;
			for (var i = 0; i < targets.Count; i++)
			{
				if (dropped[i])
				{
					continue;
				}

				watching++;
				logger.LogInformation("Watching {label}", targets[i].Label);
			}
			logger.LogInformation("Watching {count} target(s) for changes. Press Ctrl+C to stop.", watching);

			while (!cancellationToken.IsCancellationRequested)
			{
				// Wake on the first event, then let the burst settle before doing any work
				await wakeups.Reader.ReadAsync(cancellationToken);
				await delay(settings.Debounce, cancellationToken);
				while (wakeups.Reader.TryRead(out _))
				{
					// Coalesced into the snapshot below
				}

				var (due, toResubscribe) = TakeSnapshot();

				foreach (var index in toResubscribe)
				{
					await ResubscribeAsync(index, cancellationToken);
				}

				if (AllDropped())
				{
					logger.LogCritical("All watchers stopped and could not be restarted — stopping watch mode.");
					return;
				}

				foreach (var index in due)
				{
					if (dropped[index])
					{
						continue;
					}

					await RunTargetAsync(targets[index], run, cancellationToken);
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Ctrl+C or a cancelled run — a clean shutdown
		}
		finally
		{
			for (var i = 0; i < subscriptions.Length; i++)
			{
				try
				{
					subscriptions[i]?.Dispose();
				}
				catch (Exception ex)
				{
					logger.LogDebug("Failed to dispose watcher for {label}: {message}", targets[i].Label, ex.Message);
				}
				subscriptions[i] = null;
			}

			logger.LogInformation("Watch mode stopped.");
		}
	}

	private async Task RunTargetAsync(WatchTarget target, WatchRun run, CancellationToken cancellationToken)
	{
		if (target.ReadinessPath is { } readinessPath
			&& !await fileSystem.WaitUntilReadableAsync(readinessPath, ReadinessTimeout, cancellationToken))
		{
			logger.LogWarning("{label} is still locked by another process after {seconds} seconds — attempting the sync anyway.", target.Label, ReadinessTimeout.TotalSeconds);
		}

		logger.LogInformation("Change detected in {label} — re-running sync...", target.Label);

		try
		{
			await run(target, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			// Never let a failed run end the watch session — the point is to keep going until Ctrl+C
			logger.LogError(ex, "Sync failed for {label}: {message}", target.Label, ex.Message);
		}
	}

	private void TrySubscribe(int index)
	{
		var target = targets[index];
		try
		{
			subscriptions[index] = fileSystem.Subscribe(
				target.Scope,
				change =>
				{
					if (target.Accept(change))
					{
						MarkDirty(index);
					}
				},
				ex => HandleWatcherError(index, ex));
		}
		catch (Exception ex)
		{
			logger.LogError("Could not watch {label}: {message}", target.Label, ex.Message);
			dropped[index] = true;
		}
	}

	private async Task ResubscribeAsync(int index, CancellationToken cancellationToken)
	{
		try
		{
			subscriptions[index]?.Dispose();
		}
		catch (Exception ex)
		{
			logger.LogDebug("Failed to dispose watcher for {label}: {message}", targets[index].Label, ex.Message);
		}
		subscriptions[index] = null;

		foreach (var backoff in ResubscribeBackoff)
		{
			await delay(backoff, cancellationToken);

			dropped[index] = false;
			TrySubscribe(index);

			if (!dropped[index])
			{
				logger.LogInformation("Resumed watching {label}", targets[index].Label);
				return;
			}
		}

		logger.LogError("Giving up on watching {label} — it will no longer be re-synced automatically.", targets[index].Label);
		dropped[index] = true;
	}

	private void HandleWatcherError(int index, Exception exception)
	{
		// Typically an InternalBufferOverflow (events were lost) or the watched folder disappearing.
		// A full re-sync recovers whatever was missed, so mark the target dirty and resubscribe.
		logger.LogWarning("Watcher for {label} reported an error: {message}. Restarting it and re-syncing.", targets[index].Label, exception.Message);

		lock (gate)
		{
			resubscribe[index] = true;
			dirty[index] = true;
		}

		wakeups.Writer.TryWrite(index);
	}

	private void MarkDirty(int index)
	{
		lock (gate)
		{
			dirty[index] = true;
		}

		wakeups.Writer.TryWrite(index);
	}

	private (List<int> Due, List<int> Resubscribe) TakeSnapshot()
	{
		var due = new List<int>();
		var toResubscribe = new List<int>();

		lock (gate)
		{
			for (var i = 0; i < targets.Count; i++)
			{
				if (dirty[i])
				{
					dirty[i] = false;
					due.Add(i);
				}

				if (resubscribe[i])
				{
					resubscribe[i] = false;
					toResubscribe.Add(i);
				}
			}
		}

		return (due, toResubscribe);
	}

	private bool AllDropped() => dropped.All(d => d);
}
