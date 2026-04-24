using System.CommandLine;
using XrmSync.Model;

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
	/// Config command does not handle profile sync items.
	/// </summary>
	public Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct)
		=> Task.FromResult<int?>(null);
}
