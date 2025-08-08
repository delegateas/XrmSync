using Microsoft.Extensions.Logging;

namespace XrmSync.Model;

public record XrmSyncOptions(string AssemblyPath, string SolutionName, LogLevel LogLevel, bool DryRun);
