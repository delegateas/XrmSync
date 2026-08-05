using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using XrmSync.Watch;

namespace Tests.Watch;

/// <summary>
/// Stands in for the sync execution. Records every run, lets a test observe when a run starts and
/// completes, and can hold or fail a run to exercise the loop's behaviour around it.
/// </summary>
internal sealed class RunRecorder
{
	private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

	private readonly Channel<WatchTarget> started = Channel.CreateUnbounded<WatchTarget>();
	private readonly Channel<WatchTarget> completed = Channel.CreateUnbounded<WatchTarget>();
	private readonly Lock gate = new();

	private int concurrent;

	/// <summary>Invoked inside the run, with the 1-based run number.</summary>
	public Func<WatchTarget, int, Task>? OnRun { get; set; }

	public List<string> Labels { get; } = [];
	public int Count { get; private set; }
	public int MaxConcurrent { get; private set; }

	public async Task<int> Run(WatchTarget target, CancellationToken cancellationToken)
	{
		int runNumber;
		lock (gate)
		{
			Count++;
			runNumber = Count;
			Labels.Add(target.Label);
			concurrent++;
			MaxConcurrent = Math.Max(MaxConcurrent, concurrent);
		}

		started.Writer.TryWrite(target);

		try
		{
			if (OnRun is not null)
			{
				await OnRun(target, runNumber);
			}
		}
		finally
		{
			lock (gate)
			{
				concurrent--;
			}
			completed.Writer.TryWrite(target);
		}

		return 0;
	}

	public async Task<WatchTarget> WaitForStartAsync() => await ReadAsync(started.Reader);

	public async Task<WatchTarget> WaitForCompletionAsync() => await ReadAsync(completed.Reader);

	private static async Task<WatchTarget> ReadAsync(ChannelReader<WatchTarget> reader) =>
		await reader.ReadAsync(new CancellationTokenSource(WaitTimeout).Token);
}

/// <summary>
/// Captures log entries so tests can assert on the warnings the loop emits.
/// </summary>
internal sealed class RecordingLogger : ILogger
{
	private readonly Lock gate = new();
	private readonly List<(LogLevel Level, string Message)> entries = [];

	public IReadOnlyList<(LogLevel Level, string Message)> Entries
	{
		get { lock (gate) return [.. entries]; }
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		lock (gate)
		{
			entries.Add((logLevel, formatter(state, exception)));
		}
	}

	public bool HasEntry(LogLevel level, string fragment) =>
		Entries.Any(e => e.Level == level && e.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}
