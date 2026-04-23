using System.CommandLine;
using XrmSync.Model;

namespace XrmSync.Commands;

/// <summary>
/// Interface for self-contained XrmSync commands that can be added to the root command
/// </summary>
internal interface IXrmSyncCommand
{
	/// <summary>
	/// Gets the command instance with all options and handlers configured
	/// </summary>
	Command GetCommand();

	/// <summary>
	/// Executes this command using values already resolved from a profile sync item
	/// and root-level execution context. Returns null when this command does not
	/// handle the given sync item type; returns an exit code otherwise.
	/// </summary>
	Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct);
}
