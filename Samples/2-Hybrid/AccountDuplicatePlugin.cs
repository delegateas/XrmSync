using BusinessDomain.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using System;
using XrmPluginCore;
using CorePlugin = XrmPluginCore.Plugin;
using EO = XrmPluginCore.Enums.EventOperation;
using ES = XrmPluginCore.Enums.ExecutionStage;
using IT = XrmPluginCore.Enums.ImageType;

namespace SamplePlugins
{
    // NEW Plugin - will be a CREATE difference (doesn't exist in SamplePlugins)
    public class AccountDuplicatePlugin : CorePlugin
    {
        public AccountDuplicatePlugin()
        {
            RegisterStep<Account>(
                EO.Create,
                ES.PreOperation,
                ExecuteCreate)
                .AddFilteredAttributes(x => x.Name, x => x.EmailAddress1);

#pragma warning disable CS0618 // Type or member is obsolete
            RegisterPluginStep<Account>(
                EO.Update,
                ES.PostOperation,
                ExecuteUpdate)
                .AddFilteredAttributes(x => x.Telephone1, x => x.Fax)
                .AddImage(IT.PreImage, x => x.Telephone1)
                .AddImage(IT.PostImage, x => x.Telephone1, x => x.Fax);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        protected void ExecuteCreate(IServiceProvider serviceProvider)
        {
            var tracingService = serviceProvider.GetService<ITracingService>();
            var pluginContext = serviceProvider.GetService<IPluginExecutionContext>();
            tracingService.Trace($"AccountDuplicatePlugin executed for {pluginContext.MessageName}");
        }

        protected void ExecuteUpdate(LocalPluginContext context)
        {
            context.Trace("AccountDuplicatePlugin ExecuteUpdate executed");
        }
    }
}