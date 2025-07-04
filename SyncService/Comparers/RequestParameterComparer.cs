using DG.XrmPluginSync.Model.CustomApi;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class RequestParameterComparer : BaseComparer<RequestParameter>
{
    protected override bool EqualsInternal(RequestParameter x, RequestParameter y)
    {
        return x.Name == y.Name &&
            x.DisplayName == y.DisplayName &&
            x.UniqueName == y.UniqueName &&
            x.IsCustomizable == y.IsCustomizable &&
            x.Type == y.Type &&
            x.IsOptional == y.IsOptional &&
            x.LogicalEntityName == y.LogicalEntityName;
    }
}
