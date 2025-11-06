using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;

namespace XrmSync.SyncService.Validation.Webresource.Rules;

/// <summary>
/// Validates that webresources being deleted do not have dependencies in the Dataverse environment.
/// This rule only applies to webresources being deleted (Id != default).
/// </summary>
internal class WebresourceDependencyRule(IWebresourceReader webresourceReader) : IExtendedValidationRule<WebresourceDefinition>
{
    public string ErrorMessage(WebresourceDefinition item) =>
        "Cannot delete webresource as it has dependent objects in the Dataverse environment";

    private static string ErrorMessage(WebresourceDependency dependency) =>
        $"Cannot delete webresource as it is required by {dependency.DependentObjectType} with ID {dependency.DependentObjectId}";

    public IEnumerable<(WebresourceDefinition Entity, string Error)> GetErrorMessages(IEnumerable<WebresourceDefinition> items)
    {
        var violations = GetViolations(items).ToList();
        if (violations.Count == 0)
        {
            return [];
        }

        var dependencies = GetDependenciesForViolations(violations);

        return dependencies.Select(d => (d.Webresource, ErrorMessage(d)));
    }

    public IEnumerable<WebresourceDefinition> GetViolations(IEnumerable<WebresourceDefinition> items)
    {
        return GetDependencies(items).Select(d => d.Webresource);
    }

    private IEnumerable<WebresourceDependency> GetDependencies(IEnumerable<WebresourceDefinition> items)
    {
        // Only validate webresources being deleted (already in environment)
        var itemsToDelete = items.Where(wr => wr.Id != default).ToList();
        if (itemsToDelete.Count == 0)
        {
            return [];
        }

        return webresourceReader.GetWebresourcesWithDependencies(itemsToDelete);
    }

    private IEnumerable<WebresourceDependency> GetDependenciesForViolations(IEnumerable<WebresourceDefinition> violations)
    {
        return webresourceReader.GetWebresourcesWithDependencies(violations);
    }
}
