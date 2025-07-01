using System.Collections.Generic;

namespace DG.XrmPluginSync.Model;

public record PluginImageEntity : EntityBase
{
    public required string PluginStepName { get; set; }
    public required string EntityAlias { get; set; }
    public required int ImageType { get; set; }
    public required string Attributes { get; set; }

    public class PluginImageDTOEqualityComparer<T> : IEqualityComparer<T> where T : PluginImageEntity
    {
        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return
                x.Name == y.Name &&
                x.EntityAlias == y.EntityAlias &&
                x.ImageType == y.ImageType &&
                x.Attributes == y.Attributes;
        }

        public int GetHashCode(T obj)
        {
            return obj.Name != null ? obj.Name.GetHashCode() : 0;
        }
    }
}
