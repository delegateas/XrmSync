using BusinessDomain.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using System;
using XrmPluginCore;
using XrmPluginCore.Enums;

namespace SamplePlugins 
{
    // NEW Plugin - will be a CREATE difference (doesn't exist in SamplePlugins)
    public class AccountDuplicatePlugin : Plugin
    {
        public AccountDuplicatePlugin()
        {
            RegisterStep<Account>(
                EventOperation.Create,
                ExecutionStage.PreOperation,
                ExecuteCreate)
                .AddFilteredAttributes(x => x.Name, x => x.EmailAddress1);

            RegisterStep<Account>(
                EventOperation.Update,
                ExecutionStage.PostOperation,
                ExecuteUpdate)
                .AddFilteredAttributes(x => x.Telephone1, x => x.Fax)
                .AddImage(ImageType.PreImage, x => x.Telephone1)
                .AddImage(ImageType.PostImage, x => x.Telephone1, x => x.Fax);
        }

        protected void ExecuteCreate(IServiceProvider serviceProvider)
        {
            var tracingService = serviceProvider.GetService<ITracingService>();
            var pluginContext = serviceProvider.GetService<IPluginExecutionContext>();
            tracingService.Trace($"AccountDuplicatePlugin executed for {pluginContext.MessageName}");
        }

        protected void ExecuteUpdate(IServiceProvider serviceProvider)
        {
            var tracingService = serviceProvider.GetService<ITracingService>();
            tracingService.Trace("AccountDuplicatePlugin ExecuteUpdate executed");
        }
    }
}