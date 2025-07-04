using DG.XrmPluginSync.Model.Plugin;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class PluginTypeComparer : BaseComparer<PluginType>
{
    protected override bool EqualsInternal(PluginType x, PluginType y)
    {
        return x.Name == y.Name;
    }
}
