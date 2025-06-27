using DG.XrmPluginSync.SyncService.Exceptions;

namespace DG.XrmPluginSync.SyncService.Models.Requests;

public class ActivateOrDeactivatePluginsRequest : IRequest
{
    public string SolutionPath { get; set; }
    public string SolutionName { get; set; }
    public bool Activate { get; set; }
    public string GetName()
    {
        return "Activate or Deactivate Plugins";
    }
    public IList<(string key, string value)> GetArguments()
    {
        return [
            ("Solution Path", SolutionPath),
            ("Solution Name", SolutionPath),
            ("Activate", Activate.ToString()),
        ];
    }

    public void Validate()
    {
        var exceptions = new List<Exception>();
        if (string.IsNullOrEmpty(SolutionPath) && string.IsNullOrEmpty(SolutionName))
        {
            exceptions.Add(new ValidationException("Either the solution name or path of the solution must be specified"));
        }
        if (exceptions.Count == 1) throw exceptions.First();
        if (exceptions.Count > 0) throw new AggregateException("The inputs are invalid", exceptions);
    }
}
