using XrmPluginCore.Enums;
using System;
using System.Collections.Generic;

namespace XrmSync.Model.Plugin;

public record Step : EntityBase
{
    public required ExecutionStage ExecutionStage { get; set; }
    public required string EventOperation { get; set; }
    public required string LogicalName { get; set; }
    public required Deployment Deployment { get; set; }
    public required ExecutionMode ExecutionMode { get; set; }
    public required int ExecutionOrder { get; set; }
    public required string FilteredAttributes { get; set; }
    public required Guid UserContext { get; set; }
    public required bool AsyncAutoDelete { get; set; }
    public required List<Image> PluginImages { get; set; } = [];
}
