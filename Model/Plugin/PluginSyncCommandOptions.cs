namespace XrmSync.Model.Plugin;

// Command-specific options that can be populated from CLI or profile
/// <param name="NoDelete">
/// Only create and update components — never delete components that no longer exist locally.
/// Records that must be recreated because an immutable property changed are still replaced.
/// </param>
public record PluginSyncCommandOptions(
	string AssemblyPath,
	string SolutionName,
	string? ManagedIdentityClientId = null,
	string? ManagedIdentityTenantId = null,
	bool AllowEmptyTypes = false,
	bool NoDelete = false)
{
	public static PluginSyncCommandOptions Empty => new(string.Empty, string.Empty);

	/// <summary>
	/// True when either managed identity value has been supplied, indicating the
	/// managed identity should be ensured as part of the plugin sync.
	/// </summary>
	public bool HasManagedIdentity =>
		!string.IsNullOrWhiteSpace(ManagedIdentityClientId) || !string.IsNullOrWhiteSpace(ManagedIdentityTenantId);
}
