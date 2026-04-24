namespace XrmSync.Model.Plugin;

// Command-specific options that can be populated from CLI or profile
public record PluginSyncCommandOptions(string AssemblyPath, string SolutionName)
{
	public static PluginSyncCommandOptions Empty => new(string.Empty, string.Empty);
}

