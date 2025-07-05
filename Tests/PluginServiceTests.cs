using DG.XrmSync.Model;
using DG.XrmSync.SyncService;
using DG.XrmSync.Dataverse.Interfaces;
using DG.XrmSync.SyncService.Comparers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using DG.XrmSync.Model.Plugin;
using DG.XrmSync.SyncService.AssemblyReader;
using DG.XrmSync.Model.CustomApi;

namespace Tests;

public class PluginServiceTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly IPluginReader _pluginReader = Substitute.For<IPluginReader>();
    private readonly IPluginWriter _pluginWriter = Substitute.For<IPluginWriter>();
    private readonly IAssemblyReader _assemblyReader = Substitute.For<IAssemblyReader>();
    private readonly ISolutionReader _solutionReader = Substitute.For<ISolutionReader>();
    private readonly Description _description = new();
    private readonly XrmSyncOptions _options = new();
    private readonly PluginTypeComparer _typeComparer = new();
    private readonly PluginStepComparer _stepComparer = new();
    private readonly PluginImageComparer _imageComparer = new();
    private readonly PluginSyncService _plugin;

    public PluginServiceTests()
    {
        _plugin = new PluginSyncService(_pluginReader, _pluginWriter, _assemblyReader, _solutionReader, _description, _options, _typeComparer, _stepComparer, _imageComparer, _logger);
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
        var solutionName = "solution";
        var expectedId = Guid.NewGuid();
        _pluginWriter.CreatePluginAssembly(assembly.Name, solutionName, assembly.DllPath, assembly.Hash, assembly.Version, _description.SyncDescription)
            .Returns(expectedId);

        // Act
        var result = _plugin.CreatePluginAssembly(assembly, solutionName);

        // Assert
        Assert.Equal(expectedId, result.Id);
        _pluginWriter.Received(1).CreatePluginAssembly(assembly.Name, solutionName, assembly.DllPath, assembly.Hash, assembly.Version, _description.SyncDescription);
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
    public void CreatePlugins_CallsWriterForTypesStepsImages()
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
        var crmPluginSteps = new List<Step>();
        var solutionName = "solution";
        var pluginTypes = new List<PluginDefinition> {
            new() {
                Name = "Type1",
                PluginSteps = [],
                Id = Guid.NewGuid()
            }
        };
        var pluginSteps = new List<Step> {
            new() {
                Name = "Step1",
                PluginTypeName = "Type1",
                ExecutionStage = 10,
                EventOperation = "Update",
                LogicalName = "account",
                Deployment = 0,
                ExecutionMode = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.NewGuid(),
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
        var createdTypes = new List<PluginDefinition> {
            new() {
                Name = "CreatedType",
                PluginSteps = [],
                Id = Guid.NewGuid()
            }
        };
        var createdSteps = new List<Step> {
            new() {
                Name = "CreatedStep",
                PluginTypeName = "Type1",
                ExecutionStage = 10,
                EventOperation = "Update",
                LogicalName = "account",
                Deployment = 0,
                ExecutionMode = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.NewGuid(),
                PluginImages = []
            }
        };
        _pluginWriter.CreatePluginTypes(pluginTypes, crmAssembly.Id, _description.SyncDescription).Returns(createdTypes);
        _pluginWriter.CreatePluginSteps(pluginSteps, Arg.Any<List<PluginDefinition>>(), solutionName, _description.SyncDescription).Returns(createdSteps);

        // Act
        _plugin.CreatePlugins(crmAssembly, crmPluginSteps, solutionName, pluginTypes, pluginSteps, pluginImages);

        // Assert
        _pluginWriter.Received(1).CreatePluginTypes(pluginTypes, crmAssembly.Id, _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginSteps(pluginSteps, crmAssembly.Plugins, solutionName, _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginImages(pluginImages, crmPluginSteps);
        Assert.Contains(createdTypes[0], crmAssembly.Plugins);
        Assert.Contains(createdSteps[0], crmPluginSteps);
    }

    [Fact]
    public void DeletePlugins_CallsWriter()
    {
        // Arrange
        var types = new List<PluginType>();
        var steps = new List<Step>();
        var images = new List<Image>();
        var apis = new List<ApiDefinition>();
        var reqs = new List<RequestParameter>();
        var resps = new List<ResponseProperty>();

        // Act
        _plugin.DeletePlugins(new CompiledData(types, steps, images, apis, reqs, resps));

        // Assert
        _pluginWriter.Received(1).DeletePlugins(types, steps, images, apis, reqs, resps);
    }

    [Fact]
    public void UpdatePlugins_CallsWriter()
    {
        // Arrange
        var steps = new List<Step>();
        var images = new List<Image>();

        // Act
        _plugin.UpdatePlugins(steps, images);

        // Assert
        _pluginWriter.Received(1).UpdatePlugins(steps, images, _description.SyncDescription);
    }
}
