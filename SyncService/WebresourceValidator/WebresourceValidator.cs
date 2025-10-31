using Microsoft.Extensions.DependencyInjection;
using XrmSync.Model.Webresource;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.PluginValidator.Rules;

namespace XrmSync.SyncService.WebresourceValidator;

internal class WebresourceValidator(IServiceProvider serviceProvider) : IWebresourceValidator
{
    private static IEnumerable<ValidationException> Validate<T>(string prefix, IEnumerable<T> items, IEnumerable<IValidationRule<T>> rules, Func<T, string> nameOf)
    {
        return rules.SelectMany(rule =>
        {
            if (rule is IExtendedValidationRule<T> extendedRule)
            {
                return extendedRule.GetErrorMessages(items).Select(x => new ValidationException($"{prefix} {nameOf(x.Entity)}: {x.Error}"));
            }
            else
            {
                return rule.GetViolations(items).Select(x => new ValidationException($"{prefix} {nameOf(x)}: {rule.ErrorMessage(x)}"));
            }
        });
    }

    public void Validate(List<WebresourceDefinition> webresources)
    {
        var rules = serviceProvider.GetServices<IValidationRule<WebresourceDefinition>>();

        var exceptions = Validate("Webresource", webresources, rules, w => w.Name);

        ThrowExceptions(exceptions, "Some webresources can't be validated");
    }

    private static void ThrowExceptions(IEnumerable<ValidationException> exceptions, string aggregateExceptionMessage)
    {
        var exceptionsList = exceptions.ToList();
        if (exceptionsList.Count == 1) throw exceptionsList[0];
        else if (exceptionsList.Count > 1) throw new AggregateException(aggregateExceptionMessage, exceptions);
    }
}
