using System.Threading.Channels;
using XrmSync.Watch;

namespace Tests.Watch;

/// <summary>
/// Covers the real <see cref="FileSystemWatcher"/>-backed implementation against a temporary folder.
/// Everything above it (<see cref="WatchLoop"/>) is tested through the <see cref="IWatchFileSystem"/>
/// seam instead, so these are the only tests that touch the disk.
/// </summary>
public class WatchFileSystemTests : IDisposable
{
	private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(15);

	private readonly string directory = Directory.CreateTempSubdirectory("xrmsync-watch-").FullName;

	public void Dispose() => Directory.Delete(directory, recursive: true);

	[Fact]
	public async Task WritingAFileRaisesAChangeForItsFullPath()
	{
		// Arrange
		var fileSystem = new WatchFileSystem();
		var changes = Channel.CreateUnbounded<FileChange>();
		var filePath = Path.Combine(directory, "app.js");

		using var subscription = fileSystem.Subscribe(
			new WatchScope(directory, "*.*", IncludeSubdirectories: true),
			change => changes.Writer.TryWrite(change),
			_ => { });

		// Act
		await File.WriteAllTextAsync(filePath, "console.log('hi');");

		// Assert
		var change = await changes.Reader.ReadAsync(new CancellationTokenSource(EventTimeout).Token);
		Assert.Equal(filePath, change.FullPath);
	}

	[Fact]
	public async Task AFileInASubfolderIsSeenWhenSubdirectoriesAreIncluded()
	{
		// Arrange
		var fileSystem = new WatchFileSystem();
		var changes = Channel.CreateUnbounded<FileChange>();
		var subFolder = Directory.CreateDirectory(Path.Combine(directory, "js")).FullName;
		var filePath = Path.Combine(subFolder, "app.js");

		using var subscription = fileSystem.Subscribe(
			new WatchScope(directory, "*.*", IncludeSubdirectories: true),
			change => changes.Writer.TryWrite(change),
			_ => { });

		// Act
		await File.WriteAllTextAsync(filePath, "console.log('hi');");

		// Assert — other events (e.g. for the folder itself) may arrive first
		using var cts = new CancellationTokenSource(EventTimeout);
		while (true)
		{
			var change = await changes.Reader.ReadAsync(cts.Token);
			if (change.FullPath == filePath)
			{
				return;
			}
		}
	}

	[Fact]
	public async Task AReadableFileIsReportedReadyImmediately()
	{
		// Arrange
		var fileSystem = new WatchFileSystem();
		var filePath = Path.Combine(directory, "MyPlugin.dll");
		await File.WriteAllTextAsync(filePath, "binary");

		// Act
		var readable = await fileSystem.WaitUntilReadableAsync(filePath, TimeSpan.FromSeconds(1), CancellationToken.None);

		// Assert
		Assert.True(readable);
	}

	[Fact]
	public async Task AMissingFileIsNotWaitedForSoTheSyncCanReportIt()
	{
		// Arrange
		var fileSystem = new WatchFileSystem();

		// Act
		var readable = await fileSystem.WaitUntilReadableAsync(
			Path.Combine(directory, "does-not-exist.dll"),
			TimeSpan.FromSeconds(30),
			CancellationToken.None);

		// Assert — returns straight away rather than burning the timeout
		Assert.True(readable);
	}
}
