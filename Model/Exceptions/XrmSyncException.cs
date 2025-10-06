namespace XrmSync.Model.Exceptions;

public class XrmSyncException : Exception
{
    public XrmSyncException()
    {
    }

    public XrmSyncException(string? message) : base(message)
    {
    }

    public XrmSyncException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

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
}
