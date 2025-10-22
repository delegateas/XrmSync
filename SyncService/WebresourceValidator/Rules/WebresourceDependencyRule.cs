using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;
using XrmSync.SyncService.PluginValidator.Rules;

namespace XrmSync.SyncService.WebresourceValidator.Rules;

internal class WebresourceDependencyRule(IWebresourceReader webresourceReader) : IValidationRule<WebresourceDefinition>
{
    public string ErrorMessage(WebresourceDefinition item) =>
        "Cannot delete webresource as it has dependent objects in the Dataverse environment";

    public IEnumerable<WebresourceDefinition> GetViolations(IEnumerable<WebresourceDefinition> items)
    {
        return webresourceReader.GetWebresourcesWithDependencies(items);
    }
}
