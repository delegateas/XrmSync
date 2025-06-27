using System.Collections.Generic;

namespace DG.XrmPluginSync.Model
{
    public class PluginTypeEntity : EntityBase
    {
        public List<PluginStepEntity> PluginSteps { get; set; } = new List<PluginStepEntity>();

        public class PluginTypeDTOEqualityComparer<T> : IEqualityComparer<T> where T : PluginTypeEntity
        {
            public bool Equals(T x, T y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Name == y.Name;
            }

            public int GetHashCode(T obj)
            {
                return obj.Name != null ? obj.Name.GetHashCode() : 0;
            }
        }
    }
}
