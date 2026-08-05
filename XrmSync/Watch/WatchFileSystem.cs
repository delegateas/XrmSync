namespace XrmSync.Watch;

internal sealed class WatchFileSystem : IWatchFileSystem
{
	private const int BufferSize = 64 * 1024;

	public IDisposable Subscribe(WatchScope scope, Action<FileChange> onChange, Action<Exception> onError)
	{
		var watcher = new FileSystemWatcher(scope.Directory, scope.Filter)
		{
			IncludeSubdirectories = scope.IncludeSubdirectories,
			InternalBufferSize = BufferSize,
			NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName
		};

		watcher.Created += (_, e) => onChange(new FileChange(e.FullPath, FileChangeKind.Created));
		watcher.Changed += (_, e) => onChange(new FileChange(e.FullPath, FileChangeKind.Changed));
		watcher.Deleted += (_, e) => onChange(new FileChange(e.FullPath, FileChangeKind.Deleted));
		watcher.Renamed += (_, e) =>
		{
			// Both sides matter: the old name may need removing remotely, the new one uploading
			onChange(new FileChange(e.OldFullPath, FileChangeKind.Renamed));
			onChange(new FileChange(e.FullPath, FileChangeKind.Renamed));
		};
		watcher.Error += (_, e) => onError(e.GetException());

		watcher.EnableRaisingEvents = true;
		return watcher;
	}

	public async Task<bool> WaitUntilReadableAsync(string path, TimeSpan timeout, CancellationToken cancellationToken)
	{
		var deadline = DateTimeOffset.UtcNow + timeout;

		while (true)
		{
			if (!File.Exists(path))
			{
				// Nothing to wait for — let the sync itself report the missing file
				return true;
			}

			try
			{
				using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				return true;
			}
			catch (IOException)
			{
				// Still being written
			}
			catch (UnauthorizedAccessException)
			{
				// Locked by the writer
			}

			if (DateTimeOffset.UtcNow >= deadline)
			{
				return false;
			}

			await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
		}
	}
}
