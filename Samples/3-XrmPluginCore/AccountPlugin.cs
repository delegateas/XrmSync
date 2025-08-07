using BusinessDomain.Context;
using DG.XrmPluginCore;
using DG.XrmPluginCore.Enums;

namespace SamplePlugins {
    public class AccountPlugin : Plugin {

        public AccountPlugin() {
            // MODIFIED: Update step - changed filtered attributes and images to test Updates
            RegisterPluginStep<Account>(
                EventOperation.Update,
                ExecutionStage.PostOperation,
                Execute)
                .AddFilteredAttributes(x => x.Name, x => x.AccountNumber, x => x.Telephone1) // Added Telephone1
                .AddImage(ImageType.PreImage, x => x.Name, x => x.AccountNumber, x => x.Telephone1) // Added Telephone1
                .AddImage(ImageType.PostImage, x => x.Name, x => x.Telephone1); // Added Telephone1

            // EXISTING: Create step (same as SamplePlugins - no change)
            RegisterPluginStep<Account>(
                EventOperation.Create,
                ExecutionStage.PostOperation,
                Execute);

            // NEW: Create PreOperation step - this will be a CREATE difference
            RegisterPluginStep<Account>(
                EventOperation.Create,
                ExecutionStage.PreOperation,
                ExecuteCreatePreOp)
                .AddFilteredAttributes(x => x.AccountNumber, x => x.WebsiteUrl);

            // NEW: Delete step - this will be a CREATE difference (not in SamplePlugins)
            RegisterPluginStep<Account>(
                EventOperation.Delete,
                ExecutionStage.PreOperation,
                ExecuteDeletePreOp)
                .AddImage(ImageType.PreImage, x => x.Name, x => x.AccountNumber);

            // NEW: Additional Update step with different stage - CREATE difference
            RegisterPluginStep<Account>(
                EventOperation.Update,
                ExecutionStage.PreOperation,
                ExecuteUpdatePreOp)
                .AddFilteredAttributes(x => x.Description)
                .AddImage(ImageType.PreImage, x => x.Description);
        }

        protected void Execute(LocalPluginContext ctx) {
            ctx.Trace("Execute executed");
        }

        protected void ExecuteCreatePreOp(LocalPluginContext ctx)
        {
            ctx.Trace("ExecutePreOp executed");
        }

        protected void ExecuteDeletePreOp(LocalPluginContext ctx)
        {
            ctx.Trace("ExecuteDelete executed");
        }

        protected void ExecuteUpdatePreOp(LocalPluginContext ctx)
        {
            ctx.Trace("ExecuteUpdatePre executed");
        }
    }
}
