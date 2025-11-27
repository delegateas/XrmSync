namespace SamplePlugins
{
	using System;
	using XrmPluginCore;
	using XrmPluginCore.Enums;

	// Another NEW CustomAPI - will be a CREATE difference
	public class DeleteAccountApi : Plugin
	{
		public DeleteAccountApi()
		{
			RegisterCustomAPI(nameof(DeleteAccountApi), Execute)
				.AddRequestParameter("AccountId", CustomApiParameterType.Guid)
				.AddRequestParameter("ForceDelete", CustomApiParameterType.Boolean, isOptional: true)
				.AddResponseProperty("Deleted", CustomApiParameterType.Boolean)
				.AddResponseProperty("Message", CustomApiParameterType.String)
				.MakePrivate()
				.SetDescription("Description") // This matches the DAXIF description
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
