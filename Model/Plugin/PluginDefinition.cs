using System.Collections.Generic;

namespace XrmSync.Model.Plugin;

public record PluginDefinition(string Name) : EntityBase(Name)
{
    public required List<Step> PluginSteps { get; set; }
}