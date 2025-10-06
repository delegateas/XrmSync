using Microsoft.Extensions.DependencyInjection;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.PluginValidator.Rules;

namespace XrmSync.SyncService.PluginValidator;

internal class PluginValidator(IServiceProvider serviceProvider) : IPluginValidator
{
    private static IEnumerable<ValidationException> Validate<T>(string prefix, IEnumerable<T> items, IEnumerable<IValidationRule<T>> rules, Func<T, string> nameOf)
    {
        return rules.SelectMany(rule =>
        {
            var violatingItems = rule.GetViolations(items);
            return violatingItems.Select(x => new ValidationException($"{prefix} {nameOf(x)}: {rule.ErrorMessage(x)}"));
        });
    }

    public void Validate(List<PluginDefinition> pluginTypes)
    {
        var pluginStepsWithParents = pluginTypes.SelectMany(x => x.PluginSteps.Select(step => new ParentReference<Step, PluginDefinition>(step, x)));
        var pluginSteps = pluginStepsWithParents.Select(x => x.Entity);

        var stepRules = serviceProvider.GetServices<IValidationRule<Step>>();
        var stepWithParentRules = serviceProvider.GetServices<IValidationRule<ParentReference<Step, PluginDefinition>>>();

        IEnumerable<ValidationException> exceptions = [
            ..Validate("Plugin", pluginSteps, stepRules, s => s.Name),
            ..Validate("Plugin", pluginStepsWithParents, stepWithParentRules, s => s.Entity.Name)
        ];

        ThrowExceptions(exceptions, "Some plugins can't be validated");
    }

    public void Validate(List<CustomApiDefinition> customApis)
    {
        var rules = serviceProvider.GetServices<IValidationRule<CustomApiDefinition>>();

        ThrowExceptions(Validate("CustomAPI", customApis, rules, s => s.Name), "Some custom apis can't be validated");
    }

    private static void ThrowExceptions(IEnumerable<ValidationException> exceptions, string aggregateExceptionMessage)
    {
        var exceptionsList = exceptions.ToList();
        if (exceptionsList.Count == 1) throw exceptionsList[0];
        else if (exceptionsList.Count > 1) throw new AggregateException(aggregateExceptionMessage, exceptions);
    }
}
