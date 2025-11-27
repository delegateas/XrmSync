using XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Validation.Plugin.Rules;

internal class AsyncPreOperationRule : IValidationRule<Step>
{
	public string ErrorMessage(Step item) => "Pre-execution stages do not support asynchronous execution mode";

	public IEnumerable<Step> GetViolations(IEnumerable<Step> items) => items.Where(x => x.ExecutionMode == ExecutionMode.Asynchronous && x.ExecutionStage != ExecutionStage.PostOperation);
}
