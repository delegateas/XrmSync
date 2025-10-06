using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;

namespace XrmSync.Model;

public record AssemblyInfo(string Name) : EntityBase(Name)
{
    public required string Version { get; set; }
    public required string Hash { get; set; }
    public string? DllPath { get; set; }
    public List<PluginDefinition> Plugins { get; set; } = [];
    public List<CustomApiDefinition> CustomApis { get; set; } = [];
}
