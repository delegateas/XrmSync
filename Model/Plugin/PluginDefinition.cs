using System.Collections.Generic;

namespace DG.XrmSync.Model.Plugin;

public record PluginDefinition : EntityBase
{
    public required List<Step> PluginSteps { get; set; } = [];
}

public record PluginType : EntityBase;