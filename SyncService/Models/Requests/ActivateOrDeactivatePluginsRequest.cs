using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.SyncService.Exceptions;
using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync.SyncService.Models.Requests;

public class ActivateOrDeactivatePluginsRequest : RequestBase
{
    public ActivateOrDeactivatePluginsRequest(ILogger logger, Description description) : base(logger, description) { }

    public required string SolutionPath { get; set; }
    public required string SolutionName { get; set; }
    public bool Activate { get; set; }

    public override string GetName() => "Activate or Deactivate Plugins";

    public override IList<(string key, string value)> GetArguments() => [
        ("Solution Path", SolutionPath),
        ("Solution Name", SolutionName),
        ("Activate", Activate.ToString()),
    ];
}
