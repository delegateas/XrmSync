using DG.XrmPluginCore.Enums;

namespace XrmSync.Model.CustomApi;

public record RequestParameter : EntityBase
{
    public required string UniqueName { get; set; }
    public required string CustomApiName { get; set; }
    public required string DisplayName { get; set; }
    public bool IsCustomizable { get; set; }
    public bool IsOptional { get; set; }
    public required string LogicalEntityName { get; set; }
    public CustomApiParameterType Type { get; set; }
}
