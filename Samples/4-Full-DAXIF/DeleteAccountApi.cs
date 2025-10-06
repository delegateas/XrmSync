namespace SamplePlugins
{
    using System;

    // Another NEW CustomAPI - will be a CREATE difference
    public class DeleteAccountApi : CustomAPI
    {
        public DeleteAccountApi() : base(typeof(DeleteAccountApi))
        {
            RegisterCustomAPI(nameof(DeleteAccountApi), Execute)
                .AddRequestParameter(new CustomAPIConfig.CustomAPIRequestParameter("AccountId", RequestParameterType.Guid))
                .AddRequestParameter(new CustomAPIConfig.CustomAPIRequestParameter("ForceDelete", RequestParameterType.Boolean, isOptional: true))
                .AddResponseProperty(new CustomAPIConfig.CustomAPIResponseProperty("Deleted", RequestParameterType.Boolean))
                .AddResponseProperty(new CustomAPIConfig.CustomAPIResponseProperty("Message", RequestParameterType.String))
                .MakePrivate()
                .EnableCustomization();
        }

        protected void Execute(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException("localContext");
            }

            var service = localContext.OrganizationService;

            var accountId = (Guid)localContext.PluginExecutionContext.InputParameters["AccountId"];
            var forceDelete = localContext.PluginExecutionContext.InputParameters.ContainsKey("ForceDelete") ? 
                (bool)localContext.PluginExecutionContext.InputParameters["ForceDelete"] : false;

            try
            {
                service.Delete("account", accountId);
                localContext.PluginExecutionContext.OutputParameters["Deleted"] = true;
                localContext.PluginExecutionContext.OutputParameters["Message"] = "Account deleted successfully";
            }
            catch (Exception ex)
            {
                if (forceDelete)
                {
                    throw;
                }
                localContext.PluginExecutionContext.OutputParameters["Deleted"] = false;
                localContext.PluginExecutionContext.OutputParameters["Message"] = $"Failed to delete account: {ex.Message}";
            }
        }
    }
}