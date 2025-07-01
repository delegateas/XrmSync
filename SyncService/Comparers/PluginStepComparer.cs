using DG.XrmPluginSync.Model;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class PluginStepComparer : IEqualityComparer<PluginStepEntity>
{
    public bool Equals(PluginStepEntity? x, PluginStepEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
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

    public int GetHashCode(PluginStepEntity obj)
    {
        return (obj.Name?.GetHashCode()) ?? 0;
    }
}
