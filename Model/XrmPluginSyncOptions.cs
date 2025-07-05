namespace DG.XrmSync.Model;

public class XrmSyncOptions
{
    public string? DataverseUrl { get; set; }
    public string AssemblyPath { get; set; } = string.Empty;
    public string SolutionName { get; set; } = string.Empty;
    public bool DryRun { get; set; }
    public string LogLevel { get; set; } = "Information";
}
