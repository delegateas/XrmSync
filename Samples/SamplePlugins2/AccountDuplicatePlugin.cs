using System;
using BusinessDomain.Context;

namespace SamplePlugins 
{
    // NEW Plugin - will be a CREATE difference (doesn't exist in SamplePlugins)
    public class AccountDuplicatePlugin : Plugin
    {
        public AccountDuplicatePlugin() : base(typeof(AccountDuplicatePlugin)) 
        {
            RegisterPluginStep<Account>(
                EventOperation.Create,
                ExecutionStage.PreOperation,
                Execute)
                .AddFilteredAttributes(x => x.Name, x => x.EmailAddress1);

            RegisterPluginStep<Account>(
                EventOperation.Update,
                ExecutionStage.PostOperation,
                ExecuteUpdate)
                .AddFilteredAttributes(x => x.Telephone1, x => x.Fax)
                .AddImage(ImageType.PreImage, x => x.Telephone1)
                .AddImage(ImageType.PostImage, x => x.Telephone1, x => x.Fax);
        }

        protected void Execute(LocalPluginContext localContext)
        {
            localContext.Trace($"AccountDuplicatePlugin executed for {localContext.PluginExecutionContext.MessageName}");
        }

        protected void ExecuteUpdate(LocalPluginContext localContext) 
        {
            localContext.Trace("AccountDuplicatePlugin ExecuteUpdate executed");
        }
    }
}