using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.SyncService;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.Model;
using XrmSync.SyncService.Difference;
using XrmPluginCore.Enums;
using XrmSync.Analyzer.Reader;
using XrmSync.SyncService.Validation;

namespace Tests.Plugins;

public class PluginServiceTests
{
	private readonly ILogger<PluginSyncService> logger = Substitute.For<ILogger<PluginSyncService>>();
	private readonly IPluginAssemblyReader pluginAssemblyReader = Substitute.For<IPluginAssemblyReader>();
	private readonly IPluginAssemblyWriter pluginAssemblyWriter = Substitute.For<IPluginAssemblyWriter>();
	private readonly IPluginReader pluginReader = Substitute.For<IPluginReader>();
	private readonly IPluginWriter pluginWriter = Substitute.For<IPluginWriter>();
	private readonly IValidator<PluginDefinition> pluginValidator = Substitute.For<IValidator<PluginDefinition>>();
	private readonly IValidator<CustomApiDefinition> customApiValidator = Substitute.For<IValidator<CustomApiDefinition>>();
	private readonly ICustomApiReader customApiReader = Substitute.For<ICustomApiReader>();
	private readonly ICustomApiWriter customApiWriter = Substitute.For<ICustomApiWriter>();
	private readonly ILocalReader assemblyReader = Substitute.For<ILocalReader>();
	private readonly ISolutionReader solutionReader = Substitute.For<ISolutionReader>();
	private readonly IDifferenceCalculator differenceUtility = Substitute.For<IDifferenceCalculator>();
	private readonly IDescription description = new Description();
	private readonly IPrintService printService = Substitute.For<IPrintService>();
	private readonly PluginSyncCommandOptions options = new(string.Empty, "solution");

	private readonly PluginSyncService plugin;

	public PluginServiceTests()
	{
		plugin = new PluginSyncService(
			pluginAssemblyReader,
			pluginAssemblyWriter,
			pluginReader,
			pluginWriter,
			pluginValidator,
			customApiValidator,
			customApiReader,
			customApiWriter,
			assemblyReader,
			solutionReader,
			differenceUtility,
			description,
			printService,
			Options.Create(options), logger);
	}

	[Fact]
	public void CreatePluginAssemblyCallsWriterAndReturnsAssemblyWithId()
	{
		// Arrange
		var assembly = new AssemblyInfo("TestAssembly")
		{
			DllPath = "path",
			Hash = "hash",
			Version = "1.0.0.0",
			Plugins = []
		};
		var expectedId = Guid.NewGuid();
		pluginAssemblyWriter.CreatePluginAssembly(assembly.Name, assembly.DllPath, assembly.Hash, assembly.Version, description.SyncDescription)
			.Returns(expectedId);

		// Act
		var result = plugin.CreatePluginAssembly(assembly);

		// Assert
		Assert.Equal(expectedId, result.Id);
		pluginAssemblyWriter.Received(1).CreatePluginAssembly(assembly.Name, assembly.DllPath, assembly.Hash, assembly.Version, description.SyncDescription);
	}

	[Fact]
	public void UpdatePluginAssemblyCallsWriter()
	{
		// Arrange
		var assemblyId = Guid.NewGuid();
		var assembly = new AssemblyInfo("TestAssembly")
		{
			DllPath = "path",
			Hash = "hash",
			Version = "1.0.0.0",
			Plugins = []
		};

		// Act
		plugin.UpdatePluginAssembly(assemblyId, assembly);

		// Assert
		pluginAssemblyWriter.Received(1).UpdatePluginAssembly(assemblyId, assembly.Name, assembly.DllPath, assembly.Hash, assembly.Version, description.SyncDescription);
	}

