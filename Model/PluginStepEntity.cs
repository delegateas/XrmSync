using System;
using System.Collections.Generic;

namespace DG.XrmPluginSync.Model;

public record PluginStepEntity : EntityBase
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
    public required List<PluginImageEntity> PluginImages { get; set; } = [];

    public class PluginStepDTOEqualityComparer<T> : IEqualityComparer<T> where T : PluginStepEntity
    {
        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return
                x.Name == y.Name &&
                x.ExecutionStage == y.ExecutionStage &&
                x.Deployment == y.Deployment &&
                x.ExecutionMode == y.ExecutionMode &&
                x.ExecutionOrder == y.ExecutionOrder &&
                x.FilteredAttributes == y.FilteredAttributes &&
                x.UserContext == y.UserContext;
        }

        public int GetHashCode(T obj)
        {
            return obj.Name != null ? obj.Name.GetHashCode() : 0;
        }
    }
}
