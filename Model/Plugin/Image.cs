using DG.XrmPluginCore.Enums;

namespace XrmSync.Model.Plugin;

public record Image : EntityBase
{
    public required Step Step { get; set; }
    public required string EntityAlias { get; set; }
    public required ImageType ImageType { get; set; }
    public required string Attributes { get; set; }
}
