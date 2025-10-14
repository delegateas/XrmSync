using System.Linq.Expressions;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Comparers;

internal class PluginDefinitionComparer : BaseComparer<PluginDefinition>
{
    public override IEnumerable<Expression<Func<PluginDefinition, object?>>> GetDifferentPropertyNames(PluginDefinition local, PluginDefinition remote)
    {
        if (local.Name != remote.Name) yield return x => x.Name;
    }
}
