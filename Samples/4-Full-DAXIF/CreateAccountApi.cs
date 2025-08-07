namespace SamplePlugins
{
    using System;
    using Microsoft.Xrm.Sdk;

    public class CreateAccountApi : CustomAPI
    {
        public CreateAccountApi() : base(typeof(CreateAccountApi))
        {
            // MODIFIED: Changed the API configuration to test Updates
            RegisterCustomAPI(nameof(CreateAccountApi), Execute)
                .AddRequestParameter(new CustomAPIConfig.CustomAPIRequestParameter("Name", RequestParameterType.String))
                .AddRequestParameter(new CustomAPIConfig.CustomAPIRequestParameter("Phone", RequestParameterType.String, isOptional: true)) // Added optional parameter
                .AddResponseProperty(new CustomAPIConfig.CustomAPIResponseProperty("Name", RequestParameterType.String))
                .AddResponseProperty(new CustomAPIConfig.CustomAPIResponseProperty("AccountId", RequestParameterType.Guid)) // Added response property
                .MakePrivate(); // Make it private
        }

        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException("localContext");
            }

            var service = localContext.OrganizationService;

            var name = localContext.PluginExecutionContext.InputParameters["Name"] as string;
            var phone = localContext.PluginExecutionContext.InputParameters.ContainsKey("Phone") ? 
                localContext.PluginExecutionContext.InputParameters["Phone"] as string : null;

            var account = new Entity("account")
            {
                ["name"] = name
            };

            if (!string.IsNullOrEmpty(phone))
            {
                account["telephone1"] = phone;
            }

            var accountId = service.Create(account);

            localContext.PluginExecutionContext.OutputParameters["Name"] = name;
            localContext.PluginExecutionContext.OutputParameters["AccountId"] = accountId;
        }
    }
}