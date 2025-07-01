using System.Collections.Generic;

namespace DG.XrmPluginSync.Model;

public record PluginImageEntity : EntityBase
{
    public required string PluginStepName { get; set; }
    public required string EntityAlias { get; set; }
    public required int ImageType { get; set; }
    public required string Attributes { get; set; }
}
