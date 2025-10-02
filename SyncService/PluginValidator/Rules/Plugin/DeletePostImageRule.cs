using XrmPluginCore.Enums;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.PluginValidator.Rules.Plugin;

internal class DeletePostImageRule : IValidationRule<Step>
{
    public string ErrorMessage => "Delete events does not support post-images";

    public IEnumerable<Step> GetViolations(IEnumerable<Step> items)
    {
        return items.Where(x => x.EventOperation == nameof(EventOperation.Delete) && x.PluginImages.Any(image => image.ImageType == ImageType.PostImage));
    }
}