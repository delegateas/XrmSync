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

	/// <summary>
	/// True when this item is part of an active watch session. Lets a sync do extra work that only makes
	/// sense for live development (e.g. publishing webresources).
	/// </summary>
	public bool WatchSession { get; init; }
}
