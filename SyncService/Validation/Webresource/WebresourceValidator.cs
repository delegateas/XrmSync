using XrmSync.Model.Webresource;

namespace XrmSync.SyncService.Validation.Webresource;

internal class WebresourceValidator(IEnumerable<IValidationRule<WebresourceDefinition>> rules) : Validator<WebresourceDefinition>
{
	public override void ValidateOrThrow(IEnumerable<WebresourceDefinition> webresources) =>
		ValidateOrThrow("Webresource", webresources, rules, w => w.Name, "Some webresources can't be validated");
}
