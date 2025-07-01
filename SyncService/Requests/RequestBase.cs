using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.SyncService.Exceptions;
using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync.SyncService.Requests;

public abstract class RequestBase(ILogger logger, Description description) : IRequest
{
	public abstract string GetName();

	public abstract IList<(string key, string value)> GetArguments();

	public void Validate()
	{
		var exceptions = new List<Exception>();
		exceptions.AddRange(
			GetArguments()
				.Where(arg => string.IsNullOrEmpty(arg.value))
				.Select(arg => new ValidationException($"Argument '{arg.key}' is missing or empty"))
		);

		if (exceptions.Count == 1) throw exceptions.First();
		if (exceptions.Count > 0) throw new AggregateException("The inputs are invalid", exceptions);
	}

	public void LogAndValidateRequest()
	{
		logger.LogInformation(description.ToolHeader);
		logger.LogInformation(GetName());
		var arguments = GetArguments();
		if (arguments.Count != 0)
		{
			foreach (var (key, value) in arguments)
			{
				logger.LogTrace($"{key}: {value}");
			}
		}
		
		try
		{
			Validate();
		}
		catch (AggregateException e)
		{
			logger.LogError(e.Message);
			foreach (var ex in e.InnerExceptions)
			{
				logger.LogError(ex.Message);
			}
			throw;
		}
		catch (Exception e)
		{
			logger.LogError(e.Message);
			throw;
		}
	}
}