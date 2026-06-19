namespace XrmSync.Model.Plugin;

// Command-specific options that can be populated from CLI or profile
public record PluginSyncCommandOptions(
	string AssemblyPath,
	string SolutionName,
	string? ManagedIdentityClientId = null,
	string? ManagedIdentityTenantId = null)
{
	public static PluginSyncCommandOptions Empty => new(string.Empty, string.Empty);

	/// <summary>
	/// True when either managed identity value has been supplied, indicating the
	/// managed identity should be ensured as part of the plugin sync.
	/// </summary>
	public bool HasManagedIdentity =>
		!string.IsNullOrWhiteSpace(ManagedIdentityClientId) || !string.IsNullOrWhiteSpace(ManagedIdentityTenantId);
}
