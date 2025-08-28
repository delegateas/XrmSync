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
using Microsoft.Crm.Sdk.Messages;
using DG.XrmPluginCore.Enums;

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

        var pluginType =
            new PluginDefinition
            {
                Name = "Type1",
                Id = Guid.NewGuid(),
                PluginSteps = [
                    new() {
                        Name = "Step1",
                        ExecutionStage = DG.XrmPluginCore.Enums.ExecutionStage.PreValidation,
                        EventOperation = "Update",
                        LogicalName = "account",
                        Deployment = 0,
                        ExecutionMode = 0,
                        ExecutionOrder = 1,
                        FilteredAttributes = string.Empty,
                        UserContext = Guid.NewGuid(),
                        AsyncAutoDelete = false,
                        PluginImages = [
                            new() {
                                Name = "Image1",
                                EntityAlias = "alias",
                                ImageType = 0,
                                Attributes = string.Empty
                            }
                        ]
                    }
                ]
            };

        List<PluginDefinition> pluginTypes = [ pluginType ];
        var pluginSteps = pluginType.PluginSteps.ConvertAll(s => new ParentReference<Step, PluginDefinition>(s, pluginType));
        var pluginImages = pluginSteps.SelectMany(s => s.Entity.PluginImages.Select(i => new ParentReference<Image, Step>(i, s.Entity))).ToList();

        var customApi =
            new CustomApiDefinition
            {
                Name = "CustomApi1",
                UniqueName = "customapi_testapi",
                Description = "Test API",
                DisplayName = "Test API",
                BoundEntityLogicalName = "account",
                ExecutePrivilegeName = "prvTestExecute",
                PluginType = new PluginType { Name = "CustomApiType", Id = Guid.NewGuid() },
                RequestParameters = [
                    new() {
                        Name = "TestParameter",
                        UniqueName = "test_parameter",
                        Type = 0,
                        DisplayName = "Test Parameter",
                        LogicalEntityName = "account",
                        IsCustomizable = false,
                        IsOptional = false
                    }
                ],
                ResponseProperties = [
                    new() {
                        Name = "TestResponse",
                        UniqueName = "test_response",
                        Type = 0,
                        DisplayName = "Test Response",
                        LogicalEntityName = "account",
                        IsCustomizable = false
                    }
                ]
            };

        List<CustomApiDefinition> customApis = [ customApi ];
        var requestParams = customApi.RequestParameters.ConvertAll(r => new ParentReference<RequestParameter, CustomApiDefinition>(r, customApi));
        var responseProps = customApi.ResponseProperties.ConvertAll(r => new ParentReference<ResponseProperty, CustomApiDefinition>(r, customApi));

        var createdTypes = new List<PluginDefinition> {
            new() {
                Name = "CreatedType",
                Id = Guid.NewGuid(),
                PluginSteps = [
                    new() {
                        Name = "CreatedStep",
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
                ]
            }
        };
        var createdSteps = createdTypes.SelectMany(t => t.PluginSteps.Select(s => new ParentReference<Step, PluginDefinition>(s, t))).ToList();

        var createdCustomApis = new List<CustomApiDefinition> {
            new() {
                Name = "CreatedCustomApi",
                UniqueName = "customapi_created",
                Id = Guid.NewGuid(),
                Description = "Created API",
                DisplayName = "Created API",
                BoundEntityLogicalName = "account",
                ExecutePrivilegeName = "prvCreatedExecute",
                PluginType = new () { Name = "CreatedApiType", Id = Guid.NewGuid() },
            }
        };

        _pluginWriter.CreatePluginTypes(pluginTypes.ArgMatches(), Arg.Any<Guid>(), Arg.Any<string>()).Returns(createdTypes);
        _pluginWriter.CreatePluginSteps(pluginSteps.ArgMatches(), Arg.Any<string>()).Returns(createdSteps);
        _customApiWriter.CreateCustomApis(customApis.ArgMatches(), Arg.Any<string>()).Returns(createdCustomApis);

        var differences = new Differences(
            Difference<PluginDefinition>.Empty with {
                Creates = [ EntityDifference<PluginDefinition>.FromLocal(pluginType) ]
            },
            Difference<Step, PluginDefinition>.Empty with {
                Creates = [.. pluginSteps.Select(EntityDifference<Step, PluginDefinition>.FromLocal)]
            },
            Difference<Image, Step>.Empty with {
                Creates = [.. pluginImages.Select(EntityDifference<Image, Step>.FromLocal)]
            },
            Difference<CustomApiDefinition>.Empty with {
                Creates = [ EntityDifference<CustomApiDefinition>.FromLocal(customApi) ]
            },
            Difference<RequestParameter, CustomApiDefinition>.Empty with {
                Creates = [.. requestParams.Select(EntityDifference<RequestParameter, CustomApiDefinition>.FromLocal) ]
            },
            Difference<ResponseProperty, CustomApiDefinition>.Empty with {
                Creates = [.. responseProps.Select(EntityDifference<ResponseProperty, CustomApiDefinition>.FromLocal)]
            }
        );

        // Act
        _plugin.DoCreates(differences, crmAssembly);

        // Assert
        _pluginWriter.Received(1).CreatePluginTypes(pluginTypes.ArgMatches(), crmAssembly.Id, _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginSteps(pluginSteps.ArgMatches(), _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginImages(pluginImages.ArgMatches());
        _customApiWriter.Received(1).CreateCustomApis(customApis.ArgMatches(), _description.SyncDescription);
        _customApiWriter.Received(1).CreateRequestParameters(requestParams.ArgMatches());
        _customApiWriter.Received(1).CreateResponseProperties(responseProps.ArgMatches());
    }

    [Fact]
    public void DeletePlugins_CallsWriter()
    {
        // Arrange
        Image image = new ()
        {
            Name = "Image1",
            EntityAlias = "alias",
            ImageType = ImageType.PreImage,
            Attributes = string.Empty
        };

        Step step = new ()
        {
            Name = "Step1",
            ExecutionStage = ExecutionStage.PreOperation,
            ExecutionMode = ExecutionMode.Asynchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = [ image ]
        };

        PluginDefinition type = new()
        {
            Name = "Type1",
            Id = Guid.NewGuid(),
            PluginSteps = [step]
        };

        List<PluginDefinition> types = [ type ];
        List<ParentReference<Step, PluginDefinition>> steps = [ new(step, type) ];
        List<ParentReference<Image, Step>> images = [ new(image, step) ];
        List<CustomApiDefinition> apis = [];
        List<ParentReference<RequestParameter, CustomApiDefinition>> reqs = [];
        List<ParentReference<ResponseProperty, CustomApiDefinition>> resps = [];

        // Act
        _plugin.DoDeletes(new Differences(
            Difference<PluginDefinition>.Empty with { Deletes = types },
            Difference<Step, PluginDefinition>.Empty with { Deletes = steps },
            Difference<Image, Step>.Empty with { Deletes = images },
            Difference<CustomApiDefinition>.Empty with { Deletes = apis },
            Difference<RequestParameter, CustomApiDefinition>.Empty with { Deletes = reqs },
            Difference<ResponseProperty, CustomApiDefinition>.Empty with { Deletes = resps }
        ));

        // Assert
        _pluginWriter.Received(1).DeletePluginImages(images.ConvertAll(i => i.Entity).ArgMatches());
        _pluginWriter.Received(1).DeletePluginSteps(steps.ConvertAll(s => s.Entity).ArgMatches());
        _pluginWriter.Received(1).DeletePluginTypes(types);
        _customApiWriter.Received(1).DeleteCustomApiRequestParameters(reqs.ConvertAll(r => r.Entity).ArgMatches());
        _customApiWriter.Received(1).DeleteCustomApiResponseProperties(resps.ConvertAll(r => r.Entity).ArgMatches());
        _customApiWriter.Received(1).DeleteCustomApiDefinitions(apis);
    }

    [Fact]
    public void UpdatePlugins_CallsWriter()
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
        _plugin.DoUpdates(data);

        // Assert
        _pluginWriter.Received(1).UpdatePluginSteps(data.PluginSteps.Updates.ConvertAll(upd => upd.Local.Entity).ArgMatches(), _description.SyncDescription);
        _pluginWriter.Received(1).UpdatePluginImages(data.PluginImages.Updates.ConvertAll(upd => upd.Local).ArgMatches());
        _customApiWriter.Received(1).UpdateCustomApis(data.CustomApis.Updates.ConvertAll(upd => upd.Local).ArgMatches(), _description.SyncDescription);
        _customApiWriter.Received(1).UpdateRequestParameters(data.RequestParameters.Updates.ConvertAll(upd => upd.Local.Entity).ArgMatches());
        _customApiWriter.Received(1).UpdateResponseProperties(data.ResponseProperties.Updates.ConvertAll(upd => upd.Local.Entity).ArgMatches());
    }
}
