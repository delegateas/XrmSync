namespace XrmSync.Model.Exceptions;

public class OptionsValidationException : XrmSyncException
{
    public OptionsValidationException()
    {
    }

    public OptionsValidationException(string? message) : base(message)
    {
    }

    public OptionsValidationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public OptionsValidationException(string prefix, IEnumerable<string> errors)
        : base($"{prefix} options validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(e => $"- {e}"))}")
    {
    }
}
