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
    private readonly XrmSyncOptions _options = new(string.Empty, "solution", LogLevel.Information, false);

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
        var solutionName = _options.SolutionName;
        const string solutionPrefix = "pre";
        var pluginTypes = new List<PluginType> {
            new() {
                Name = "Type1",
                Id = Guid.NewGuid()
            }
        };
        var pluginSteps = new List<Step> {
            new() {
                Name = "Step1",
                PluginTypeName = "Type1",
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
        var pluginImages = new List<Image> {
            new() {
                Name = "Image1",
                PluginStepName = "Step1",
                EntityAlias = "alias",
                ImageType = 0,
                Attributes = string.Empty
            }
        };
        var customApis = new List<CustomApiDefinition>()
        {
            new() {
                Name = "CustomApi1",
                UniqueName = "customapi_testapi",
                Description = "Test API",
                DisplayName = "Test API",
                BoundEntityLogicalName = "account",
                ExecutePrivilegeName = "prvTestExecute",
                PluginTypeName = "Type1"
            }
        };

        var requestParams = new List<RequestParameter> {
            new() {
                Name = "TestParameter",
                UniqueName = "test_parameter",
                CustomApiName = "customapi_testapi",
                Type = 0,
                DisplayName = "Test Parameter",
                LogicalEntityName = "account"
            }
        };

        var responseProps = new List<ResponseProperty> {
            new() {
                Name = "TestResponse",
                UniqueName = "test_response",
                CustomApiName = "customapi_testapi",
                Type = 0,
                DisplayName = "Test Response",
                LogicalEntityName = "account"
            }
        };

        var createdTypes = new List<PluginType> {
            new() {
                Name = "CreatedType",
                Id = Guid.NewGuid()
            }
        };

        var createdSteps = new List<Step> {
            new() {
                Name = "CreatedStep",
                PluginTypeName = "Type1",
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

        var createdCustomApis = new List<CustomApiDefinition> {
            new() {
                Name = "CreatedCustomApi",
                UniqueName = "customapi_created",
                Id = Guid.NewGuid(),
                Description = "Created API",
                DisplayName = "Created API",
                BoundEntityLogicalName = "account",
                ExecutePrivilegeName = "prvCreatedExecute",
                PluginTypeName = "Type1"
            }
        };

        _pluginWriter.CreatePluginTypes(pluginTypes, Arg.Any<Guid>(), Arg.Any<string>()).Returns(createdTypes);
        _pluginWriter.CreatePluginSteps(pluginSteps, Arg.Any<List<PluginType>>(), Arg.Any<string>()).Returns(createdSteps);
        _customApiWriter.CreateCustomApis(customApis, Arg.Any<List<PluginType>>(), Arg.Any<string>(), Arg.Any<string>()).Returns(createdCustomApis);

        var differences = new Differences(
            new Difference<PluginType>(pluginTypes, [], [], []),
            new Difference<Step>(pluginSteps, [], [], []),
            new Difference<Image>(pluginImages, [], [], []),
            new Difference<CustomApiDefinition>(customApis, [], [], []),
            new Difference<RequestParameter>(requestParams, [], [], []),
            new Difference<ResponseProperty>(responseProps, [], [], [])
        );

        // Act
        _plugin.DoCreates(differences, [], [], crmAssembly, solutionPrefix);

        // Assert
        _pluginWriter.Received(1).CreatePluginTypes(pluginTypes, crmAssembly.Id, _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginSteps(pluginSteps, Arg.Is<List<PluginType>>(t => !t.Except(createdTypes).Any()), _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginImages(pluginImages, Arg.Is<List<Step>>(s => !s.Except(createdSteps).Any()));
        _customApiWriter.Received(1).CreateCustomApis(customApis, Arg.Is<List<PluginType>>(t => !t.Except(createdTypes).Any()), solutionPrefix, _description.SyncDescription);
        Assert.Equal(createdCustomApis, crmAssembly.CustomApis);
        _customApiWriter.Received(1).CreateRequestParameters(requestParams, Arg.Is<List<CustomApiDefinition>>(c => !c.Except(createdCustomApis).Any()));
        _customApiWriter.Received(1).CreateResponseProperties(responseProps, Arg.Is<List<CustomApiDefinition>>(c => !c.Except(createdCustomApis).Any()));
    }

    [Fact]
    public void DeletePlugins_CallsWriter()
    {
        // Arrange
        var types = new List<PluginType>();
        var steps = new List<Step>();
        var images = new List<Image>();
        var apis = new List<CustomApiDefinition>();
        var reqs = new List<RequestParameter>();
        var resps = new List<ResponseProperty>();

        // Act
        _plugin.DoDeletes(new Differences(
            new Difference<PluginType>([], [], types, []),
            new Difference<Step>([], [], steps, []),
            new Difference<Image>([], [], images, []),
            new Difference<CustomApiDefinition>([], [], apis, []),
            new Difference<RequestParameter>([], [], reqs, []),
            new Difference<ResponseProperty>([], [], resps, [])
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
            new Difference<PluginType>([], [], [], []),
            new Difference<Step>([], [], [], []),
            new Difference<Image>([], [], [], []),
            new Difference<CustomApiDefinition>([], [], [], []),
            new Difference<RequestParameter>([], [], [], []),
            new Difference<ResponseProperty>([], [], [], [])
        );

        List<PluginType> pluginTypes = [];
        List<Step> pluginSteps = [];

        // Act
        _plugin.DoUpdates(data, pluginTypes, pluginSteps);

        // Assert
        _pluginWriter.Received(1).UpdatePluginSteps(data.PluginSteps.Updates, _description.SyncDescription);
        _pluginWriter.Received(1).UpdatePluginImages(data.PluginImages.Updates, pluginSteps);
        _customApiWriter.Received(1).UpdateCustomApis(data.CustomApis.Updates, pluginTypes, _description.SyncDescription);
        _customApiWriter.Received(1).UpdateRequestParameters(data.RequestParameters.Updates);
        _customApiWriter.Received(1).UpdateResponseProperties(data.ResponseProperties.Updates);
    }
}
