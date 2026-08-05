using System.Threading.Channels;
using XrmSync.Watch;

namespace Tests.Watch;

/// <summary>
/// In-memory stand-in for the real file system watcher, so the watch loop can be driven
/// deterministically without touching disk or waiting for real file system events.
/// </summary>
internal sealed class FakeWatchFileSystem : IWatchFileSystem
{
	private readonly List<(Action<FileChange> OnChange, Action<Exception> OnError)> handlers = [];
	private readonly Channel<int> subscribed = Channel.CreateUnbounded<int>();

	public List<WatchScope> Scopes { get; } = [];
	public int SubscribeCount { get; private set; }
	public int DisposeCount { get; private set; }
	public List<string> ReadinessChecks { get; } = [];
	public bool Readable { get; set; } = true;
	public Func<WatchScope, bool>? FailSubscribe { get; set; }

	public IDisposable Subscribe(WatchScope scope, Action<FileChange> onChange, Action<Exception> onError)
	{
		if (FailSubscribe?.Invoke(scope) == true)
		{
			throw new IOException($"Cannot watch {scope.Directory}");
		}

		SubscribeCount++;
		Scopes.Add(scope);
		handlers.Add((onChange, onError));
		subscribed.Writer.TryWrite(handlers.Count - 1);
		return new Subscription(this);
	}

	public Task<bool> WaitUntilReadableAsync(string path, TimeSpan timeout, CancellationToken cancellationToken)
	{
		ReadinessChecks.Add(path);
		return Task.FromResult(Readable);
	}

	/// <summary>Waits until the loop has subscribed, so events cannot be raised too early.</summary>
	public async Task<int> WaitForSubscriptionAsync() =>
		await subscribed.Reader.ReadAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

	public void Raise(FileChange change, int handlerIndex = 0) => handlers[handlerIndex].OnChange(change);

	public void RaiseError(Exception exception, int handlerIndex = 0) => handlers[handlerIndex].OnError(exception);

	private sealed class Subscription(FakeWatchFileSystem owner) : IDisposable
	{
		public void Dispose() => owner.DisposeCount++;
	}
}

/// <summary>
/// Replaces the debounce delay with one the test releases explicitly, so no real time passes and
/// coalescing behaviour is deterministic.
/// </summary>
internal sealed class ControlledDelay
{
	private readonly Channel<TaskCompletionSource> pending = Channel.CreateUnbounded<TaskCompletionSource>();

	public int Calls { get; private set; }

	public Task Delay(TimeSpan duration, CancellationToken cancellationToken)
	{
		Calls++;
		var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		pending.Writer.TryWrite(tcs);
		return tcs.Task.WaitAsync(cancellationToken);
	}

	/// <summary>Waits until the loop is waiting on a delay, then lets it continue.</summary>
	public async Task ReleaseNextAsync()
	{
		var tcs = await pending.Reader.ReadAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
		tcs.SetResult();
	}
}
