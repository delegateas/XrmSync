using System.Linq.Expressions;
using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.Comparers;

internal class ResponsePropertyComparer : BaseComparer<ResponseProperty>
{
    public override IEnumerable<Expression<Func<ResponseProperty, object?>>> GetDifferentPropertyNames(ResponseProperty x, ResponseProperty y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
        if (x.DisplayName != y.DisplayName) yield return x => x.DisplayName;
    }

    public override IEnumerable<Expression<Func<ResponseProperty, object?>>> GetRequiresRecreate(ResponseProperty local, ResponseProperty remote)
    {
        if (local.UniqueName != remote.UniqueName) yield return local => local.UniqueName;
        if (local.IsCustomizable != remote.IsCustomizable) yield return local => local.IsCustomizable;
        if (local.Type != remote.Type) yield return x => x.Type;
        if (local.LogicalEntityName != remote.LogicalEntityName) yield return x => x.LogicalEntityName;
    }
}
