using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Model;

namespace XrmSync.Commands;

/// <summary>
/// Single source of truth for all shared CLI option instances.
/// Shared between the root command (as profile overrides) and sub-commands (as primary inputs).
/// All string options are nullable: required-ness is validated programmatically since any option
/// may be satisfied by a profile rather than a CLI argument.
/// </summary>
internal static class CommandOptions
{
	public static readonly Option<string?> Assembly = CliOptions.Assembly.CreateOption<string?>();
	public static readonly Option<bool?> AllowEmptyTypes = CliOptions.AllowEmptyTypes.CreateOption<bool?>();
	public static readonly Option<string?> Solution = CliOptions.Solution.CreateOption<string?>();
	public static readonly Option<string?> Folder = CliOptions.Webresource.CreateOption<string?>();
	public static readonly Option<string[]?> FileExtensions = CliOptions.FileExtensions.CreateOption<string[]?>();
	public static readonly Option<string?> Prefix = CliOptions.Analysis.Prefix.CreateOption<string?>();
	public static readonly Option<IdentityOperation?> Operation = CliOptions.ManagedIdentity.Operation.CreateOption<IdentityOperation?>();
	public static readonly Option<string?> ClientId = CliOptions.ManagedIdentity.ClientId.CreateOption<string?>();
	public static readonly Option<string?> TenantId = CliOptions.ManagedIdentity.TenantId.CreateOption<string?>();
	public static readonly Option<bool?> DryRun = CliOptions.Execution.DryRun.CreateOption<bool?>();
	public static readonly Option<LogLevel?> LogLevel = CliOptions.Logging.LogLevel.CreateOption<LogLevel?>();
	public static readonly Option<bool?> CiMode = CliOptions.Logging.CiMode.CreateOption<bool?>();
	public static readonly Option<string?> Profile = CliOptions.Config.Profile.CreateOption<string?>();
}
