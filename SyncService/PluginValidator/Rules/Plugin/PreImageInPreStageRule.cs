using DG.XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class PreImageInPreStageRule : IValidationRule<Step>
{
    public string ErrorMessage => "Pre execution stages does not support post-images";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        return items.Where(x => (x.ExecutionStage == ExecutionStage.PreOperation ||
            x.ExecutionStage == ExecutionStage.PreValidation) && 
            x.PluginImages.Any(image => image.ImageType == ImageType.PostImage));
    }
}