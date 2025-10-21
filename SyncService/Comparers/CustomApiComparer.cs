using System.Linq.Expressions;
using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.Comparers;

internal class CustomApiComparer(IDescription description) : BaseComparer<CustomApiDefinition>
{
    private bool DescriptionEquals(CustomApiDefinition local, CustomApiDefinition remote)
    {
        return local.Description.StartsWith($"Synced with {description.ToolHeader}") ||
               remote.Description.StartsWith($"Synced with {description.ToolHeader}") ||
               local.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase) ||
               remote.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase) ||
               local.Description == remote.Description;
    }

    public override IEnumerable<Expression<Func<CustomApiDefinition, object?>>> GetDifferentPropertyNames(CustomApiDefinition local, CustomApiDefinition remote)
    {
        if (local.Name != remote.Name) yield return x => x.Name;
        if (local.DisplayName != remote.DisplayName) yield return x => x.DisplayName;
        if (!DescriptionEquals(local, remote)) yield return x => x.Description;
        if (local.IsPrivate != remote.IsPrivate) yield return x => x.IsPrivate;
        if (local.ExecutePrivilegeName != remote.ExecutePrivilegeName) yield return x => x.ExecutePrivilegeName;
        if (local.PluginType.Name != remote.PluginType.Name) yield return x => x.PluginType;
    }

    public override IEnumerable<Expression<Func<CustomApiDefinition, object?>>> GetRequiresRecreate(CustomApiDefinition local, CustomApiDefinition remote)
    {
        if (local.UniqueName != remote.UniqueName) yield return x => x.UniqueName;
        if (local.BindingType != remote.BindingType) yield return x => x.BindingType;
        if (local.BoundEntityLogicalName != remote.BoundEntityLogicalName) yield return x => x.BoundEntityLogicalName;
        if (local.IsFunction != remote.IsFunction) yield return x => x.IsFunction;
        if (local.IsCustomizable != remote.IsCustomizable) yield return x => x.IsCustomizable;
        if (local.EnabledForWorkflow != remote.EnabledForWorkflow) yield return x => x.EnabledForWorkflow;
        if (local.AllowedCustomProcessingStepType != remote.AllowedCustomProcessingStepType) yield return x => x.AllowedCustomProcessingStepType;
    }
}
