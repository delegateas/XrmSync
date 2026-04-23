namespace XrmSync.Model.Webresource;

public record WebresourceSyncCommandOptions(string FolderPath, string SolutionName, List<string>? FileExtensions = null)
{
	public static WebresourceSyncCommandOptions Empty => new(string.Empty, string.Empty);
}

