using System;
using System.Collections.Generic;

namespace DG.XrmPluginSync.Model.CustomApi;

public record ApiDefinition : EntityBase
{
    public required string UniqueName { get; set; }
    public required string DisplayName { get; set; }
    public required string Description { get; set; }
    public bool IsFunction { get; set; }
    public bool EnabledForWorkflow { get; set; }
    public int BindingType { get; set; }
    public required string BoundEntityLogicalName { get; set; }
    public int AllowedCustomProcessingStepType { get; set; }
    public required string PluginTypeName { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsCustomizable { get; set; }
    public bool IsPrivate { get; set; }
    public required string ExecutePrivilegeName { get; set; }

    public List<RequestParameter> RequestParameters { get; set; } = [];
    public List<ResponseProperty> ResponseProperties { get; set; } = [];
}
