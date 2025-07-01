using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.SyncService;
using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.Dataverse.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests;

public class PluginServiceTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly IPluginReader _pluginReader = Substitute.For<IPluginReader>();
    private readonly IPluginWriter _pluginWriter = Substitute.For<IPluginWriter>();
    private readonly Description _description = new();
    private readonly Plugin _plugin;

    public PluginServiceTests()
    {
        _plugin = new Plugin(_logger, _pluginReader, _pluginWriter, _description);
    }

    [Fact]
    public void CreatePluginAssembly_CallsWriterAndReturnsAssemblyWithId()
    {
        // Arrange
        var assembly = new PluginAssembly {
            Name = "TestAssembly",
            DllPath = "path",
            Hash = "hash",
            Version = "1.0.0.0",
            PluginTypes = new List<PluginTypeEntity>()
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
        var assembly = new PluginAssembly {
            Name = "TestAssembly",
            DllPath = "path",
            Hash = "hash",
            Version = "1.0.0.0",
            PluginTypes = new List<PluginTypeEntity>()
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
        var crmAssembly = new PluginAssembly {
            Id = Guid.NewGuid(),
            Name = "TestAssembly",
            DllPath = "path",
            Hash = "hash",
            Version = "1.0.0.0",
            PluginTypes = new List<PluginTypeEntity>()
        };
        var crmPluginSteps = new List<PluginStepEntity>();
        var solutionName = "solution";
        var pluginTypes = new List<PluginTypeEntity> {
            new() {
                Name = "Type1",
                PluginSteps = new List<PluginStepEntity>(),
                Id = Guid.NewGuid()
            }
        };
        var pluginSteps = new List<PluginStepEntity> {
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
                PluginImages = new List<PluginImageEntity>()
            }
        };
        var pluginImages = new List<PluginImageEntity> {
            new() {
                Name = "Image1",
                PluginStepName = "Step1",
                EntityAlias = "alias",
                ImageType = 0,
                Attributes = string.Empty
            }
        };
        var createdTypes = new List<PluginTypeEntity> {
            new() {
                Name = "CreatedType",
                PluginSteps = new List<PluginStepEntity>(),
                Id = Guid.NewGuid()
            }
        };
        var createdSteps = new List<PluginStepEntity> {
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
                PluginImages = new List<PluginImageEntity>()
            }
        };
        _pluginWriter.CreatePluginTypes(pluginTypes, crmAssembly.Id, _description.SyncDescription).Returns(createdTypes);
        _pluginWriter.CreatePluginSteps(pluginSteps, Arg.Any<List<PluginTypeEntity>>(), solutionName, _description.SyncDescription).Returns(createdSteps);

        // Act
        _plugin.CreatePlugins(crmAssembly, crmPluginSteps, solutionName, pluginTypes, pluginSteps, pluginImages);

        // Assert
        _pluginWriter.Received(1).CreatePluginTypes(pluginTypes, crmAssembly.Id, _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginSteps(pluginSteps, crmAssembly.PluginTypes, solutionName, _description.SyncDescription);
        _pluginWriter.Received(1).CreatePluginImages(pluginImages, crmPluginSteps);
        Assert.Contains(createdTypes[0], crmAssembly.PluginTypes);
        Assert.Contains(createdSteps[0], crmPluginSteps);
    }

    [Fact]
    public void DeletePlugins_CallsWriter()
    {
        // Arrange
        var types = new List<PluginTypeEntity>();
        var steps = new List<PluginStepEntity>();
        var images = new List<PluginImageEntity>();

        // Act
        _plugin.DeletePlugins(types, steps, images);

        // Assert
        _pluginWriter.Received(1).DeletePlugins(types, steps, images);
    }

    [Fact]
    public void UpdatePlugins_CallsWriter()
    {
        // Arrange
        var steps = new List<PluginStepEntity>();
        var images = new List<PluginImageEntity>();

        // Act
        _plugin.UpdatePlugins(steps, images);

        // Assert
        _pluginWriter.Received(1).UpdatePlugins(steps, images, _description.SyncDescription);
    }
}
