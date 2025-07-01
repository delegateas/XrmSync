using System.Collections.Generic;

namespace DG.XrmPluginSync.Model;

public record PluginAssembly : EntityBase
{
    public required string Version { get; set; }
    public required string DllPath { get; set; }
    public required string Hash { get; set; }
    public List<PluginTypeEntity> PluginTypes { get; set; } = [];
}
