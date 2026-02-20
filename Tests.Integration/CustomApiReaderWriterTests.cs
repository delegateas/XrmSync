using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk.Query;
using Tests.Integration.Infrastructure;
using XrmPluginCore.Enums;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;

namespace Tests.Integration;

/// <summary>
/// Integration tests for ICustomApiReader and ICustomApiWriter.
/// </summary>
public sealed class CustomApiReaderWriterTests : TestBase
{
	#region Reader Tests

	[Fact]
	public void GetCustomApis_ReturnsEmpty_WhenNoApisInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("EmptyApiSolution");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<ICustomApiReader>();

		// Act
		var result = reader.GetCustomApis(solutionId);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetCustomApis_ReturnsFullyPopulatedDefinitions()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("ApiReaderSolution");
		var assemblyId = Producer.ProducePluginAssembly("ApiAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.ApiPlugin");

		var customApiId = Producer.ProduceCustomApi(
			uniqueName: "test_MyApi",
			name: "MyApi",
			displayName: "My Custom API",
			pluginTypeId: pluginTypeId,
			bindingType: 0,
			boundEntityLogicalName: null);

		Producer.ProduceSolutionComponent(solutionId, customApiId, componentType: 68); // 68 = CustomAPI

		var reqParamId = Producer.ProduceCustomApiRequestParameter(
			customApiId, "test_InputParam", "InputParam", "Input Parameter",
			type: 10, // String
			isOptional: false);

		var respPropId = Producer.ProduceCustomApiResponseProperty(
			customApiId, "test_OutputProp", "OutputProp", "Output Property",
			type: 10); // String

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<ICustomApiReader>();

		// Act
		var result = reader.GetCustomApis(solutionId);

		// Assert
		Assert.Single(result);
		var api = result[0];
		Assert.Equal(customApiId, api.Id);
		Assert.Equal("MyApi", api.Name);
		Assert.Equal("test_MyApi", api.UniqueName);
		Assert.Equal("My Custom API", api.DisplayName);
		Assert.Equal((BindingType)0, api.BindingType); // 0 = Global/Unbound
		Assert.Equal("Namespace.ApiPlugin", api.PluginType.Name);
		Assert.Equal(pluginTypeId, api.PluginType.Id);

		// Request parameters
		Assert.Single(api.RequestParameters);
		var reqParam = api.RequestParameters[0];
		Assert.Equal(reqParamId, reqParam.Id);
		Assert.Equal("InputParam", reqParam.Name);
		Assert.Equal("test_InputParam", reqParam.UniqueName);
		Assert.Equal("Input Parameter", reqParam.DisplayName);
		Assert.Equal(CustomApiParameterType.String, reqParam.Type);
		Assert.False(reqParam.IsOptional);

