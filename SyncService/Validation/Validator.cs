using XrmSync.SyncService.Exceptions;

namespace XrmSync.SyncService.Validation;

internal abstract class Validator<T> : IValidator<T>
{
    public abstract void ValidateOrThrow(IEnumerable<T> items);

    protected static void ValidateOrThrow(string prefix, IEnumerable<T> items, IEnumerable<IValidationRule<T>> rules, Func<T, string> nameOf, string aggregateExceptionMessage) =>
        ThrowExceptions(Validate(prefix, items, rules, nameOf), aggregateExceptionMessage);

    protected static IEnumerable<ValidationException> Validate<TInner>(string prefix, IEnumerable<TInner> items, IEnumerable<IValidationRule<TInner>> rules, Func<TInner, string> nameOf) =>
        rules.SelectMany(rule =>
        {
            var messages = (rule is IExtendedValidationRule<TInner> er)
                ? er.GetErrorMessages(items)
                : rule.GetViolations(items).Select(x => (Entity: x, Error: rule.ErrorMessage(x)));

            return messages.Select(x => new ValidationException($"{prefix} {nameOf(x.Entity)}: {x.Error}"));
        });

    protected static void ThrowExceptions(IEnumerable<ValidationException> exceptions, string aggregateExceptionMessage)
    {
        var exceptionsList = exceptions.ToList();
        if (exceptionsList.Count == 1) throw exceptionsList[0];
        else if (exceptionsList.Count > 1) throw new AggregateException(aggregateExceptionMessage, exceptions);
    }
}