	[Fact]
	public void CreatePluginsCallsWriter()
	{
		// Arrange
		var crmAssembly = new AssemblyInfo("TestAssembly")
		{
			Id = Guid.NewGuid(),
			DllPath = "path",
			Hash = "hash",
			Version = "1.0.0.0",
			Plugins = []
		};

		var pluginType =
			new PluginDefinition("Type1")
			{
				Id = Guid.NewGuid(),
				PluginSteps = [
					new("Step1") {
						ExecutionStage = ExecutionStage.PreValidation,
						EventOperation = "Update",
						LogicalName = "account",
						Deployment = 0,
						ExecutionMode = 0,
						ExecutionOrder = 1,
						FilteredAttributes = string.Empty,
						UserContext = Guid.NewGuid(),
						AsyncAutoDelete = false,
						PluginImages = [
							new("Image1") {
								EntityAlias = "alias",
								ImageType = 0,
								Attributes = string.Empty
							}
						]
					}
				]
			};

		List<PluginDefinition> pluginTypes = [pluginType];
		var pluginSteps = pluginType.PluginSteps.ConvertAll(s => new ParentReference<Step, PluginDefinition>(s, pluginType));
		var pluginImages = pluginSteps.SelectMany(s => s.Entity.PluginImages.Select(i => new ParentReference<Image, Step>(i, s.Entity))).ToList();

		var customApi =
			new CustomApiDefinition("CustomApi1")
			{
				UniqueName = "customapi_testapi",
				Description = "Test API",
				DisplayName = "Test API",
				BoundEntityLogicalName = "account",
				ExecutePrivilegeName = "prvTestExecute",
				PluginType = new PluginType("CustomApiType") { Id = Guid.NewGuid() },
				RequestParameters = [
					new("TestParameter") {
						UniqueName = "test_parameter",
						Type = 0,
						DisplayName = "Test Parameter",
						LogicalEntityName = "account",
						IsCustomizable = false,
						IsOptional = false
					}
				],
				ResponseProperties = [
					new("TestResponse") {
						UniqueName = "test_response",
						Type = 0,
						DisplayName = "Test Response",
						LogicalEntityName = "account",
						IsCustomizable = false
					}
				]
			};

		List<CustomApiDefinition> customApis = [customApi];
		var requestParams = customApi.RequestParameters.ConvertAll(r => new ParentReference<RequestParameter, CustomApiDefinition>(r, customApi));
		var responseProps = customApi.ResponseProperties.ConvertAll(r => new ParentReference<ResponseProperty, CustomApiDefinition>(r, customApi));

		var createdTypes = new List<PluginDefinition> {
			new("CreatedType") {
				Id = Guid.NewGuid(),
				PluginSteps = [
					new("CreatedStep") {
						ExecutionStage = ExecutionStage.PreValidation,
						EventOperation = "Update",
						LogicalName = "account",
						Deployment = 0,
						ExecutionMode = 0,
						ExecutionOrder = 1,
						FilteredAttributes = string.Empty,
						UserContext = Guid.NewGuid(),
						AsyncAutoDelete = false,
						PluginImages = []
					}
				]
			}
		};
		var createdSteps = createdTypes.SelectMany(t => t.PluginSteps.Select(s => new ParentReference<Step, PluginDefinition>(s, t))).ToList();

		var createdCustomApis = new List<CustomApiDefinition> {
			new("CreatedCustomApi") {
				UniqueName = "customapi_created",
				Id = Guid.NewGuid(),
				Description = "Created API",
				DisplayName = "Created API",
				BoundEntityLogicalName = "account",
				ExecutePrivilegeName = "prvCreatedExecute",
				PluginType = new ("CreatedApiType") { Id = Guid.NewGuid() },
			}
		};

		pluginWriter.CreatePluginTypes(pluginTypes.ArgMatches(), Arg.Any<Guid>(), Arg.Any<string>()).Returns(createdTypes);
		pluginWriter.CreatePluginSteps(pluginSteps.ArgMatches(), Arg.Any<string>()).Returns(createdSteps);
		customApiWriter.CreateCustomApis(customApis.ArgMatches(), Arg.Any<string>()).Returns(createdCustomApis);

		var differences = new Differences(
			Difference<PluginDefinition>.Empty with
			{
				Creates = [EntityDifference<PluginDefinition>.FromLocal(pluginType)]
			},
			Difference<Step, PluginDefinition>.Empty with
			{
				Creates = [.. pluginSteps.Select(EntityDifference<Step, PluginDefinition>.FromLocal)]
			},
			Difference<Image, Step>.Empty with
			{
				Creates = [.. pluginImages.Select(EntityDifference<Image, Step>.FromLocal)]
			},
			Difference<CustomApiDefinition>.Empty with
			{
				Creates = [EntityDifference<CustomApiDefinition>.FromLocal(customApi)]
			},
			Difference<RequestParameter, CustomApiDefinition>.Empty with
			{
				Creates = [.. requestParams.Select(EntityDifference<RequestParameter, CustomApiDefinition>.FromLocal)]
			},
			Difference<ResponseProperty, CustomApiDefinition>.Empty with
			{
				Creates = [.. responseProps.Select(EntityDifference<ResponseProperty, CustomApiDefinition>.FromLocal)]
			}
		);

		// Act
		plugin.DoCreates(differences, crmAssembly);

		// Assert
		pluginWriter.Received(1).CreatePluginTypes(pluginTypes.ArgMatches(), crmAssembly.Id, description.SyncDescription);
		pluginWriter.Received(1).CreatePluginSteps(pluginSteps.ArgMatches(), description.SyncDescription);
		pluginWriter.Received(1).CreatePluginImages(pluginImages.ArgMatches());
		customApiWriter.Received(1).CreateCustomApis(customApis.ArgMatches(), description.SyncDescription);
		customApiWriter.Received(1).CreateRequestParameters(requestParams.ArgMatches());
		customApiWriter.Received(1).CreateResponseProperties(responseProps.ArgMatches());
	}

