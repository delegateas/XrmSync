using XrmPluginCore.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.SyncService;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.Difference;
using XrmSync.SyncService.Extensions;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;

namespace Tests.CustomApis;

public class DifferenceCalculatorCustomApiTests
{
	private readonly DifferenceCalculator differenceCalculator;

	public DifferenceCalculatorCustomApiTests()
	{
		var logger = new LoggerFactory().CreateLogger<PrintService>();
		var description = new Description();
		var options = new ExecutionModeOptions(true);
		differenceCalculator = new DifferenceCalculator(
			new PluginDefinitionComparer(),
			new PluginStepComparer(),
			new PluginImageComparer(),
			new CustomApiComparer(description),
			new RequestParameterComparer(),
			new ResponsePropertyComparer(),
			new PrintService(logger, Options.Create(options), new Description(), Substitute.For<IDataverseReader>())
		);
	}

	private static AssemblyInfo CreateAssemblyInfo(List<CustomApiDefinition> customApis) => new("TestAssembly")
	{
		Id = Guid.Empty,
		DllPath = "test.dll",
		Hash = "hash",
		Version = "1.0.0",
		CustomApis = customApis,
		Plugins = []
	};

	private static CustomApiDefinition CreateCustomApi(string name, Guid? id = null) => new(name)
	{
		Id = id ?? Guid.NewGuid(),
		PluginType = new PluginType("TestPluginType") { Id = Guid.NewGuid() },
		UniqueName = $"new_{name}",
		DisplayName = name,
		Description = "Test description",
		IsFunction = false,
		EnabledForWorkflow = false,
		AllowedCustomProcessingStepType = (AllowedCustomProcessingStepType)0,
		BindingType = (BindingType)0,
		BoundEntityLogicalName = string.Empty,
		IsCustomizable = true,
		OwnerId = Guid.Empty,
		IsPrivate = false,
		ExecutePrivilegeName = string.Empty,
		RequestParameters = [],
		ResponseProperties = []
	};

	private static RequestParameter CreateRequestParameter(string name, Guid? id = null) => new(name)
	{
		Id = id ?? Guid.NewGuid(),
		UniqueName = $"new_{name}",
		DisplayName = name,
		IsCustomizable = true,
		IsOptional = false,
		LogicalEntityName = string.Empty,
		Type = CustomApiParameterType.String
	};

	private static ResponseProperty CreateResponseProperty(string name, Guid? id = null) => new(name)
	{
		Id = id ?? Guid.NewGuid(),
		UniqueName = $"new_{name}",
		DisplayName = name,
		IsCustomizable = true,
		LogicalEntityName = string.Empty,
		Type = CustomApiParameterType.String
	};

	// ========== CustomApiDefinition Tests ==========

