using System.Linq.Expressions;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Comparers;

public class PluginStepComparer() : BaseComparer<Step>
{
    public override IEnumerable<Expression<Func<Step, object?>>> GetDifferentPropertyNames(Step local, Step remote)
    {
        if (local.Name != remote.Name) yield return x => x.Name;
        if (local.ExecutionOrder != remote.ExecutionOrder) yield return x => x.ExecutionOrder;
        if (local.FilteredAttributes != remote.FilteredAttributes) yield return x => x.FilteredAttributes;
        if (local.UserContext != remote.UserContext) yield return x => x.UserContext;
        if (local.AsyncAutoDelete != remote.AsyncAutoDelete) yield return x => x.AsyncAutoDelete;
    }

    public override IEnumerable<Expression<Func<Step, object?>>> GetRequiresRecreate(Step local, Step remote)
    {
        if (local.ExecutionStage != remote.ExecutionStage) yield return x => x.ExecutionStage;
        if (local.Deployment != remote.Deployment) yield return x => x.Deployment;
        if (local.ExecutionMode != remote.ExecutionMode) yield return x => x.ExecutionMode;
    }
}
