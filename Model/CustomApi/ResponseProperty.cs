namespace DG.XrmSync.Model.CustomApi;

public record ResponseProperty : EntityBase
{
    public required string UniqueName { get; set; }
    public required string CustomApiName { get; set; }
    public required string DisplayName { get; set; }
    public bool IsCustomizable { get; set; }
    public required string LogicalEntityName { get; set; }
    public int Type { get; set; }
}
