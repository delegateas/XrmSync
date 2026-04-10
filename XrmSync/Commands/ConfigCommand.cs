using System.CommandLine;

namespace XrmSync.Commands;

internal class ConfigCommand : Command, IXrmSyncCommand
{
	public ConfigCommand() : base("config", "Configuration management commands")
	{
		// Add subcommands
		Add(new ConfigValidateCommand().GetCommand());
		Add(new ConfigListCommand().GetCommand());
	}

	public Command GetCommand() => this;

	/// <summary>
	/// Config command has no profile sync items to override.
	/// </summary>
	public ProfileOverrideProvider? GetProfileOverrides(Option<string?> assembly, Option<string?> solution) => null;
}
