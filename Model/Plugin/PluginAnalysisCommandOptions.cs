namespace XrmSync.Model.Plugin;

public record PluginAnalysisCommandOptions(string AssemblyPath, string PublisherPrefix, bool PrettyPrint)
{
	public static PluginAnalysisCommandOptions Empty => new(string.Empty, "new", false);
}

