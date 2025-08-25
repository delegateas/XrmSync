using Microsoft.Extensions.DependencyInjection;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.PluginValidator.Rules;

namespace XrmSync.SyncService.PluginValidator;

internal class PluginValidator(IServiceProvider serviceProvider) : IPluginValidator
{
    private static void Validate<T>(string prefix, IEnumerable<T> items, IEnumerable<IValidationRule<T>> rules, string aggregateException)
        where T : EntityBase
    {
        List<Exception> exceptions = [];
        foreach (var rule in rules)
        {
            var violatingItems = rule.GetViolations(items);
            exceptions.AddRange(violatingItems.Select(x => new ValidationException($"{prefix} {x.Name}: {rule.ErrorMessage}")));
        }

        if (exceptions.Count == 1) throw exceptions[0];
        else if (exceptions.Count > 1) throw new AggregateException(aggregateException, exceptions);
    }

    public void Validate(List<PluginDefinition> pluginTypes)
    {
        var pluginSteps = pluginTypes.SelectMany(x => x.PluginSteps);
        var rules = serviceProvider.GetServices<IValidationRule<Step>>();
        
        Validate("Plugin", pluginSteps, rules, "Some plugins can't be validated");
    }

    public void Validate(List<CustomApiDefinition> customApis)
    {
        var rules = serviceProvider.GetServices<IValidationRule<CustomApiDefinition>>();
        
        Validate("CustomAPI", customApis, rules, "Some custom apis can't be validated");
    }
}
