using DG.XrmSync.Model.CustomApi;
using System.Linq.Expressions;

namespace DG.XrmSync.SyncService.Comparers;

public class RequestParameterComparer : BaseComparer<RequestParameter>
{
    public override IEnumerable<Expression<Func<RequestParameter, object>>> GetDifferentPropertyNames(RequestParameter x, RequestParameter y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
        if (x.DisplayName != y.DisplayName) yield return x => x.DisplayName;
        if (x.UniqueName != y.UniqueName) yield return x => x.UniqueName;
        if (x.IsCustomizable != y.IsCustomizable) yield return x => x.IsCustomizable;
        if (x.Type != y.Type) yield return x => x.Type;
        if (x.IsOptional != y.IsOptional) yield return x => x.IsOptional;
        if (x.LogicalEntityName != y.LogicalEntityName) yield return x => x.LogicalEntityName;
    }
}
