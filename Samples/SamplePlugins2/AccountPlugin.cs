
using System;
using BusinessDomain.Context;

namespace SamplePlugins {
    public class AccountPlugin : Plugin {

        public AccountPlugin() : base(typeof(AccountPlugin)) {
            RegisterPluginStep<Account>(
                EventOperation.Update,
                ExecutionStage.PostOperation,
                Execute)
                .AddFilteredAttributes(x => x.Name, x => x.AccountNumber)
                .AddImage(ImageType.PreImage, x => x.Name, x => x.AccountNumber)
                .AddImage(ImageType.PostImage, x => x.Name);

            RegisterPluginStep<Account>(
                EventOperation.Create,
                ExecutionStage.PostOperation,
                Execute);

            RegisterPluginStep<Account>(
                EventOperation.Create,
                ExecutionStage.PreOperation,
                ExecutePreOp)
                .AddFilteredAttributes(x => x.AccountNumber);
        }

        protected void Execute(LocalPluginContext localContext) {
            if (localContext == null) {
                throw new ArgumentNullException(nameof(localContext));
            }

            if (localContext.PluginExecutionContext == null)
            {
                throw new ArgumentNullException(nameof(localContext.PluginExecutionContext));
            }

            if (localContext.OrganizationService == null)
            {
                throw new ArgumentNullException(nameof(localContext.OrganizationService));
            }

            if (localContext.TracingService == null)
            {
                throw new ArgumentNullException(nameof(localContext.TracingService));
            }

            if (localContext.OrganizationAdminService == null)
            {
                throw new ArgumentNullException(nameof(localContext.OrganizationAdminService));
            }

            var service = localContext.OrganizationService;

            var account = GetEntity<Account>(localContext);
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            var rand = new Random();
            service.Create(new Lead() {
                Subject = nameof(AccountPlugin) + " " + localContext.PluginExecutionContext.MessageName + ": Some new lead " + rand.Next(0, 1000),
                ParentAccountId = new Account(localContext.PluginExecutionContext.PrimaryEntityId).ToEntityReference()
            });
        }

        protected void ExecutePreOp(LocalPluginContext ctx)
        {
            ctx.Trace("ExecutePreOp executed");
        }
    }
}
