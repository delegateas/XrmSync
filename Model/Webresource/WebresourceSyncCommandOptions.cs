namespace XrmSync.Model.Webresource;

/// <param name="PublishAfterSync">
/// Publish the created and updated webresources once the sync completes. Only set for watch sessions,
/// where the whole point is that a saved file goes live without a manual publish.
/// </param>
public record WebresourceSyncCommandOptions(string FolderPath, string SolutionName, List<string>? FileExtensions = null, bool PublishAfterSync = false)
{
	public static WebresourceSyncCommandOptions Empty => new(string.Empty, string.Empty);
}

