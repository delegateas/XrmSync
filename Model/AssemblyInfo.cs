using DG.XrmPluginSync.Model.CustomApi;
using DG.XrmPluginSync.Model.Plugin;
using System.Collections.Generic;

namespace DG.XrmPluginSync.Model;

public record AssemblyInfo : EntityBase
{
    public required string Version { get; set; }
    public required string DllPath { get; set; }
    public required string Hash { get; set; }
    public List<PluginDefinition> Plugins { get; set; } = [];
    public List<ApiDefinition> CustomApis { get; set; } = [];
}
