using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;

namespace XrmSync.SyncService.Validation.Webresource.Rules;

internal class WebresourceDependencyRule(IWebresourceReader webresourceReader) : IExtendedValidationRule<WebresourceDefinition>
{
    public string ErrorMessage(WebresourceDefinition item) =>
        "Cannot delete webresource as it has dependent objects in the Dataverse environment";

    private static string ErrorMessage(WebresourceDependency dependency) =>
        $"Cannot delete webresource as it is required by {dependency.DependentObjectType} with ID {dependency.DependentObjectId}";

    public IEnumerable<(WebresourceDefinition Entity, string Error)> GetErrorMessages(IEnumerable<WebresourceDefinition> items)
    {
        return GetDependencies(items).Select(d => (d.Webresource, ErrorMessage(d)));
    }

    public IEnumerable<WebresourceDefinition> GetViolations(IEnumerable<WebresourceDefinition> items)
    {
        return GetDependencies(items).Select(d => d.Webresource);
    }

    private IEnumerable<WebresourceDependency> GetDependencies(IEnumerable<WebresourceDefinition> items)
    {
        return webresourceReader.GetWebresourcesWithDependencies(items);
    }
}
