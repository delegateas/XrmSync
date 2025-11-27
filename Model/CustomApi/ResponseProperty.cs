using XrmPluginCore.Enums;

namespace XrmSync.Model.CustomApi;

public record ResponseProperty(string Name) : EntityBase(Name)
{
	public required string UniqueName { get; set; }
	public required string DisplayName { get; set; }
	public required bool IsCustomizable { get; set; }
	public required string LogicalEntityName { get; set; }
	public required CustomApiParameterType Type { get; set; }
}
