using XrmPluginCore.Enums;

namespace XrmSync.Model.Plugin;

public record Image(string Name) : EntityBase(Name)
{
    public required string EntityAlias { get; set; }
    public required ImageType ImageType { get; set; }
    public required string Attributes { get; set; }
}
