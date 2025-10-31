using XrmSync.Model;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Exceptions;

namespace XrmSync.SyncService.Validation.Plugin;

internal class PluginValidator(IEnumerable<IValidationRule<Step>> stepRules, IEnumerable<IValidationRule<ParentReference<Step, PluginDefinition>>> stepWithParentRules) : Validator<PluginDefinition>
{
    public override void ValidateOrThrow(IEnumerable<PluginDefinition> pluginTypes)
    {
        var pluginStepsWithParents = pluginTypes.SelectMany(x => x.PluginSteps.Select(step => new ParentReference<Step, PluginDefinition>(step, x)));
        var pluginSteps = pluginStepsWithParents.Select(x => x.Entity);

        IEnumerable<ValidationException> exceptions = [
            ..Validate("Plugin", pluginSteps, stepRules, s => s.Name),
            ..Validate("Plugin", pluginStepsWithParents, stepWithParentRules, s => s.Entity.Name)
        ];

        ThrowExceptions(exceptions, "Some plugins can't be validated");
    }
}
