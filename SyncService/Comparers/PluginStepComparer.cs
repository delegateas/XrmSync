using DG.XrmSync.Model.Plugin;
using System.Linq.Expressions;

namespace DG.XrmSync.SyncService.Comparers;

public class PluginStepComparer : BaseComparer<Step>
{
    public override IEnumerable<Expression<Func<Step, object>>> GetDifferentPropertyNames(Step x, Step y)
    {
        if (x.Name != y.Name) yield return x => x.Name;
        if (x.ExecutionStage != y.ExecutionStage) yield return x => x.ExecutionStage;
        if (x.Deployment != y.Deployment) yield return x => x.Deployment;
        if (x.ExecutionMode != y.ExecutionMode) yield return x => x.ExecutionMode;
        if (x.ExecutionOrder != y.ExecutionOrder) yield return x => x.ExecutionOrder;
        if (x.FilteredAttributes != y.FilteredAttributes) yield return x => x.FilteredAttributes;
        if (x.UserContext != y.UserContext) yield return x => x.UserContext;
    }
}