	[Fact]
	public void DoCreatesResolvesCustomApiPluginTypeIdFromNewlyCreatedTypes()
	{
		// Arrange
		var crmAssembly = new AssemblyInfo("TestAssembly")
		{
			Id = Guid.NewGuid(),
			DllPath = "path",
			Hash = "hash",
			Version = "1.0.0.0",
			Plugins = []
		};

		// A new plugin type (not yet in Dataverse)
		var sharedTypeName = "Namespace.MyCustomApiPlugin";
		var pluginType = new PluginDefinition(sharedTypeName) { PluginSteps = [] };

		// A new custom API referencing the same plugin type by name, but with Id = Guid.Empty
		var customApi = new CustomApiDefinition("TestCustomApi")
		{
			UniqueName = "new_test_custom_api",
			Description = "Test",
			DisplayName = "Test",
			BoundEntityLogicalName = string.Empty,
			ExecutePrivilegeName = string.Empty,
			PluginType = new PluginType(sharedTypeName), // Id defaults to Guid.Empty
			RequestParameters = [],
			ResponseProperties = []
		};

		var expectedPluginTypeId = Guid.NewGuid();

		// Mock CreatePluginTypes to simulate Dataverse assigning an ID
		pluginWriter.CreatePluginTypes(Arg.Any<ICollection<PluginDefinition>>(), Arg.Any<Guid>(), Arg.Any<string>())
			.Returns(callInfo =>
			{
				var types = callInfo.ArgAt<ICollection<PluginDefinition>>(0);
				foreach (var t in types)
					t.Id = expectedPluginTypeId;
				return types;
			});

		var differences = new Differences(
			Difference<PluginDefinition>.Empty with
			{
				Creates = [EntityDifference<PluginDefinition>.FromLocal(pluginType)]
			},
			Difference<Step, PluginDefinition>.Empty,
			Difference<Image, Step>.Empty,
			Difference<CustomApiDefinition>.Empty with
			{
				Creates = [EntityDifference<CustomApiDefinition>.FromLocal(customApi)]
			},
			Difference<RequestParameter, CustomApiDefinition>.Empty,
			Difference<ResponseProperty, CustomApiDefinition>.Empty
		);

		// Act
		plugin.DoCreates(differences, crmAssembly);

		// Assert — the custom API's PluginType.Id should have been resolved to the newly created type's ID
		Assert.Equal(expectedPluginTypeId, customApi.PluginType.Id);
	}

