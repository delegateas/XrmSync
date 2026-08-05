using XrmSync.Model;
using XrmSync.Model.Webresource;

namespace XrmSync.Watch;

/// <summary>
/// Turns sync items into watch targets. Only Plugin and Webresource items are watchable — the
/// filters mirror exactly what the corresponding sync reads, so a change that would not affect the
/// sync does not trigger one.
/// </summary>
internal static class WatchTargetResolver
{
	/// <summary>
	/// Returns null when the item type is not watchable, or when its path is not configured.
	/// </summary>
	public static WatchTarget? TryCreate(SyncItem item, ProfileConfiguration profile) => item switch
	{
		PluginSyncItem plugin => ForAssembly(profile.ResolveAssemblyPath(plugin.AssemblyPath), plugin),
		WebresourceSyncItem webresource => ForFolder(webresource.FolderPath, webresource.FileExtensions, webresource),
		_ => null
	};

	public static WatchTarget? ForAssembly(string? assemblyPath, SyncItem item)
	{
		if (string.IsNullOrWhiteSpace(assemblyPath))
		{
			return null;
		}

		var fullPath = Path.GetFullPath(assemblyPath);
		var directory = Path.GetDirectoryName(fullPath);
		var fileName = Path.GetFileName(fullPath);

		if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
		{
			return null;
		}

		// FileSystemWatcher.Filter also matches 8.3 short names, and the build writes sibling files
		// (.pdb, .deps.json) into the same directory — so the file name is re-checked here.
		return new WatchTarget(
			item,
			$"{item.SyncType} ({fileName})",
			new WatchScope(directory, fileName, IncludeSubdirectories: false),
			change => string.Equals(Path.GetFileName(change.FullPath), fileName, StringComparison.OrdinalIgnoreCase),
			ReadinessPath: fullPath);
	}

	public static WatchTarget? ForFolder(string? folderPath, IEnumerable<string>? fileExtensions, SyncItem item)
	{
		if (string.IsNullOrWhiteSpace(folderPath))
		{
			return null;
		}

		var fullPath = Path.GetFullPath(folderPath);

		// Same predicate as LocalReader.ReadWebResourceFolder: a supported extension, and within the
		// configured extension filter when one is set.
		var allowedTypes = WebresourceTypeMap.ResolveTypes(fileExtensions);

		return new WatchTarget(
			item,
			$"{item.SyncType} ({fullPath})",
			new WatchScope(fullPath, "*.*", IncludeSubdirectories: true),
			change =>
			{
				var extension = Path.GetExtension(change.FullPath).ToLowerInvariant();
				return WebresourceTypeMap.ExtensionToType.TryGetValue(extension, out var type)
					&& (allowedTypes.Count == 0 || allowedTypes.Contains(type));
			},
			ReadinessPath: null);
	}
}
