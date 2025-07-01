using DG.XrmPluginSync.Model;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class PluginTypeComparer : IEqualityComparer<PluginTypeEntity>
{
    public bool Equals(PluginTypeEntity? x, PluginTypeEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return x.Name == y.Name;
    }

    public int GetHashCode(PluginTypeEntity obj)
    {
        return (obj.Name?.GetHashCode()) ?? 0;
    }
}
