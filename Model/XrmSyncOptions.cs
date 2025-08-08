namespace XrmSync.Model;

public record XrmSyncOptions(string AssemblyPath, string SolutionName, string LogLevel, bool DryRun);
