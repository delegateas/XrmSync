using BusinessDomain.Context;
using XrmPluginCore;
using XrmPluginCore.Enums;

namespace SamplePlugins
{
	public class AccountPlugin : Plugin
	{

		public AccountPlugin()
		{
			// MODIFIED: Update step - changed filtered attributes and images to test Updates
			RegisterStep<Account, IAccountService>(
				EventOperation.Update,
				ExecutionStage.PostOperation,
				s => s.TraceExecution())
				.AddFilteredAttributes(x => x.Name, x => x.AccountNumber, x => x.Telephone1) // Added Telephone1
				.AddImage(ImageType.PreImage, x => x.Name, x => x.AccountNumber, x => x.Telephone1) // Added Telephone1
				.AddImage(ImageType.PostImage, x => x.Name, x => x.Telephone1); // Added Telephone1

			// EXISTING: Create step (same as SamplePlugins - no change)
			RegisterStep<Account, IAccountService>(
				EventOperation.Create,
				ExecutionStage.PostOperation,
				s => s.TraceExecution());

			// NEW: Create PreOperation step - this will be a CREATE difference
			RegisterStep<Account, IAccountService>(
				EventOperation.Create,
				ExecutionStage.PreOperation,
				s => s.TraceExecution())
				.AddFilteredAttributes(x => x.AccountNumber, x => x.WebsiteUrl);

			// NEW: Delete step - this will be a CREATE difference (not in SamplePlugins)
			RegisterStep<Account, IAccountService>(
				EventOperation.Delete,
				ExecutionStage.PreOperation,
				s => s.TraceExecution())
				.AddImage(ImageType.PreImage, x => x.Name, x => x.AccountNumber);

			// NEW: Additional Update step with different stage - CREATE difference
			RegisterStep<Account, IAccountService>(
				EventOperation.Update,
				ExecutionStage.PreOperation,
				s => s.TraceExecution())
				.AddFilteredAttributes(x => x.Description)
				.AddImage(ImageType.PreImage, x => x.Description);
		}
	}
}
