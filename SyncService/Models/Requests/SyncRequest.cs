using DG.XrmPluginSync.SyncService.Common;
using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync.SyncService.Models.Requests;

public class SyncRequest : RequestBase
{
    public SyncRequest(ILogger logger, Description description) : base(logger, description) { }

    public required string AssemblyPath { get; set; }
    public required string SolutionName { get; set; }
    public required bool DryRun { get; set; }

    public override string GetName() => "Sync Plugins";

    public override IList<(string key, string value)> GetArguments() => [
        ("Assembly Path", AssemblyPath),
        ("Solution Name", SolutionName),
        ("Dry Run", DryRun.ToString())
    ];
}
