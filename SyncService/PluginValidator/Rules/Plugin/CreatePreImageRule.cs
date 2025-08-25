using DG.XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class CreatePreImageRule : IValidationRule<Step>
{
    public string ErrorMessage => "Create events does not support pre-images";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        return items.Where(x => x.EventOperation == nameof(EventOperation.Create) && x.PluginImages.Any(image => image.ImageType == ImageType.PreImage));
    }
}