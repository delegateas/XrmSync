using DG.XrmPluginSync.Model.CustomApi;
using System.Linq.Expressions;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class ResponsePropertyComparer : BaseComparer<ResponseProperty>
{
    public override IEnumerable<Expression<Func<ResponseProperty, object>>> GetDifferentPropertyNames(ResponseProperty x, ResponseProperty y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
        if (x.DisplayName != y.DisplayName) yield return x => x.DisplayName;
        if (x.UniqueName != y.UniqueName) yield return x => x.UniqueName;
        if (x.IsCustomizable != y.IsCustomizable) yield return x => x.IsCustomizable;
        if (x.Type != y.Type) yield return x => x.Type;
        if (x.LogicalEntityName != y.LogicalEntityName) yield return x => x.LogicalEntityName;
    }
}
