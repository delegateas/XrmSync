using Microsoft.Extensions.Logging;

namespace XrmSync.Model;

/// <summary>
/// Carries execution parameters resolved from configuration and CLI overrides.
/// Used as the single DI-registered context replacing both SharedOptions and ExecutionModeOptions.
/// </summary>
public record ExecutionContext(
	string? SolutionName,
	bool? DryRun,
	bool? CiMode,
	LogLevel? LogLevel,
	string? ProfileName)
{
	public static ExecutionContext Empty => new(null, null, null, null, null);
}
