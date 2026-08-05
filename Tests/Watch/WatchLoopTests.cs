using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.Watch;

namespace Tests.Watch;

public class WatchLoopTests
{
	private static readonly WatchSettings Settings = new(Enabled: true, Suppressed: false, Debounce: TimeSpan.FromMilliseconds(500));

	private static WatchTarget Target(string label, bool accept = true, string? readinessPath = null) =>
		new(PluginSyncItem.Empty, label, new WatchScope($"/tmp/{label}", "*.*", IncludeSubdirectories: false), _ => accept, readinessPath);

	private static FileChange Change(string path = "/tmp/a/plugin.dll") => new(path, FileChangeKind.Changed);

	[Fact]
	public async Task AcceptedChangeTriggersASingleRunForItsTarget()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var target = Target("Plugin (a.dll)");
		var loop = new WatchLoop(fileSystem, new RecordingLogger(), Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([target], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();
		fileSystem.Raise(Change());
		await delay.ReleaseNextAsync();
		var ran = await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Same(target, ran);
		Assert.Equal(1, runs.Count);
	}

	[Fact]
	public async Task BurstOfChangesInsideTheDebounceWindowCoalescesIntoOneRun()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var loop = new WatchLoop(fileSystem, new RecordingLogger(), Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();

		for (var i = 0; i < 10; i++)
		{
			fileSystem.Raise(Change());
		}

		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(1, runs.Count);
	}

	[Fact]
	public async Task ChangeArrivingDuringARunQueuesExactlyOneFollowUpRun()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var loop = new WatchLoop(fileSystem, new RecordingLogger(), Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// A change raised while the first sync is still running must not be lost
		runs.OnRun = (_, runNumber) =>
		{
			if (runNumber == 1)
			{
				fileSystem.Raise(Change());
			}
			return Task.CompletedTask;
		};

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();
		fileSystem.Raise(Change());

		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();
		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(2, runs.Count);
		Assert.Equal(1, runs.MaxConcurrent);
	}

	[Fact]
	public async Task TwoTargetsChangedInOneWindowEachRunOnceWithoutOverlapping()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var loop = new WatchLoop(fileSystem, new RecordingLogger(), Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)"), Target("Webresource (wwwroot)")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();
		await fileSystem.WaitForSubscriptionAsync();

		fileSystem.Raise(Change(), handlerIndex: 0);
		fileSystem.Raise(Change("/tmp/wwwroot/app.js"), handlerIndex: 1);

		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();
		await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(2, runs.Count);
		Assert.Equal(1, runs.MaxConcurrent);
		Assert.Equal(["Plugin (a.dll)", "Webresource (wwwroot)"], runs.Labels);
	}

	[Fact]
	public async Task RejectedChangeTriggersNoRunAndNoDebounce()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var loop = new WatchLoop(fileSystem, new RecordingLogger(), Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)", accept: false)], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();
		fileSystem.Raise(Change("/tmp/a/plugin.pdb"));

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(0, runs.Count);
		Assert.Equal(0, delay.Calls);
	}

	[Fact]
	public async Task FailingRunIsLoggedAndTheLoopKeepsWatching()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var logger = new RecordingLogger();
		var loop = new WatchLoop(fileSystem, logger, Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		runs.OnRun = (_, runNumber) => runNumber == 1
			? throw new InvalidOperationException("sync exploded")
			: Task.CompletedTask;

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();

		fileSystem.Raise(Change());
		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();

		fileSystem.Raise(Change());
		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(2, runs.Count);
		Assert.True(logger.HasEntry(LogLevel.Error, "sync exploded"));
	}

	[Fact]
	public async Task ReadinessOfTheAssemblyIsAwaitedBeforeTheRun()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var loop = new WatchLoop(fileSystem, new RecordingLogger(), Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)", readinessPath: "/tmp/a/plugin.dll")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();
		fileSystem.Raise(Change());
		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(["/tmp/a/plugin.dll"], fileSystem.ReadinessChecks);
	}

	[Fact]
	public async Task AStillLockedAssemblyIsWarnedAboutAndSyncedAnyway()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem { Readable = false };
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var logger = new RecordingLogger();
		var loop = new WatchLoop(fileSystem, logger, Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)", readinessPath: "/tmp/a/plugin.dll")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();
		fileSystem.Raise(Change());
		await delay.ReleaseNextAsync();
		await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(1, runs.Count);
		Assert.True(logger.HasEntry(LogLevel.Warning, "still locked"));
	}

	[Fact]
	public async Task CancellationStopsTheLoopAndDisposesEveryWatcher()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var loop = new WatchLoop(fileSystem, new RecordingLogger(), Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)"), Target("Webresource (wwwroot)")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();
		await fileSystem.WaitForSubscriptionAsync();

		await cts.CancelAsync();
		await loopTask; // must not throw

		// Assert
		Assert.Equal(2, fileSystem.SubscribeCount);
		Assert.Equal(fileSystem.SubscribeCount, fileSystem.DisposeCount);
	}

	[Fact]
	public async Task WatcherErrorResubscribesAndReSyncsWhateverWasMissed()
	{
		// Arrange
		var fileSystem = new FakeWatchFileSystem();
		var delay = new ControlledDelay();
		var runs = new RunRecorder();
		var logger = new RecordingLogger();
		var loop = new WatchLoop(fileSystem, logger, Settings, delay.Delay);
		var cts = new CancellationTokenSource();

		// Act
		var loopTask = loop.RunAsync([Target("Plugin (a.dll)")], runs.Run, cts.Token);
		await fileSystem.WaitForSubscriptionAsync();

		fileSystem.RaiseError(new InvalidOperationException("Too many changes at once in directory"));

		await delay.ReleaseNextAsync();  // debounce
		await delay.ReleaseNextAsync();  // resubscribe backoff
		await fileSystem.WaitForSubscriptionAsync();
		await runs.WaitForCompletionAsync();

		await cts.CancelAsync();
		await loopTask;

		// Assert
		Assert.Equal(2, fileSystem.SubscribeCount);
		Assert.Equal(1, runs.Count);
		Assert.True(logger.HasEntry(LogLevel.Warning, "reported an error"));
	}
}
