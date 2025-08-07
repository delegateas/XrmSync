namespace SamplePlugins
{
    using Microsoft.Xrm.Sdk;
    using System;

    // NEW CustomAPI - will be a CREATE difference (doesn't exist in SamplePlugins)
    public class UpdateAccountApi : CustomAPI
    {
        public UpdateAccountApi() : base(typeof(UpdateAccountApi))
        {
            RegisterCustomAPI(nameof(UpdateAccountApi), Execute)
                .AddRequestParameter(new CustomAPIConfig.CustomAPIRequestParameter("AccountId", RequestParameterType.Guid))
                .AddRequestParameter(new CustomAPIConfig.CustomAPIRequestParameter("NewName", RequestParameterType.String))
                .AddRequestParameter(new CustomAPIConfig.CustomAPIRequestParameter("Industry", RequestParameterType.String, isOptional: true))
                .AddResponseProperty(new CustomAPIConfig.CustomAPIResponseProperty("Success", RequestParameterType.Boolean))
                .AddResponseProperty(new CustomAPIConfig.CustomAPIResponseProperty("UpdatedName", RequestParameterType.String))
                .Bind<BusinessDomain.Context.Account>(BindingType.Entity)
                .AllowCustomProcessingStep(AllowedCustomProcessingStepType.SyncAndAsync);
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