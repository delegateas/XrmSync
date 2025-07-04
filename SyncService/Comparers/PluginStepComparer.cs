using DG.XrmPluginSync.Model.Plugin;

namespace DG.XrmPluginSync.SyncService.Comparers;

public class PluginStepComparer : BaseComparer<Step>
{
    protected override bool EqualsInternal(Step x, Step y)
    {
        return
            x.Name == y.Name &&
            x.ExecutionStage == y.ExecutionStage &&
            x.Deployment == y.Deployment &&
            x.ExecutionMode == y.ExecutionMode &&
            x.ExecutionOrder == y.ExecutionOrder &&
            x.FilteredAttributes == y.FilteredAttributes &&
            x.UserContext == y.UserContext;
    }
}
