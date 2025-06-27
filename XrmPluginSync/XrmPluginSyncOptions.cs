namespace DG.XrmPluginSync;

public class XrmPluginSyncOptions
{
    public string AssemblyPath { get; set; } = string.Empty;
    public string SolutionName { get; set; } = string.Empty;
    public bool DryRun { get; set; }
    public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
    public string? DataverseUrl { get; set; }
}
