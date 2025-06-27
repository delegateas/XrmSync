using DG.XrmPluginSync.SyncService.Exceptions;

namespace DG.XrmPluginSync.SyncService.Models.Requests;

public class SyncRequest : IRequest
{
    public required string AssemblyPath { get; set; }
    public required string ProjectPath { get; set; }
    public required string SolutionName { get; set; }
    public required bool DryRun { get; set; }

    public string GetName()
    {
        return "Sync Plugins";
    }

    public IList<(string key, string value)> GetArguments()
    {
        return [
            ("Assembly Path", AssemblyPath),
            ("Project Path", ProjectPath),
            ("Solution Name", SolutionName),
            ("Dry Run", DryRun.ToString())
        ];
    }

    public void Validate()
    {
        var exceptions = new List<Exception>();
        if (string.IsNullOrEmpty(AssemblyPath))
        {
            exceptions.Add(new ValidationException("Path to assembly must be specified"));
        }
        if (string.IsNullOrEmpty(ProjectPath))
        {
            exceptions.Add(new ValidationException("Path to project must be specified"));
        }
        if (string.IsNullOrEmpty(SolutionName))
        {
            exceptions.Add(new ValidationException("Solution Name must be specified"));
        }
        if (exceptions.Count == 1) throw exceptions.First();
        if (exceptions.Count > 0) throw new AggregateException("The inputs are invalid", exceptions);
    }
}
