namespace XrmSync.Watch;

/// <summary>
/// The file system seam used by <see cref="WatchLoop"/>, so the loop can be unit tested without
/// touching disk or waiting for real file system events.
/// </summary>
internal interface IWatchFileSystem
{
	/// <summary>
	/// Starts watching <paramref name="scope"/>. Disposing the returned handle stops the watcher.
	/// </summary>
	IDisposable Subscribe(WatchScope scope, Action<FileChange> onChange, Action<Exception> onError);

	/// <summary>
	/// Waits until the file can be opened for reading — a build may still be writing it.
	/// Returns false when the timeout elapses first, true when the file is readable or does not exist.
	/// </summary>
	Task<bool> WaitUntilReadableAsync(string path, TimeSpan timeout, CancellationToken cancellationToken);
}