		// Response properties
		Assert.Single(api.ResponseProperties);
		var respProp = api.ResponseProperties[0];
		Assert.Equal(respPropId, respProp.Id);
		Assert.Equal("OutputProp", respProp.Name);
		Assert.Equal("test_OutputProp", respProp.UniqueName);
		Assert.Equal("Output Property", respProp.DisplayName);
		Assert.Equal(CustomApiParameterType.String, respProp.Type);
	}

	[Fact]
	public void GetCustomApis_FiltersApisNotInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("FilterApiSolution");
		var assemblyId = Producer.ProducePluginAssembly("FilterApiAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.FilterApiPlugin");

		// Create API but do NOT add solution component
		Producer.ProduceCustomApi("test_Orphan", "Orphan", "Orphan API", pluginTypeId);

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<ICustomApiReader>();

		// Act
		var result = reader.GetCustomApis(solutionId);

		// Assert
		Assert.Empty(result);
	}

	#endregion

	#region Writer Tests

	[Fact]
	public void CreateCustomApis_CreatesEntitiesAndSetsIds()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("CreateApiSolution");
		var assemblyId = Producer.ProducePluginAssembly("CreateApiAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.CreateApiPlugin");

		var apis = new List<CustomApiDefinition>
		{
			new("NewApi")
			{
				UniqueName = "test_NewApi",
				DisplayName = "New API",
				Description = "A new custom API",
				BoundEntityLogicalName = "",
				ExecutePrivilegeName = "",
				PluginType = new XrmSync.Model.CustomApi.PluginType("Namespace.CreateApiPlugin") { Id = pluginTypeId }
			}
		};

		var sp = BuildPluginServiceProvider("CreateApiSolution");
		var writer = sp.GetRequiredService<ICustomApiWriter>();

		// Act
		var result = writer.CreateCustomApis(apis, "test description");

		// Assert
		Assert.Single(result);
		Assert.NotEqual(Guid.Empty, result.First().Id);

		// Verify entity exists in Dataverse
		var retrieved = Service.Retrieve("customapi", result.First().Id, new ColumnSet("name", "uniquename", "displayname"));
		Assert.Equal("NewApi", retrieved.GetAttributeValue<string>("name"));
		Assert.Equal("test_NewApi", retrieved.GetAttributeValue<string>("uniquename"));
		Assert.Equal("New API", retrieved.GetAttributeValue<string>("displayname"));
	}

	[Fact]
	public void CreateRequestParameters_CreatesParametersLinkedToApi()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("CreateReqParamSolution");
		var assemblyId = Producer.ProducePluginAssembly("CreateReqParamAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.ReqParamPlugin");

		var customApiId = Producer.ProduceCustomApi("test_ParamApi", "ParamApi", "Param API", pluginTypeId);

		var api = new CustomApiDefinition("ParamApi")
		{
			Id = customApiId,
			UniqueName = "test_ParamApi",
			DisplayName = "Param API",
			Description = "",
			BoundEntityLogicalName = "",
			ExecutePrivilegeName = "",
			PluginType = new XrmSync.Model.CustomApi.PluginType("Namespace.ReqParamPlugin") { Id = pluginTypeId }
		};

		var param = new RequestParameter("InputParam")
		{
			UniqueName = "test_InputParam",
			DisplayName = "Input Parameter",
			IsCustomizable = true,
			IsOptional = false,
			LogicalEntityName = "",
			Type = CustomApiParameterType.String
		};

		var requestParams = new List<ParentReference<RequestParameter, CustomApiDefinition>>
		{
			new(param, api)
		};

		var sp = BuildPluginServiceProvider("CreateReqParamSolution");
		var writer = sp.GetRequiredService<ICustomApiWriter>();

		// Act
		var result = writer.CreateRequestParameters(requestParams);

		// Assert
		Assert.Single(result);
		Assert.NotEqual(Guid.Empty, result.First().Entity.Id);

		// Verify entity exists in Dataverse
		var retrieved = Service.Retrieve("customapirequestparameter", result.First().Entity.Id, new ColumnSet("name", "uniquename"));
		Assert.Equal("InputParam", retrieved.GetAttributeValue<string>("name"));
		Assert.Equal("test_InputParam", retrieved.GetAttributeValue<string>("uniquename"));
	}

	[Fact]
	public void CreateResponseProperties_CreatesPropertiesLinkedToApi()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("CreateRespPropSolution");
		var assemblyId = Producer.ProducePluginAssembly("CreateRespPropAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.RespPropPlugin");

		var customApiId = Producer.ProduceCustomApi("test_PropApi", "PropApi", "Prop API", pluginTypeId);

		var api = new CustomApiDefinition("PropApi")
		{
			Id = customApiId,
			UniqueName = "test_PropApi",
			DisplayName = "Prop API",
			Description = "",
			BoundEntityLogicalName = "",
			ExecutePrivilegeName = "",
			PluginType = new XrmSync.Model.CustomApi.PluginType("Namespace.RespPropPlugin") { Id = pluginTypeId }
		};

		var prop = new ResponseProperty("OutputProp")
		{
			UniqueName = "test_OutputProp",
			DisplayName = "Output Property",
			IsCustomizable = true,
			LogicalEntityName = "",
			Type = CustomApiParameterType.Integer
		};

		var responseProps = new List<ParentReference<ResponseProperty, CustomApiDefinition>>
		{
			new(prop, api)
		};

		var sp = BuildPluginServiceProvider("CreateRespPropSolution");
		var writer = sp.GetRequiredService<ICustomApiWriter>();

		// Act
		var result = writer.CreateResponseProperties(responseProps);

		// Assert
		Assert.Single(result);
		Assert.NotEqual(Guid.Empty, result.First().Entity.Id);

		// Verify entity exists in Dataverse
		var retrieved = Service.Retrieve("customapiresponseproperty", result.First().Entity.Id, new ColumnSet("name", "uniquename"));
		Assert.Equal("OutputProp", retrieved.GetAttributeValue<string>("name"));
		Assert.Equal("test_OutputProp", retrieved.GetAttributeValue<string>("uniquename"));
	}

	[Fact]
	public void UpdateCustomApis_UpdatesProperties()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("UpdateApiSolution");
		var assemblyId = Producer.ProducePluginAssembly("UpdateApiAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.UpdateApiPlugin");

		var customApiId = Producer.ProduceCustomApi("test_UpdateApi", "UpdateApi", "Original Display", pluginTypeId);

		var apis = new List<CustomApiDefinition>
		{
			new("UpdateApi")
			{
				Id = customApiId,
				UniqueName = "test_UpdateApi",
				DisplayName = "Updated Display",
				Description = "Updated description",
				BoundEntityLogicalName = "",
				ExecutePrivilegeName = "",
				PluginType = new XrmSync.Model.CustomApi.PluginType("Namespace.UpdateApiPlugin") { Id = pluginTypeId }
			}
		};

		var sp = BuildPluginServiceProvider("UpdateApiSolution");
		var writer = sp.GetRequiredService<ICustomApiWriter>();

		// Act
		writer.UpdateCustomApis(apis, "fallback description");

		// Assert
		var retrieved = Service.Retrieve("customapi", customApiId, new ColumnSet("displayname", "description"));
		Assert.Equal("Updated Display", retrieved.GetAttributeValue<string>("displayname"));
		Assert.Equal("Updated description", retrieved.GetAttributeValue<string>("description"));
	}

	[Fact]
	public void DeleteCustomApiDefinitions_RemovesEntities()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("DeleteApiSolution");
		var assemblyId = Producer.ProducePluginAssembly("DeleteApiAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.DeleteApiPlugin");

		var customApiId = Producer.ProduceCustomApi("test_DeleteApi", "DeleteApi", "Delete API", pluginTypeId);

		var apis = new List<CustomApiDefinition>
		{
			new("DeleteApi")
			{
				Id = customApiId,
				UniqueName = "test_DeleteApi",
				DisplayName = "Delete API",
				Description = "",
				BoundEntityLogicalName = "",
				ExecutePrivilegeName = "",
				PluginType = new XrmSync.Model.CustomApi.PluginType("Namespace.DeleteApiPlugin") { Id = pluginTypeId }
			}
		};

		var sp = BuildPluginServiceProvider("DeleteApiSolution");
		var writer = sp.GetRequiredService<ICustomApiWriter>();

		// Act
		writer.DeleteCustomApiDefinitions(apis);

		// Assert
		Assert.ThrowsAny<Exception>(() => Service.Retrieve("customapi", customApiId, new ColumnSet("name")));
	}

	[Fact]
	public void DeleteCustomApiRequestParameters_RemovesEntities()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("DeleteReqParamAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.DeleteReqParamPlugin");
		var customApiId = Producer.ProduceCustomApi("test_DelParamApi", "DelParamApi", "Del Param API", pluginTypeId);
		var paramId = Producer.ProduceCustomApiRequestParameter(customApiId, "test_Param", "Param", "Parameter", type: 10);

		var parameters = new List<RequestParameter>
		{
			new("Param")
			{
				Id = paramId,
				UniqueName = "test_Param",
				DisplayName = "Parameter",
				IsCustomizable = true,
				IsOptional = false,
				LogicalEntityName = "",
				Type = CustomApiParameterType.String
			}
		};

		var sp = BuildPluginServiceProvider("AnySolution");
		var writer = sp.GetRequiredService<ICustomApiWriter>();

		// Act
		writer.DeleteCustomApiRequestParameters(parameters);

		// Assert
		Assert.ThrowsAny<Exception>(() => Service.Retrieve("customapirequestparameter", paramId, new ColumnSet("name")));
	}

	[Fact]
	public void DeleteCustomApiResponseProperties_RemovesEntities()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("DeleteRespPropAssembly");
		var pluginTypeId = Producer.ProducePluginType(assemblyId, "Namespace.DeleteRespPropPlugin");
		var customApiId = Producer.ProduceCustomApi("test_DelPropApi", "DelPropApi", "Del Prop API", pluginTypeId);
		var propId = Producer.ProduceCustomApiResponseProperty(customApiId, "test_Prop", "Prop", "Property", type: 10);

		var properties = new List<ResponseProperty>
		{
			new("Prop")
			{
				Id = propId,
				UniqueName = "test_Prop",
				DisplayName = "Property",
				IsCustomizable = true,
				LogicalEntityName = "",
				Type = CustomApiParameterType.String
			}
		};

		var sp = BuildPluginServiceProvider("AnySolution");
		var writer = sp.GetRequiredService<ICustomApiWriter>();

		// Act
		writer.DeleteCustomApiResponseProperties(properties);

		// Assert
		Assert.ThrowsAny<Exception>(() => Service.Retrieve("customapiresponseproperty", propId, new ColumnSet("name")));
	}

	#endregion
}
