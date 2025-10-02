using XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class AsyncPreOperationRule : IValidationRule<Step>
{
    public string ErrorMessage => "Pre execution stages does not support asynchronous execution mode";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items) => items.Where(x => x.ExecutionMode == ExecutionMode.Asynchronous && x.ExecutionStage != ExecutionStage.PostOperation);
}