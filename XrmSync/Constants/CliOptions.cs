namespace XrmSync.Constants;

/// <summary>
/// Centralized constants for all CLI option names and aliases
/// </summary>
internal static class CliOptions
{
	/// <summary>
	/// Assembly/Plugin options
	/// </summary>
	public static readonly CliOptionDescriptor Assembly = new(
		"--assembly", ["--assembly-file", "-a", "--af"],
		"Path to the plugin assembly (*.dll)",
		Arity: System.CommandLine.ArgumentArity.ZeroOrOne);

	/// <summary>
	/// Allow empty plugin types (keep types that no longer have steps instead of deleting them)
	/// </summary>
	public static readonly CliOptionDescriptor AllowEmptyTypes = new(
		"--allow-empty-types", ["--allow-empty", "--aet"],
		"Keep plugin types registered even when they no longer have any steps, instead of deleting them. Useful for managed solutions where deleting a component would require a full upgrade.",
		Arity: System.CommandLine.ArgumentArity.ZeroOrOne);

	/// <summary>
	/// Webresource folder options
	/// </summary>
	public static readonly CliOptionDescriptor Webresource = new(
		"--folder", ["--webresources", "-w", "--wr", "--path"],
		"Path to the root folder containing the webresources to sync",
		Arity: System.CommandLine.ArgumentArity.ZeroOrOne);

	/// <summary>
	/// File extension filter options
	/// </summary>
	public static readonly CliOptionDescriptor FileExtensions = new(
		"--file-extensions", ["--ext", "-e"],
		"File extensions to include in the sync (e.g. js css). When omitted, all supported types are synced.",
		Arity: System.CommandLine.ArgumentArity.ZeroOrMore,
		AllowMultipleArgumentsPerToken: true);

	/// <summary>
	/// Solution name options
	/// </summary>
	public static readonly CliOptionDescriptor Solution = new(
		"--solution", ["--solution-name", "--sn", "-n"],
		"Name of the solution");

	/// <summary>
	/// Execution mode options
	/// </summary>
	internal static class Execution
	{
		public static readonly CliOptionDescriptor DryRun = new(
			"--dry-run", ["--dryrun", "--dr"],
			"Perform a dry run without making changes to Dataverse",
			Arity: System.CommandLine.ArgumentArity.ZeroOrOne);
	}

	/// <summary>
	/// CI and logging options
	/// </summary>
	internal static class Logging
	{
		public static readonly CliOptionDescriptor CiMode = new(
			"--ci-mode", ["--ci"],
			"Enable CI mode which prefixes all warnings and errors for easier parsing in CI systems",
			Arity: System.CommandLine.ArgumentArity.ZeroOrOne);

		public static readonly CliOptionDescriptor LogLevel = new(
			"--log-level", ["-l", "--ll", "--loglevel"],
			"Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) (Default: Information)");
	}

	/// <summary>
	/// Configuration file options
	/// </summary>
	internal static class Config
	{
		public static readonly CliOptionDescriptor Profile = new(
			"--profile", ["--profile-name", "-p"],
			"Name of the profile to load from appsettings.json (automatically uses single profile if only one exists)");

		public static readonly CliOptionDescriptor All = new(
			"--all", [],
			"Validate all profiles found in the configuration");
	}

	/// <summary>
	/// Managed identity options
	/// </summary>
	internal static class ManagedIdentity
	{
		public static readonly CliOptionDescriptor Operation = new(
			"--operation", ["-o", "--op"],
			"The operation to perform: Remove or Ensure",
			Arity: System.CommandLine.ArgumentArity.ZeroOrOne);

		public static readonly CliOptionDescriptor ClientId = new(
			"--client-id", ["--cid"],
			"Azure AD application (client) ID for the managed identity",
			Arity: System.CommandLine.ArgumentArity.ZeroOrOne);

		public static readonly CliOptionDescriptor TenantId = new(
			"--tenant-id", ["--tid"],
			"Azure AD tenant ID for the managed identity",
			Arity: System.CommandLine.ArgumentArity.ZeroOrOne);
	}

	/// <summary>
	/// Analysis-specific options
	/// </summary>
	internal static class Analysis
	{
		public static readonly CliOptionDescriptor Prefix = new(
			"--prefix", ["--publisher-prefix", "-pp"],
			"Publisher prefix for unique names (Default: new)",
			Arity: System.CommandLine.ArgumentArity.ZeroOrOne);

		public static readonly CliOptionDescriptor PrettyPrint = new(
			"--pretty-print", ["--pp"],
			"Pretty print the JSON output");
	}
}
