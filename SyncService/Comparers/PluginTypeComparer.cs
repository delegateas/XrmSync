using System.Linq.Expressions;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Comparers;

public class PluginTypeComparer : BaseComparer<PluginType>
{
    public override IEnumerable<Expression<Func<PluginType, object>>> GetDifferentPropertyNames(PluginType x, PluginType y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
    }
}
