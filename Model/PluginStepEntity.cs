using System;
using System.Collections.Generic;

namespace DG.XrmPluginSync.Model
{
    public class PluginStepEntity : EntityBase
    {
        public string PluginTypeName { get; set; }
        public int ExecutionStage { get; set; }
        public string EventOperation { get; set; }
        public string LogicalName { get; set; }
        public int Deployment { get; set; }
        public int ExecutionMode { get; set; }
        public int ExecutionOrder { get; set; }
        public string FilteredAttributes { get; set; }
        public Guid UserContext { get; set; }
        public List<PluginImageEntity> PluginImages { get; set; } = new List<PluginImageEntity>();

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
}
