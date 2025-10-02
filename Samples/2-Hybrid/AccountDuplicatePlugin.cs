using BusinessDomain.Context;
using CorePlugin = XrmPluginCore.Plugin;
using EO = XrmPluginCore.Enums.EventOperation;
using ES = XrmPluginCore.Enums.ExecutionStage;
using IT = XrmPluginCore.Enums.ImageType;
using Context = XrmPluginCore.LocalPluginContext;

namespace SamplePlugins
{
    // NEW Plugin - will be a CREATE difference (doesn't exist in SamplePlugins)
    public class AccountDuplicatePlugin : CorePlugin
    {
        public AccountDuplicatePlugin()
        {
            RegisterPluginStep<Account>(
                EO.Create,
                ES.PreOperation,
                Execute)
                .AddFilteredAttributes(x => x.Name, x => x.EmailAddress1);

            RegisterPluginStep<Account>(
                EO.Update,
                ES.PostOperation,
                ExecuteUpdate)
                .AddFilteredAttributes(x => x.Telephone1, x => x.Fax)
                .AddImage(IT.PreImage, x => x.Telephone1)
                .AddImage(IT.PostImage, x => x.Telephone1, x => x.Fax);
        }

        protected void Execute(Context localContext)
        {
            localContext.Trace($"AccountDuplicatePlugin executed for {localContext.PluginExecutionContext.MessageName}");
        }

        protected void ExecuteUpdate(Context localContext) 
        {
            localContext.Trace("AccountDuplicatePlugin ExecuteUpdate executed");
        }
    }
}