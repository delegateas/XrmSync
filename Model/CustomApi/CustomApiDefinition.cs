using XrmPluginCore.Enums;

namespace XrmSync.Model.CustomApi;

public record CustomApiDefinition(string Name) : EntityBase(Name)
{
	public required PluginType PluginType { get; set; }
	public required string UniqueName { get; set; }
	public required string DisplayName { get; set; }
	public required string Description { get; set; }
	public bool IsFunction { get; set; }
	public bool EnabledForWorkflow { get; set; }
	public BindingType BindingType { get; set; }
	public required string BoundEntityLogicalName { get; set; }
	public AllowedCustomProcessingStepType AllowedCustomProcessingStepType { get; set; }
	public Guid OwnerId { get; set; }
	public bool IsCustomizable { get; set; }
	public bool IsPrivate { get; set; }
	public required string ExecutePrivilegeName { get; set; }

	public List<RequestParameter> RequestParameters { get; set; } = [];
	public List<ResponseProperty> ResponseProperties { get; set; } = [];
}

public record PluginType(string Name) : EntityBase(Name);