	[Fact]
	public void AlignCustomApiIdsTransfersPluginTypeId()
	{
		// Arrange
		var remotePluginTypeId = Guid.NewGuid();
		var remoteCustomApiId = Guid.NewGuid();

		var localCustomApi = new CustomApiDefinition("TestApi")
		{
			UniqueName = "new_test_api",
			Description = "Test",
			DisplayName = "Test",
			BoundEntityLogicalName = string.Empty,
			ExecutePrivilegeName = string.Empty,
			PluginType = new PluginType("Namespace.MyPlugin"), // Id = Guid.Empty
			RequestParameters = [],
			ResponseProperties = []
		};

		var remoteCustomApi = new CustomApiDefinition("TestApi")
		{
			Id = remoteCustomApiId,
			UniqueName = "new_test_api",
			Description = "Test",
			DisplayName = "Test",
			BoundEntityLogicalName = string.Empty,
			ExecutePrivilegeName = string.Empty,
			PluginType = new PluginType("Namespace.MyPlugin") { Id = remotePluginTypeId },
			RequestParameters = [],
			ResponseProperties = []
		};

		var localAssembly = new AssemblyInfo("TestAssembly")
		{
			DllPath = "test.dll",
			Hash = "hash",
			Version = "1.0.0",
			Plugins = [],
			CustomApis = [localCustomApi]
		};

		var crmAssembly = new AssemblyInfo("TestAssembly")
		{
			Id = Guid.NewGuid(),
			DllPath = "test.dll",
			Hash = "hash",
			Version = "1.0.0",
			Plugins = [],
			CustomApis = [remoteCustomApi]
		};

		// Act — AlignCustomApiIds is private static, so we invoke it via reflection
		var method = typeof(PluginSyncService).GetMethod("AlignCustomApiIds",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		Assert.NotNull(method);
		method.Invoke(null, [localAssembly, crmAssembly]);

		// Assert
		Assert.Equal(remoteCustomApiId, localCustomApi.Id);
		Assert.Equal(remotePluginTypeId, localCustomApi.PluginType.Id);
	}

	[Fact]
	public void DeletePluginsCallsWriter()
	{
		// Arrange
		Image image = new("Image1")
		{
			EntityAlias = "alias",
			ImageType = ImageType.PreImage,
			Attributes = string.Empty
		};

		Step step = new("Step1")
		{
			ExecutionStage = ExecutionStage.PreOperation,
			ExecutionMode = ExecutionMode.Asynchronous,
			EventOperation = "Update",
			LogicalName = "account",
			Deployment = 0,
			ExecutionOrder = 1,
			FilteredAttributes = string.Empty,
			UserContext = Guid.NewGuid(),
			AsyncAutoDelete = false,
			PluginImages = [image]
		};

		PluginDefinition type = new("Type1")
		{
			Id = Guid.NewGuid(),
			PluginSteps = [step]
		};

		List<PluginDefinition> types = [type];
		List<ParentReference<Step, PluginDefinition>> steps = [new(step, type)];
		List<ParentReference<Image, Step>> images = [new(image, step)];
		List<CustomApiDefinition> apis = [];
		List<ParentReference<RequestParameter, CustomApiDefinition>> reqs = [];
		List<ParentReference<ResponseProperty, CustomApiDefinition>> resps = [];

		// Act
		plugin.DoDeletes(new Differences(
			Difference<PluginDefinition>.Empty with { Deletes = types },
			Difference<Step, PluginDefinition>.Empty with { Deletes = steps },
			Difference<Image, Step>.Empty with { Deletes = images },
			Difference<CustomApiDefinition>.Empty with { Deletes = apis },
			Difference<RequestParameter, CustomApiDefinition>.Empty with { Deletes = reqs },
			Difference<ResponseProperty, CustomApiDefinition>.Empty with { Deletes = resps }
		));

		// Assert
		pluginWriter.Received(1).DeletePluginImages(images.ConvertAll(i => i.Entity).ArgMatches());
		pluginWriter.Received(1).DeletePluginSteps(steps.ConvertAll(s => s.Entity).ArgMatches());
		pluginWriter.Received(1).DeletePluginTypes(types);
		customApiWriter.Received(1).DeleteCustomApiRequestParameters(reqs.ConvertAll(r => r.Entity).ArgMatches());
		customApiWriter.Received(1).DeleteCustomApiResponseProperties(resps.ConvertAll(r => r.Entity).ArgMatches());
		customApiWriter.Received(1).DeleteCustomApiDefinitions(apis);
	}

	#region IncludeCustomApiPluginTypes

	[Fact]
	public void IncludeCustomApiPluginTypesAddsBackingTypeToPlugins()
	{
		// Arrange
		var localAssembly = new AssemblyInfo("TestAssembly")
		{
			DllPath = "test.dll",
			Hash = "hash",
			Version = "1.0.0",
			Plugins = [new PluginDefinition("Namespace.ExistingPlugin") { PluginSteps = [] }],
			CustomApis = [
				new CustomApiDefinition("TestApi")
				{
					UniqueName = "new_test_api",
					Description = "Test",
					DisplayName = "Test",
					BoundEntityLogicalName = string.Empty,
					ExecutePrivilegeName = string.Empty,
					PluginType = new PluginType("Namespace.CustomApiBackingType"),
					RequestParameters = [],
					ResponseProperties = []
				}
			]
		};

		// Act
		PluginSyncService.IncludeCustomApiPluginTypes(localAssembly);

		// Assert — the backing type should be added alongside the existing plugin
		Assert.Equal(2, localAssembly.Plugins.Count);
		Assert.Contains(localAssembly.Plugins, p => p.Name == "Namespace.ExistingPlugin");
		Assert.Contains(localAssembly.Plugins, p => p.Name == "Namespace.CustomApiBackingType");

		var injected = localAssembly.Plugins.Single(p => p.Name == "Namespace.CustomApiBackingType");
		Assert.Empty(injected.PluginSteps);
	}

	[Fact]
	public void IncludeCustomApiPluginTypesDoesNotDuplicateExistingType()
	{
		// Arrange — the custom API backing type already exists in Plugins (e.g. it also has steps)
		var localAssembly = new AssemblyInfo("TestAssembly")
		{
			DllPath = "test.dll",
			Hash = "hash",
			Version = "1.0.0",
			Plugins = [new PluginDefinition("Namespace.SharedType") { PluginSteps = [
				new("Step1") {
					ExecutionStage = ExecutionStage.PreValidation,
					EventOperation = "Create",
					LogicalName = "account",
					Deployment = 0,
					ExecutionMode = 0,
					ExecutionOrder = 1,
					FilteredAttributes = string.Empty,
					UserContext = Guid.Empty,
					AsyncAutoDelete = false,
					PluginImages = []
				}
			] }],
			CustomApis = [
				new CustomApiDefinition("TestApi")
				{
					UniqueName = "new_test_api",
					Description = "Test",
					DisplayName = "Test",
					BoundEntityLogicalName = string.Empty,
					ExecutePrivilegeName = string.Empty,
					PluginType = new PluginType("Namespace.SharedType"),
					RequestParameters = [],
					ResponseProperties = []
				}
			]
		};

		// Act
		PluginSyncService.IncludeCustomApiPluginTypes(localAssembly);

		// Assert — no duplicate; the existing entry with its step is preserved
		Assert.Single(localAssembly.Plugins);
		Assert.Single(localAssembly.Plugins[0].PluginSteps);
	}

	[Fact]
	public void IncludeCustomApiPluginTypesHandlesMultipleApisWithSameBackingType()
	{
		// Arrange — two custom APIs reference the same backing type
		var localAssembly = new AssemblyInfo("TestAssembly")
		{
			DllPath = "test.dll",
			Hash = "hash",
			Version = "1.0.0",
			Plugins = [],
			CustomApis = [
				new CustomApiDefinition("Api1")
				{
					UniqueName = "new_api1",
					Description = "Test",
					DisplayName = "Test",
					BoundEntityLogicalName = string.Empty,
					ExecutePrivilegeName = string.Empty,
					PluginType = new PluginType("Namespace.SharedBackingType"),
					RequestParameters = [],
					ResponseProperties = []
				},
				new CustomApiDefinition("Api2")
				{
					UniqueName = "new_api2",
					Description = "Test",
					DisplayName = "Test",
					BoundEntityLogicalName = string.Empty,
					ExecutePrivilegeName = string.Empty,
					PluginType = new PluginType("Namespace.SharedBackingType"),
					RequestParameters = [],
					ResponseProperties = []
				}
			]
		};

		// Act
		PluginSyncService.IncludeCustomApiPluginTypes(localAssembly);

		// Assert — only one entry added despite two APIs referencing the same type
		Assert.Single(localAssembly.Plugins);
		Assert.Equal("Namespace.SharedBackingType", localAssembly.Plugins[0].Name);
	}

	[Fact]
	public void IncludeCustomApiPluginTypesNoOpsWithEmptyCustomApis()
	{
		// Arrange
		var localAssembly = new AssemblyInfo("TestAssembly")
		{
			DllPath = "test.dll",
			Hash = "hash",
			Version = "1.0.0",
			Plugins = [new PluginDefinition("Namespace.Plugin") { PluginSteps = [] }],
			CustomApis = []
		};

		// Act
		PluginSyncService.IncludeCustomApiPluginTypes(localAssembly);

		// Assert — nothing changed
		Assert.Single(localAssembly.Plugins);
	}

	#endregion

	[Fact]
	public void UpdatePluginsCallsWriter()
	{
		// Arrange
		var data = new Differences(
			Difference<PluginDefinition>.Empty,
			Difference<Step, PluginDefinition>.Empty,
			Difference<Image, Step>.Empty,
			Difference<CustomApiDefinition>.Empty,
			Difference<RequestParameter, CustomApiDefinition>.Empty,
			Difference<ResponseProperty, CustomApiDefinition>.Empty
		);

		// Act
		plugin.DoUpdates(data);

		// Assert
		pluginWriter.Received(1).UpdatePluginSteps(data.PluginSteps.Updates.ConvertAll(upd => upd.Local.Entity).ArgMatches(), description.SyncDescription);
		pluginWriter.Received(1).UpdatePluginImages(data.PluginImages.Updates.ConvertAll(upd => upd.Local).ArgMatches());
		customApiWriter.Received(1).UpdateCustomApis(data.CustomApis.Updates.ConvertAll(upd => upd.Local).ArgMatches(), description.SyncDescription);
		customApiWriter.Received(1).UpdateRequestParameters(data.RequestParameters.Updates.ConvertAll(upd => upd.Local.Entity).ArgMatches());
		customApiWriter.Received(1).UpdateResponseProperties(data.ResponseProperties.Updates.ConvertAll(upd => upd.Local.Entity).ArgMatches());
	}
}
