using System;
using System.Collections.Generic;

namespace DG.XrmSync.Model.Plugin;

public record Step : EntityBase
{
    public required string PluginTypeName { get; set; }
    public required int ExecutionStage { get; set; }
    public required string EventOperation { get; set; }
    public required string LogicalName { get; set; }
    public required int Deployment { get; set; }
    public required int ExecutionMode { get; set; }
    public required int ExecutionOrder { get; set; }
    public required string FilteredAttributes { get; set; }
    public required Guid UserContext { get; set; }
    public required List<Image> PluginImages { get; set; } = [];
}
