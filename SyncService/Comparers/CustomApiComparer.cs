using DG.XrmPluginSync.Model.CustomApi;
using DG.XrmPluginSync.SyncService.Common;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class CustomApiComparer(Description description) : BaseComparer<ApiDefinition>
{
    protected override bool EqualsInternal(ApiDefinition x, ApiDefinition y)
    {
        return x.Name == y.Name &&
            x.DisplayName == y.DisplayName &&
            (x.Description.StartsWith($"Synced with {description.ToolHeader}") || y.Description.StartsWith($"Synced with {description.ToolHeader}") || x.Description == y.Description) &&
            x.PluginTypeName == y.PluginTypeName &&
            x.IsCustomizable == y.IsCustomizable &&
            x.IsPrivate == y.IsPrivate &&
            x.ExecutePrivilegeName == y.ExecutePrivilegeName;
    }
}
