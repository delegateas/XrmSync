namespace XrmSync.Model.Webresource;

/// <param name="PublishAfterSync">
/// Publish the created and updated webresources once the sync completes. Only set for watch sessions,
/// where the whole point is that a saved file goes live without a manual publish.
/// </param>
/// <param name="NoDelete">
/// Only create and update webresources — never delete the ones registered in Dataverse that no longer
/// exist in the local folder.
/// </param>
public record WebresourceSyncCommandOptions(string FolderPath, string SolutionName, List<string>? FileExtensions = null, bool PublishAfterSync = false, bool NoDelete = false)
{
	public static WebresourceSyncCommandOptions Empty => new(string.Empty, string.Empty);
}

