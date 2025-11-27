using Microsoft.Xrm.Sdk;

namespace SamplePlugins
{
	internal interface IAccountService
	{
		void TraceExecution();
	}

	internal class AccountService : IAccountService
	{
		private readonly ITracingService tracingService;
		private readonly IPluginExecutionContext context;

		public AccountService(ITracingService tracingService, IPluginExecutionContext context)
		{
			this.tracingService = tracingService;
			this.context = context;
		}

		public void TraceExecution()
		{
			tracingService.Trace("{stage} {message} executed against {entity} with ID {id}",
				context.Stage,
				context.MessageName,
				context.PrimaryEntityName,
				context.PrimaryEntityId);
		}
	}
}
