using DG.XrmSync.Model.Plugin;
using System.Linq.Expressions;

namespace DG.XrmSync.SyncService.Comparers;

public class PluginTypeComparer : BaseComparer<PluginType>
{
    public override IEnumerable<Expression<Func<PluginType, object>>> GetDifferentPropertyNames(PluginType x, PluginType y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
    }
}
