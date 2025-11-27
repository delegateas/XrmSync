using XrmSync.Model.CustomApi;

namespace XrmSync.SyncService.Validation.CustomApi;

internal class CustomApiValidator(IEnumerable<IValidationRule<CustomApiDefinition>> rules) : Validator<CustomApiDefinition>
{
	public override void ValidateOrThrow(IEnumerable<CustomApiDefinition> customApis) =>
		ValidateOrThrow("CustomAPI", customApis, rules, s => s.Name, "Some custom apis can't be validated");
}
