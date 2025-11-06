using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;

namespace XrmSync.SyncService.Validation.Webresource.Rules;

/// <summary>
/// Validates that webresources being created do not have name conflicts with existing webresources in the environment.
/// This rule only applies to webresources being created (Id == default).
/// </summary>
internal class WebresourceNameConflictRule(IWebresourceReader webresourceReader) : IExtendedValidationRule<WebresourceDefinition>
{
    public string ErrorMessage(WebresourceDefinition item) =>
        $"Cannot create webresource '{item.Name}' as a webresource with that name already exists in the environment";

    private static string ErrorMessage(string name, Guid existingId) =>
        $"Cannot create webresource '{name}' as a webresource with that name already exists in the environment (ID: {existingId})";

    public IEnumerable<(WebresourceDefinition Entity, string Error)> GetErrorMessages(IEnumerable<WebresourceDefinition> items)
    {
        var conflicts = GetConflicts(items);

        return conflicts.Select(c => (c.Webresource, ErrorMessage(c.Webresource.Name, c.ExistingId)));
    }

    public IEnumerable<WebresourceDefinition> GetViolations(IEnumerable<WebresourceDefinition> items)
    {
        return GetConflicts(items).Select(c => c.Webresource);
    }

    private IEnumerable<(WebresourceDefinition Webresource, Guid ExistingId)> GetConflicts(IEnumerable<WebresourceDefinition> items)
    {
        // Only validate webresources being created (not yet in environment)
        var itemsToCreate = items.Where(wr => wr.Id == default).ToList();
        if (itemsToCreate.Count == 0)
        {
            return [];
        }

        var existingWebresources = webresourceReader.GetWebresourcesByNames(itemsToCreate.Select(wr => wr.Name));

        return itemsToCreate
            .Where(wr => existingWebresources.ContainsKey(wr.Name))
            .Select(wr => (wr, existingWebresources[wr.Name]));
    }
}
