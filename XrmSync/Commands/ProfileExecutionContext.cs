using Microsoft.Extensions.Logging;

namespace XrmSync.Commands;

/// <summary>
/// Carries the already-resolved execution values the root command passes to sub-commands
/// when dispatching profile sync items.
/// </summary>
internal record ProfileExecutionContext(
	string SolutionName,
	bool DryRun,
	bool CiMode,
	LogLevel LogLevel,
	string? ProfileName);
