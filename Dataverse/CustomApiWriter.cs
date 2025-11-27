using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;

namespace XrmSync.Dataverse;

internal class CustomApiWriter(IDataverseWriter writer, IOptions<PluginSyncCommandOptions> configuration) : ICustomApiWriter
{
	private Dictionary<string, object> Parameters { get; } = new() {
			{ "SolutionUniqueName", configuration.Value.SolutionName }
	};

	public ICollection<CustomApiDefinition> CreateCustomApis(ICollection<CustomApiDefinition> customApis, string description)
	{
		if (customApis.Count == 0)
			return customApis;

		foreach (var api in customApis)
		{
			var entity = new CustomAPI
			{
				Name = api.Name,
				UniqueName = api.UniqueName,
				DisplayName = api.DisplayName,
				Description = GetDescription(api, description),
				IsFunction = api.IsFunction,
				WorkflowSdkStepEnabled = api.EnabledForWorkflow,
				BindingType = (customapi_bindingtype)api.BindingType,
				BoundEntityLogicalName = api.BoundEntityLogicalName,
				AllowedCustomProcessingStepType = (customapi_allowedcustomprocessingsteptype)api.AllowedCustomProcessingStepType,
				PluginTypeId = new EntityReference(Context.PluginType.EntityLogicalName, api.PluginType.Id),
				IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
				IsPrivate = api.IsPrivate,
				ExecutePrivilegeName = api.ExecutePrivilegeName
			};

			if (api.OwnerId != Guid.Empty)
			{
				entity.OwnerId = new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);
			}

			api.Id = writer.Create(entity, Parameters);
		}

		return customApis;
	}

	public ICollection<ParentReference<RequestParameter, CustomApiDefinition>> CreateRequestParameters(ICollection<ParentReference<RequestParameter, CustomApiDefinition>> requestParameters)
	{
		if (requestParameters.Count == 0)
			return [];

		foreach (var (param, customApi) in requestParameters)
		{
			var entity = new CustomAPIRequestParameter
			{
				CustomAPIId = new EntityReference(CustomAPI.EntityLogicalName, customApi.Id),
				Name = param.Name,
				UniqueName = param.UniqueName,
				DisplayName = param.DisplayName,
				IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
				IsOptional = param.IsOptional,
				LogicalEntityName = param.LogicalEntityName,
				Type = (customapifieldtype)param.Type
			};

			param.Id = writer.Create(entity, Parameters);
		}

		return requestParameters;
	}

	public ICollection<ParentReference<ResponseProperty, CustomApiDefinition>> CreateResponseProperties(ICollection<ParentReference<ResponseProperty, CustomApiDefinition>> responseProperties)
	{
		if (responseProperties.Count == 0)
			return responseProperties;

		foreach (var (prop, customApi) in responseProperties)
		{
			var entity = new CustomAPIResponseProperty
			{
				Name = prop.Name,
				UniqueName = prop.UniqueName,
				CustomAPIId = new EntityReference(CustomAPI.EntityLogicalName, customApi.Id),
				DisplayName = prop.DisplayName,
				IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
				LogicalEntityName = prop.LogicalEntityName,
				Type = (customapifieldtype)prop.Type
			};

			prop.Id = writer.Create(entity, Parameters);
		}

		return responseProperties;
	}

	public void UpdateCustomApis(ICollection<CustomApiDefinition> customApis, string description)
	{
		var updateRequests = customApis.Select(api =>
		{
			var definition = new CustomAPI
			{
				Id = api.Id,
				DisplayName = api.DisplayName,
				Description = GetDescription(api, description),
				IsFunction = api.IsFunction,
				WorkflowSdkStepEnabled = api.EnabledForWorkflow,
				BindingType = (customapi_bindingtype)api.BindingType,
				BoundEntityLogicalName = api.BoundEntityLogicalName,
				AllowedCustomProcessingStepType = (customapi_allowedcustomprocessingsteptype)api.AllowedCustomProcessingStepType,
				PluginTypeId = new EntityReference(Context.PluginType.EntityLogicalName, api.PluginType.Id),
				IsCustomizable = new BooleanManagedProperty(api.IsCustomizable),
				IsPrivate = api.IsPrivate,
				ExecutePrivilegeName = api.ExecutePrivilegeName
			};

			if (api.OwnerId != Guid.Empty)
			{
				definition.OwnerId = new EntityReference(SystemUser.EntityLogicalName, api.OwnerId);
			}

			return definition;
		}).ToList();

		if (updateRequests.Count > 0)
		{
			writer.UpdateMultiple(updateRequests);
		}
	}

	public void UpdateRequestParameters(ICollection<RequestParameter> requestParameters)
	{
		var updateRequests = requestParameters.Select(param => new CustomAPIRequestParameter()
		{
			Id = param.Id,
			DisplayName = param.DisplayName,
			IsCustomizable = new BooleanManagedProperty(param.IsCustomizable),
			IsOptional = param.IsOptional,
			LogicalEntityName = param.LogicalEntityName,
			Type = (customapifieldtype?)param.Type
		}).ToList();

		if (updateRequests.Count > 0)
			writer.UpdateMultiple(updateRequests);
	}

	public void UpdateResponseProperties(ICollection<ResponseProperty> responseProperties)
	{
		var updateRequests = responseProperties.Select(prop => new CustomAPIResponseProperty()
		{
			Id = prop.Id,
			DisplayName = prop.DisplayName,
			IsCustomizable = new BooleanManagedProperty(prop.IsCustomizable),
			LogicalEntityName = prop.LogicalEntityName,
			Type = (customapifieldtype?)prop.Type
		}).ToList();

		if (updateRequests.Count > 0)
			writer.UpdateMultiple(updateRequests);
	}

	public void DeleteCustomApiDefinitions(IEnumerable<CustomApiDefinition> customApis)
	{
		writer.DeleteMultiple(customApis.ToDeleteRequests(CustomAPI.EntityLogicalName));
	}

	public void DeleteCustomApiRequestParameters(IEnumerable<RequestParameter> requestParameters)
	{
		writer.DeleteMultiple(requestParameters.ToDeleteRequests(CustomAPIRequestParameter.EntityLogicalName));
	}

	public void DeleteCustomApiResponseProperties(IEnumerable<ResponseProperty> responseProperties)
	{
		writer.DeleteMultiple(responseProperties.ToDeleteRequests(CustomAPIResponseProperty.EntityLogicalName));
	}

	private static string GetDescription(CustomApiDefinition api, string description)
	{
		return !string.IsNullOrEmpty(api.Description) && !api.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase)
			? api.Description
			: description;
	}
}
