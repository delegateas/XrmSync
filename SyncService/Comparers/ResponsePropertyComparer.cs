using DG.XrmPluginSync.Model.CustomApi;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class ResponsePropertyComparer : BaseComparer<ResponseProperty>
{
    protected override bool EqualsInternal(ResponseProperty x, ResponseProperty y)
    {
        return x.Name == y.Name &&
            x.DisplayName == y.DisplayName &&
            x.UniqueName == y.UniqueName &&
            x.IsCustomizable == y.IsCustomizable &&
            x.Type == y.Type &&
            x.LogicalEntityName == y.LogicalEntityName;
    }
}
