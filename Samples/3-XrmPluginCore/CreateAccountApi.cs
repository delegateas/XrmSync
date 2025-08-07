namespace SamplePlugins
{
    using System;
    using DG.XrmPluginCore;
    using DG.XrmPluginCore.Enums;
    using Microsoft.Xrm.Sdk;

    public class CreateAccountApi : CustomAPI
    {
        public CreateAccountApi()
        {
            // MODIFIED: Changed the API configuration to test Updates
            RegisterCustomAPI(nameof(CreateAccountApi), Execute)
                .AddRequestParameter("Name", CustomApiParameterType.String)
                .AddRequestParameter("Phone", CustomApiParameterType.String, isOptional: true) // Added optional parameter
                .AddResponseProperty("Name", CustomApiParameterType.String)
                .AddResponseProperty("AccountId", CustomApiParameterType.Guid) // Added response property
                .SetDescription("Description") // This matches the DAXIF description
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