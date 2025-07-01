using System.Collections.Generic;

namespace DG.XrmPluginSync.Model;

public record PluginTypeEntity : EntityBase
{
    public required List<PluginStepEntity> PluginSteps { get; set; } = [];
}
