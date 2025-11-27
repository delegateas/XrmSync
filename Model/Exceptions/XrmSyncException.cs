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
