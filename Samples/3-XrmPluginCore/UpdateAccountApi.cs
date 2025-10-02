namespace SamplePlugins
{
    using XrmPluginCore;
    using XrmPluginCore.Enums;
    using Microsoft.Xrm.Sdk;
    using System;

    // NEW CustomAPI - will be a CREATE difference (doesn't exist in SamplePlugins)
    public class UpdateAccountApi : Plugin
    {
        public UpdateAccountApi()
        {
            RegisterCustomAPI(nameof(UpdateAccountApi), Execute)
                .AddRequestParameter("AccountId", CustomApiParameterType.Guid)
                .AddRequestParameter("NewName", CustomApiParameterType.String)
                .AddRequestParameter("Industry", CustomApiParameterType.String, isOptional: true)
                .AddResponseProperty("Success", CustomApiParameterType.Boolean)
                .AddResponseProperty("UpdatedName", CustomApiParameterType.String)
                .Bind<BusinessDomain.Context.Account>(BindingType.Entity)
                .AllowCustomProcessingStep(AllowedCustomProcessingStepType.SyncAndAsync)
                .SetDescription("Description"); // This matches the DAXIF description
        }

        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException("localContext");
            }

            var service = localContext.OrganizationService;

            var accountId = (Guid)localContext.PluginExecutionContext.InputParameters["AccountId"];
            var newName = localContext.PluginExecutionContext.InputParameters["NewName"] as string;
            var industry = localContext.PluginExecutionContext.InputParameters.ContainsKey("Industry") ? 
                localContext.PluginExecutionContext.InputParameters["Industry"] as string : null;

            var account = new Entity("account", accountId)
            {
                ["name"] = newName
            };

            if (!string.IsNullOrEmpty(industry))
            {
                account["industrycode"] = industry;
            }

            service.Update(account);

            localContext.PluginExecutionContext.OutputParameters["Success"] = true;
            localContext.PluginExecutionContext.OutputParameters["UpdatedName"] = newName;
        }
    }
}