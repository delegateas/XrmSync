
using System;
using BusinessDomain.Context;

namespace SamplePlugins {
    public class AccountPlugin : Plugin {

        public AccountPlugin() : base(typeof(AccountPlugin)) {
            RegisterPluginStep<Account>(
                EventOperation.Update,
                ExecutionStage.PostOperation,
                Execute)
                .AddFilteredAttributes(x => x.Name)
                .AddImage(ImageType.PreImage, x => x.Name);

            RegisterPluginStep<Account>(
                EventOperation.Create,
                ExecutionStage.PostOperation,
                Execute);
        }

        protected void Execute(LocalPluginContext localContext) {
            localContext.Trace("Execute executed");
        }
    }
}
