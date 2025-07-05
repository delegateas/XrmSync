namespace DG.XrmSync.Model.Plugin;

public record Image : EntityBase
{
    public required string PluginStepName { get; set; }
    public required string EntityAlias { get; set; }
    public required int ImageType { get; set; }
    public required string Attributes { get; set; }
}
