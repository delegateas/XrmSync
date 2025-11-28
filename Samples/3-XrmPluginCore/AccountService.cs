using Microsoft.Xrm.Sdk;
using UpdatePreImage = SamplePlugins.PluginRegistrations.AccountPlugin.AccountUpdatePostOperation.PreImage;
using UpdatePostImage = SamplePlugins.PluginRegistrations.AccountPlugin.AccountUpdatePostOperation.PostImage;
using DeletePreImage = SamplePlugins.PluginRegistrations.AccountPlugin.AccountDeletePreOperation.PreImage;
using PreUpdatePreImage = SamplePlugins.PluginRegistrations.AccountPlugin.AccountUpdatePreOperation.PreImage;

namespace SamplePlugins
{
	internal interface IAccountService
	{
		void HandleUpdate(PreUpdatePreImage preImage);
		void HandleUpdate(UpdatePreImage preImage, UpdatePostImage postImage);
		void HandleDelete(DeletePreImage preImage);
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

		public void HandleUpdate(PreUpdatePreImage preImage)
		{
			TraceExecution();
			tracingService.Trace("PreImage Description: {0}", preImage.Description);
		}

		public void HandleUpdate(UpdatePreImage preImage, UpdatePostImage postImage)
		{
			TraceExecution();
			tracingService.Trace("PreImage Name: {0}, AccountNumber: {1}, PostImage Name: {2}, AccountNumber: {3}", preImage.Name, preImage.AccountNumber, postImage.Name);
		}

		public void HandleDelete(DeletePreImage preImage)
		{
			TraceExecution();
			tracingService.Trace("PreImage Name: {0}, AccountNumber: {1}", preImage.Name, preImage.AccountNumber);
		}
	}
}
