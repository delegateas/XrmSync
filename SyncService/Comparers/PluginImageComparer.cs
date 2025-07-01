using DG.XrmPluginSync.Model;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class PluginImageComparer : IEqualityComparer<PluginImageEntity>
{
    public bool Equals(PluginImageEntity? x, PluginImageEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return
            x.Name == y.Name &&
            x.EntityAlias == y.EntityAlias &&
            x.ImageType == y.ImageType &&
            x.Attributes == y.Attributes;
    }

    public int GetHashCode(PluginImageEntity obj)
    {
        return (obj.Name?.GetHashCode()) ?? 0;
    }
}
