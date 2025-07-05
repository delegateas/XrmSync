using DG.XrmPluginSync.Model.CustomApi;
using DG.XrmPluginSync.SyncService.Common;
using System.Linq.Expressions;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class CustomApiComparer(Description description) : BaseComparer<ApiDefinition>
{
    private bool DescriptionEquals(ApiDefinition x, ApiDefinition y)
    {
        return x.Description.StartsWith($"Synced with {description.ToolHeader}") ||
               y.Description.StartsWith($"Synced with {description.ToolHeader}") ||
               x.Description == y.Description;
    }

    public override IEnumerable<Expression<Func<ApiDefinition, object>>> GetDifferentPropertyNames(ApiDefinition x, ApiDefinition y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
        if (x.DisplayName != y.DisplayName) yield return x => x.DisplayName;
        if (!DescriptionEquals(x, y))
            yield return x => x.Description ;
        if (x.PluginTypeName != y.PluginTypeName) yield return x => x.PluginTypeName;
        if (x.IsCustomizable != y.IsCustomizable) yield return x => x.IsCustomizable;
        if (x.IsPrivate != y.IsPrivate) yield return x => x.IsPrivate;
        if (x.ExecutePrivilegeName != y.ExecutePrivilegeName) yield return x => x.ExecutePrivilegeName;
    }
}
