using Microsoft.Extensions.Logging;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.SyncService;
using XrmSync.SyncService.PluginValidator;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.Model;
using XrmSync.AssemblyAnalyzer.AssemblyReader;
using XrmSync.SyncService.Difference;

namespace Tests;

public class PluginServiceTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly IPluginReader _pluginReader = Substitute.For<IPluginReader>();
    private readonly IPluginWriter _pluginWriter = Substitute.For<IPluginWriter>();
    private readonly IPluginValidator _pluginValidator = Substitute.For<IPluginValidator>();
    private readonly ICustomApiReader _customApiReader = Substitute.For<ICustomApiReader>();
    private readonly ICustomApiWriter _customApiWriter = Substitute.For<ICustomApiWriter>();
    private readonly IAssemblyReader _assemblyReader = Substitute.For<IAssemblyReader>();
    private readonly ISolutionReader _solutionReader = Substitute.For<ISolutionReader>();
    private readonly IDifferenceUtility _differenceUtility = Substitute.For<IDifferenceUtility>();
    private readonly Description _description = new();
    private readonly XrmSyncConfiguration _options = new(new(new(string.Empty, "solution", LogLevel.Information, false), null));

    private readonly PluginSyncService _plugin;

    public PluginServiceTests()
    {
        _plugin = new PluginSyncService(_pluginReader, _pluginWriter, _pluginValidator, _customApiReader, _customApiWriter, _assemblyReader, _solutionReader, _differenceUtility, _description, _options, _logger);
    }

    [Fact]
    public void CreatePluginAssembly_CallsWriterAndReturnsAssemblyWithId()
    {
        // Arrange
        var assembly = new AssemblyInfo {
            Name = "TestAssembly",
            DllPath = "path",
            Hash = "hash",
            Version = "1.0.0.0",
            Plugins = []
        };
        var expectedId = Guid.NewGuid();
        _pluginWriter.CreatePluginAssembly(assembly.Name, assembly.DllPath, assembly.Hash, assembly.Version, _description.SyncDescription)
            .Returns(expectedId);

        // Act
        var result = _plugin.CreatePluginAssembly(assembly);

        // Assert
        Assert.Equal(expectedId, result.Id);
        _pluginWriter.Received(1).CreatePluginAssembly(assembly.Name, assembly.DllPath, assembly.Hash, assembly.Version, _description.SyncDescription);
    }

    [Fact]
    public void UpdatePluginAssembly_CallsWriter()
    {
        // Arrange
        var assemblyId = Guid.NewGuid();
        var assembly = new AssemblyInfo {
            Name = "TestAssembly",
            DllPath = "path",
            Hash = "hash",
            Version = "1.0.0.0",
            Plugins = []
        };

        // Act
        _plugin.UpdatePluginAssembly(assemblyId, assembly);

        // Assert
        _pluginWriter.Received(1).UpdatePluginAssembly(assemblyId, assembly.Name, assembly.DllPath, assembly.Hash, assembly.Version, _description.SyncDescription);
    }

    [Fact]
    public void CreatePlugins_CallsWriter()
    {
        // Arrange
        var crmAssembly = new AssemblyInfo {
            Id = Guid.NewGuid(),
            Name = "TestAssembly",
            DllPath = "path",
            Hash = "hash",
            Version = "1.0.0.0",
            Plugins = []
        };
        
        var pluginTypes = new List<PluginDefinition> {
            new() {
                Name = "Type1",
                Id = Guid.NewGuid(),
                PluginSteps = []
            }
        };
        var pluginSteps = new List<Step> {
            new() {
                Name = "Step1",
                PluginType = pluginTypes[0],
                ExecutionStage = DG.XrmPluginCore.Enums.ExecutionStage.PreValidation,
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
        };
        pluginTypes[0].PluginSteps = pluginSteps;

        var pluginImages = new List<Image> {
            new() {
                Name = "Image1",
                Step = pluginSteps[0],
                EntityAlias = "alias",
                ImageType = 0,
                Attributes = string.Empty
            }
        };
        pluginSteps[0].PluginImages = pluginImages;

        var customApis = new List<CustomApiDefinition>()
        {
            new() {
                Name = "CustomApi1",
                UniqueName = "customapi_testapi",
                Description = "Test API",
                DisplayName = "Test API",
                BoundEntityLogicalName = "account",
                ExecutePrivilegeName = "prvTestExecute",
                PluginType = new PluginType { Name = "CustomApiType", Id = Guid.NewGuid() }
            }
        };

        var requestParams = new List<RequestParameter> {
            new() {
                Name = "TestParameter",
                UniqueName = "test_parameter",
                CustomApi = customApis[0],
                Type = 0,
                DisplayName = "Test Parameter",
                LogicalEntityName = "account",
                IsCustomizable = false,
                IsOptional = false
            }
        };
        customApis[0].RequestParameters = requestParams;

        var responseProps = new List<ResponseProperty> {
            new() {
                Name = "TestResponse",
                UniqueName = "test_response",
                CustomApi = customApis[0],
                Type = 0,
                DisplayName = "Test Response",
                LogicalEntityName = "account",
                IsCustomizable = false
            }
        };
        customApis[0].ResponseProperties = responseProps;

        var createdTypes = new List<PluginDefinition> {
            new() {
                Name = "CreatedType",
                Id = Guid.NewGuid(),
                PluginSteps = []
            }
        };

        var createdSteps = new List<Step> {
            new() {
                Name = "CreatedStep",
                PluginType = createdTypes[0],
                ExecutionStage = DG.XrmPluginCore.Enums.ExecutionStage.PreValidation,
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
        };
        createdTypes[0].PluginSteps = createdSteps;

        var createdCustomApis = new List<CustomApiDefinition> {
            new() {
                Name = "CreatedCustomApi",
                UniqueName = "customapi_created",
                Id = Guid.NewGuid(),
                Description = "Created API",
                DisplayName = "Created API",
                BoundEntityLogicalName = "account",
                ExecutePrivilegeName = "prvCreatedExecute",
                PluginType = customApis[0].PluginType
            }
        };

        _pluginWriter.CreatePluginTypes(pluginTypes, Arg.Any<Guid>(), Arg.Any<string>()).Returns(createdTypes);
        _pluginWriter.CreatePluginSteps(pluginSteps, Arg.Any<string>()).Returns(createdSteps);
        _customApiWriter.CreateCustomApis(customApis, Arg.Any<string>()).Returns(createdCustomApis);

        var differences = new Differences(
            Difference<PluginDefinition>.Empty with { Creates = pluginTypes.ConvertAll(EntityDifference<PluginDefinition>.FromLocal) },
            Difference<Step>.Empty with { Creates = pluginSteps.ConvertAll(EntityDifference<Step>.FromLocal) },
            Difference<Image>.Empty with { Creates = pluginImages.ConvertAll(EntityDifference<Image>.FromLocal) },
            Difference<CustomApiDefinition>.Empty with { Creates = customApis.ConvertAll(EntityDifference<CustomApiDefinition>.FromLocal) },
            Difference<RequestParameter>.Empty with { Creates = requestParams.ConvertAll(EntityDifference<RequestParameter>.FromLocal) },
            Difference<ResponseProperty>.Empty with { Creates = responseProps.ConvertAll(EntityDifference<ResponseProperty>.FromLocal) }
        );

        // Act
        _plugin.DoCreates(differences, crmAssembly);

        // Assert
        _pluginWriter.Received(1).CreatePluginTypes(ArgMatches(pluginTypes), crmAssembly.Id, _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginSteps(ArgMatches(pluginSteps), _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginImages(ArgMatches(pluginImages));
        _customApiWriter.Received(1).CreateCustomApis(ArgMatches(customApis), _description.SyncDescription);
        _customApiWriter.Received(1).CreateRequestParameters(ArgMatches(requestParams));
        _customApiWriter.Received(1).CreateResponseProperties(ArgMatches(responseProps));
    }

    private static List<T> ArgMatches<T>(List<T> expected)
    {
        return Arg.Is<List<T>>(actual => !expected.Except(actual).Any() && !actual.Except(expected).Any());
    }

    [Fact]
    public void DeletePlugins_CallsWriter()
    {
        // Arrange
        var types = new List<PluginDefinition>();
        var steps = new List<Step>();
        var images = new List<Image>();
        var apis = new List<CustomApiDefinition>();
        var reqs = new List<RequestParameter>();
        var resps = new List<ResponseProperty>();

        // Act
        _plugin.DoDeletes(new Differences(
            Difference<PluginDefinition>.Empty with { Deletes = types },
            Difference<Step>.Empty with { Deletes = steps },
            Difference<Image>.Empty with { Deletes = images },
            Difference<CustomApiDefinition>.Empty with { Deletes = apis },
            Difference<RequestParameter>.Empty with { Deletes = reqs },
            Difference<ResponseProperty>.Empty with { Deletes = resps }
        ));

        // Assert
        _pluginWriter.Received(1).DeletePluginImages(images);
        _pluginWriter.Received(1).DeletePluginSteps(steps);
        _pluginWriter.Received(1).DeletePluginTypes(types);
        _customApiWriter.Received(1).DeleteCustomApiRequestParameters(reqs);
        _customApiWriter.Received(1).DeleteCustomApiResponseProperties(resps);
        _customApiWriter.Received(1).DeleteCustomApiDefinitions(apis);
    }

    [Fact]
    public void UpdatePlugins_CallsWriter()
    {
        // Arrange
        var data = new Differences(
            Difference<PluginDefinition>.Empty,
            Difference<Step>.Empty,
            Difference<Image>.Empty,
            Difference<CustomApiDefinition>.Empty,
            Difference<RequestParameter>.Empty,
            Difference<ResponseProperty>.Empty
        );

        // Act
        _plugin.DoUpdates(data);

        // Assert
        _pluginWriter.Received(1).UpdatePluginSteps(ArgMatches(data.PluginSteps.Updates.ConvertAll(upd => upd.LocalEntity)), _description.SyncDescription);
        _pluginWriter.Received(1).UpdatePluginImages(ArgMatches(data.PluginImages.Updates.ConvertAll(upd => upd.LocalEntity)));
        _customApiWriter.Received(1).UpdateCustomApis(ArgMatches(data.CustomApis.Updates.ConvertAll(upd => upd.LocalEntity)), _description.SyncDescription);
        _customApiWriter.Received(1).UpdateRequestParameters(ArgMatches(data.RequestParameters.Updates.ConvertAll(upd => upd.LocalEntity)));
        _customApiWriter.Received(1).UpdateResponseProperties(ArgMatches(data.ResponseProperties.Updates.ConvertAll(upd => upd.LocalEntity)));
    }
}