	[Fact]
	public void NewCustomApiDetectedAsCreate()
	{
		// Arrange
		var newApi = CreateCustomApi("NewApi", id: Guid.Empty);
		var local = CreateAssemblyInfo([newApi]);
		var remote = CreateAssemblyInfo([]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Single(differences.CustomApis.Creates);
		Assert.Equal("NewApi", differences.CustomApis.Creates[0].Local.Name);
		Assert.Empty(differences.CustomApis.Updates);
		Assert.Empty(differences.CustomApis.Deletes);
	}

	[Fact]
	public void RemovedCustomApiDetectedAsDelete()
	{
		// Arrange
		var remoteApi = CreateCustomApi("OldApi");
		var local = CreateAssemblyInfo([]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Single(differences.CustomApis.Deletes);
		Assert.Equal("OldApi", differences.CustomApis.Deletes[0].Name);
		Assert.Empty(differences.CustomApis.Creates);
		Assert.Empty(differences.CustomApis.Updates);
	}

	[Fact]
	public void IdenticalCustomApisProduceNoDifferences()
	{
		// Arrange
		var sharedId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var api = CreateCustomApi("SharedApi", id: sharedId);
		api.PluginType = new PluginType("SharedPluginType") { Id = pluginTypeId };

		var remoteApi = CreateCustomApi("SharedApi", id: sharedId);
		remoteApi.PluginType = new PluginType("SharedPluginType") { Id = pluginTypeId };

		var local = CreateAssemblyInfo([api]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Empty(differences.CustomApis.Creates);
		Assert.Empty(differences.CustomApis.Updates);
		Assert.Empty(differences.CustomApis.Deletes);
	}

	[Fact]
	public void UpdatablePropertyChangesDetectedAsUpdates()
	{
		// Arrange
		var sharedId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var localApi = CreateCustomApi("LocalName", id: sharedId);
		localApi.UniqueName = "new_shared_api"; // Keep UniqueName the same to avoid recreate
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.DisplayName = "LocalDisplayName";
		localApi.IsPrivate = true;
		localApi.ExecutePrivilegeName = "new_local_privilege";

		var remoteApi = CreateCustomApi("RemoteName", id: sharedId);
		remoteApi.UniqueName = "new_shared_api"; // Keep UniqueName the same to avoid recreate
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.DisplayName = "RemoteDisplayName";
		remoteApi.IsPrivate = false;
		remoteApi.ExecutePrivilegeName = "new_remote_privilege";

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Empty(differences.CustomApis.Creates);
		Assert.Empty(differences.CustomApis.Deletes);
		Assert.Single(differences.CustomApis.Updates);

		var update = differences.CustomApis.Updates[0];
		var propNames = update.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
		Assert.Equal([
			nameof(CustomApiDefinition.DisplayName),
			nameof(CustomApiDefinition.ExecutePrivilegeName),
			nameof(CustomApiDefinition.IsPrivate),
			nameof(CustomApiDefinition.Name)
		], propNames);
	}

	[Fact]
	public void PluginTypeNameChangeDetectedAsUpdate()
	{
		// Arrange
		var sharedId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var localApi = CreateCustomApi("TestApi", id: sharedId);
		localApi.PluginType = new PluginType("LocalPluginType") { Id = pluginTypeId };

		var remoteApi = CreateCustomApi("TestApi", id: sharedId);
		remoteApi.PluginType = new PluginType("RemotePluginType") { Id = pluginTypeId };

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Empty(differences.CustomApis.Creates);
		Assert.Empty(differences.CustomApis.Deletes);
		Assert.Single(differences.CustomApis.Updates);

		var update = differences.CustomApis.Updates[0];
		var propNames = update.DifferentProperties.Select(p => p.GetMemberName()).ToArray();
		Assert.Equal([nameof(CustomApiDefinition.PluginType)], propNames);
	}

	[Fact]
	public void DescriptionSyncedWithTreatedAsEqual()
	{
		// Arrange
		var sharedId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var description = new Description();

		var localApi = CreateCustomApi("TestApi", id: sharedId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.Description = $"Synced with {description.ToolHeader} by 'user' at 2025-01-01";

		var remoteApi = CreateCustomApi("TestApi", id: sharedId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.Description = "Some completely different description";

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert — Description should not be in the diff since local starts with "Synced with {ToolHeader}"
		Assert.Empty(differences.CustomApis.Creates);
		Assert.Empty(differences.CustomApis.Deletes);
		Assert.Empty(differences.CustomApis.Updates);
	}

	[Fact]
	public void DescriptionLiteralDescriptionTreatedAsEqual()
	{
		// Arrange
		var sharedId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var localApi = CreateCustomApi("TestApi", id: sharedId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.Description = "Description"; // case insensitive match for "description"

		var remoteApi = CreateCustomApi("TestApi", id: sharedId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.Description = "Some completely different description text";

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert — Description should not be in the diff since local equals "description" (case insensitive)
		Assert.Empty(differences.CustomApis.Creates);
		Assert.Empty(differences.CustomApis.Deletes);
		Assert.Empty(differences.CustomApis.Updates);
	}

	[Fact]
	public void MixedCreatesAndDeletes()
	{
		// Arrange
		var sharedId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var newApi = CreateCustomApi("NewApi", id: Guid.Empty);

		var sharedApi = CreateCustomApi("SharedApi", id: sharedId);
		sharedApi.PluginType = new PluginType("SharedPluginType") { Id = pluginTypeId };

		var remoteSharedApi = CreateCustomApi("SharedApi", id: sharedId);
		remoteSharedApi.PluginType = new PluginType("SharedPluginType") { Id = pluginTypeId };

		var removedApi = CreateCustomApi("RemovedApi");

		var local = CreateAssemblyInfo([newApi, sharedApi]);
		var remote = CreateAssemblyInfo([remoteSharedApi, removedApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Single(differences.CustomApis.Creates);
		Assert.Equal("NewApi", differences.CustomApis.Creates[0].Local.Name);
		Assert.Single(differences.CustomApis.Deletes);
		Assert.Equal("RemovedApi", differences.CustomApis.Deletes[0].Name);
		Assert.Empty(differences.CustomApis.Updates);
	}

	// ========== RequestParameter Tests ==========

	[Fact]
	public void NewRequestParameterDetectedAsCreate()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var newParam = CreateRequestParameter("NewParam", id: Guid.Empty);

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.RequestParameters = [newParam];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.RequestParameters = [];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Single(differences.RequestParameters.Creates);
		Assert.Equal("NewParam", differences.RequestParameters.Creates[0].Local.Entity.Name);
		Assert.Empty(differences.RequestParameters.Updates);
		Assert.Empty(differences.RequestParameters.Deletes);
	}

	[Fact]
	public void RemovedRequestParameterDetectedAsDelete()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var remoteParam = CreateRequestParameter("OldParam");

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.RequestParameters = [];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.RequestParameters = [remoteParam];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Single(differences.RequestParameters.Deletes);
		Assert.Equal("OldParam", differences.RequestParameters.Deletes[0].Entity.Name);
		Assert.Empty(differences.RequestParameters.Creates);
		Assert.Empty(differences.RequestParameters.Updates);
	}

	[Fact]
	public void RequestParameterUpdatablePropertiesDetected()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var paramId = Guid.NewGuid();

		var localParam = CreateRequestParameter("LocalParamName", id: paramId);
		localParam.UniqueName = "new_shared_param"; // Keep UniqueName the same to avoid recreate
		localParam.DisplayName = "LocalDisplayName";

		var remoteParam = CreateRequestParameter("RemoteParamName", id: paramId);
		remoteParam.UniqueName = "new_shared_param"; // Keep UniqueName the same to avoid recreate
		remoteParam.DisplayName = "RemoteDisplayName";

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.RequestParameters = [localParam];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.RequestParameters = [remoteParam];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Empty(differences.RequestParameters.Creates);
		Assert.Empty(differences.RequestParameters.Deletes);
		Assert.Single(differences.RequestParameters.Updates);

		var update = differences.RequestParameters.Updates[0];
		var propNames = update.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
		Assert.Equal([
			nameof(RequestParameter.DisplayName),
			nameof(RequestParameter.Name)
		], propNames);
	}

	[Fact]
	public void RequestParameterRecreatePropertiesDetected()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var paramId = Guid.NewGuid();

		var localParam = CreateRequestParameter("Param", id: paramId);
		localParam.UniqueName = "new_local_param";
		localParam.Type = CustomApiParameterType.Integer;
		localParam.IsOptional = true;
		localParam.IsCustomizable = false;
		localParam.LogicalEntityName = "account";

		var remoteParam = CreateRequestParameter("Param", id: paramId);
		remoteParam.UniqueName = "new_remote_param";
		remoteParam.Type = CustomApiParameterType.String;
		remoteParam.IsOptional = false;
		remoteParam.IsCustomizable = true;
		remoteParam.LogicalEntityName = "contact";

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.RequestParameters = [localParam];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.RequestParameters = [remoteParam];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert — recreate properties should result in both Creates and Deletes
		Assert.Single(differences.RequestParameters.Creates);
		Assert.Single(differences.RequestParameters.Deletes);
		Assert.Empty(differences.RequestParameters.Updates);

		var create = differences.RequestParameters.Creates[0];
		var propNames = create.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
		Assert.Equal([
			nameof(RequestParameter.IsCustomizable),
			nameof(RequestParameter.IsOptional),
			nameof(RequestParameter.LogicalEntityName),
			nameof(RequestParameter.Type),
			nameof(RequestParameter.UniqueName)
		], propNames);
	}

	[Fact]
	public void DeletedParentApiCascadesRequestParameterDeletes()
	{
		// Arrange
		var param1 = CreateRequestParameter("Param1");
		var param2 = CreateRequestParameter("Param2");

		var remoteApi = CreateCustomApi("RemovedApi");
		remoteApi.RequestParameters = [param1, param2];

		var local = CreateAssemblyInfo([]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Equal(2, differences.RequestParameters.Deletes.Count);
		var deleteNames = differences.RequestParameters.Deletes.Select(d => d.Entity.Name).Order().ToArray();
		Assert.Equal(["Param1", "Param2"], deleteNames);
		Assert.Empty(differences.RequestParameters.Creates);
		Assert.Empty(differences.RequestParameters.Updates);
	}

	// ========== ResponseProperty Tests ==========

	[Fact]
	public void NewResponsePropertyDetectedAsCreate()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var newProp = CreateResponseProperty("NewProp", id: Guid.Empty);

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.ResponseProperties = [newProp];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.ResponseProperties = [];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Single(differences.ResponseProperties.Creates);
		Assert.Equal("NewProp", differences.ResponseProperties.Creates[0].Local.Entity.Name);
		Assert.Empty(differences.ResponseProperties.Updates);
		Assert.Empty(differences.ResponseProperties.Deletes);
	}

	[Fact]
	public void RemovedResponsePropertyDetectedAsDelete()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();

		var remoteProp = CreateResponseProperty("OldProp");

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.ResponseProperties = [];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.ResponseProperties = [remoteProp];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Single(differences.ResponseProperties.Deletes);
		Assert.Equal("OldProp", differences.ResponseProperties.Deletes[0].Entity.Name);
		Assert.Empty(differences.ResponseProperties.Creates);
		Assert.Empty(differences.ResponseProperties.Updates);
	}

	[Fact]
	public void ResponsePropertyUpdatablePropertiesDetected()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var propId = Guid.NewGuid();

		var localProp = CreateResponseProperty("LocalPropName", id: propId);
		localProp.UniqueName = "new_shared_prop"; // Keep UniqueName the same to avoid recreate
		localProp.DisplayName = "LocalDisplayName";

		var remoteProp = CreateResponseProperty("RemotePropName", id: propId);
		remoteProp.UniqueName = "new_shared_prop"; // Keep UniqueName the same to avoid recreate
		remoteProp.DisplayName = "RemoteDisplayName";

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.ResponseProperties = [localProp];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.ResponseProperties = [remoteProp];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert
		Assert.Empty(differences.ResponseProperties.Creates);
		Assert.Empty(differences.ResponseProperties.Deletes);
		Assert.Single(differences.ResponseProperties.Updates);

		var update = differences.ResponseProperties.Updates[0];
		var propNames = update.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
		Assert.Equal([
			nameof(ResponseProperty.DisplayName),
			nameof(ResponseProperty.Name)
		], propNames);
	}

	[Fact]
	public void ResponsePropertyRecreatePropertiesDetected()
	{
		// Arrange
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var propId = Guid.NewGuid();

		var localProp = CreateResponseProperty("Prop", id: propId);
		localProp.UniqueName = "new_local_prop";
		localProp.Type = CustomApiParameterType.Integer;
		localProp.IsCustomizable = false;
		localProp.LogicalEntityName = "account";

		var remoteProp = CreateResponseProperty("Prop", id: propId);
		remoteProp.UniqueName = "new_remote_prop";
		remoteProp.Type = CustomApiParameterType.String;
		remoteProp.IsCustomizable = true;
		remoteProp.LogicalEntityName = "contact";

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.ResponseProperties = [localProp];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.ResponseProperties = [remoteProp];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert — recreate properties should result in both Creates and Deletes
		Assert.Single(differences.ResponseProperties.Creates);
		Assert.Single(differences.ResponseProperties.Deletes);
		Assert.Empty(differences.ResponseProperties.Updates);

		var create = differences.ResponseProperties.Creates[0];
		var propNames = create.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
		Assert.Equal([
			nameof(ResponseProperty.IsCustomizable),
			nameof(ResponseProperty.LogicalEntityName),
			nameof(ResponseProperty.Type),
			nameof(ResponseProperty.UniqueName)
		], propNames);
	}

	// ========== Recreation: Children preserved ==========

	[Fact]
	public void RecreatedCustomApiIncludesUnchangedRequestParametersInCreates()
	{
		// Arrange — CustomAPI is recreated (IsFunction changed), but its RequestParameter is identical
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var paramId = Guid.NewGuid();

		var param = CreateRequestParameter("Param1", id: paramId);
		var remoteParam = CreateRequestParameter("Param1", id: paramId);

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.IsFunction = true; // Changed — triggers recreation
		localApi.RequestParameters = [param];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.IsFunction = false; // Different — triggers recreation
		remoteApi.RequestParameters = [remoteParam];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert — CustomAPI should be recreated
		Assert.Single(differences.CustomApis.Creates);
		Assert.Single(differences.CustomApis.Deletes);

		// Assert — the unchanged RequestParameter must also be in Creates (re-created with new parent)
		Assert.Single(differences.RequestParameters.Creates);
		Assert.Equal("Param1", differences.RequestParameters.Creates[0].Local.Entity.Name);

		// And the old one must be in Deletes (explicitly deleted before parent)
		Assert.Single(differences.RequestParameters.Deletes);
		Assert.Equal("Param1", differences.RequestParameters.Deletes[0].Entity.Name);
	}

	[Fact]
	public void RecreatedCustomApiIncludesUnchangedResponsePropertiesInCreates()
	{
		// Arrange — CustomAPI is recreated (IsFunction changed), but its ResponseProperty is identical
		var apiId = Guid.NewGuid();
		var pluginTypeId = Guid.NewGuid();
		var propId = Guid.NewGuid();

		var prop = CreateResponseProperty("Prop1", id: propId);
		var remoteProp = CreateResponseProperty("Prop1", id: propId);

		var localApi = CreateCustomApi("TestApi", id: apiId);
		localApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		localApi.IsFunction = true;
		localApi.ResponseProperties = [prop];

		var remoteApi = CreateCustomApi("TestApi", id: apiId);
		remoteApi.PluginType = new PluginType("PluginType") { Id = pluginTypeId };
		remoteApi.IsFunction = false;
		remoteApi.ResponseProperties = [remoteProp];

		var local = CreateAssemblyInfo([localApi]);
		var remote = CreateAssemblyInfo([remoteApi]);

		// Act
		var differences = differenceCalculator.CalculateDifferences(local, remote);

		// Assert — CustomAPI should be recreated
		Assert.Single(differences.CustomApis.Creates);
		Assert.Single(differences.CustomApis.Deletes);

		// Assert — the unchanged ResponseProperty must also be in Creates
		Assert.Single(differences.ResponseProperties.Creates);
		Assert.Equal("Prop1", differences.ResponseProperties.Creates[0].Local.Entity.Name);

		// And the old one must be in Deletes
		Assert.Single(differences.ResponseProperties.Deletes);
		Assert.Equal("Prop1", differences.ResponseProperties.Deletes[0].Entity.Name);
	}
}
