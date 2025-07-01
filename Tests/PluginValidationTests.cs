using DG.XrmPluginSync.Dataverse.Interfaces;
using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.SyncService;
using DG.XrmPluginSync.SyncService.Common;
using NSubstitute;

namespace Tests;

public class PluginValidationTests
{
    [Fact]
    public void ValidatePlugins_ThrowsException_ForPreOperationAsync()
    {
        // Arrange
        var pluginStep = new PluginStepEntity
        {
            Name = "TestStep",
            PluginTypeName = "TestType",
            ExecutionStage = (int)ExecutionStage.Pre, // Pre
            ExecutionMode = (int)ExecutionMode.Asynchronous, // Async
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            PluginImages = new List<PluginImageEntity>()
        };
        var pluginType = new PluginTypeEntity { Name = "TestType", PluginSteps = new List<PluginStepEntity> { pluginStep }, Id = Guid.NewGuid() };
        var reader = Substitute.For<IPluginReader>();
        reader.GetMissingUserContexts(Arg.Any<IEnumerable<PluginStepEntity>>()).Returns(new List<PluginStepEntity>());
        var plugin = new Plugin(Substitute.For<Microsoft.Extensions.Logging.ILogger>(), reader, Substitute.For<IPluginWriter>(), new Description());

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => plugin.ValidatePlugins(new List<PluginTypeEntity> { pluginType }));
        Assert.Contains("Pre execution stages does not support asynchronous execution mode", ex.Message);
    }

    [Fact]
    public void ValidatePlugins_ThrowsAggregateException_ForMultipleViolations()
    {
        // Arrange
        var pluginStep1 = new PluginStepEntity
        {
            Name = "Step1",
            PluginTypeName = "Type1",
            ExecutionStage = (int)ExecutionStage.Pre,
            ExecutionMode = (int)ExecutionMode.Asynchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            PluginImages = new List<PluginImageEntity>()
        };
        var pluginStep2 = new PluginStepEntity
        {
            Name = "Step2",
            PluginTypeName = "Type1",
            ExecutionStage = (int)ExecutionStage.Pre,
            ExecutionMode = (int)ExecutionMode.Synchronous,
            EventOperation = "Associate",
            LogicalName = "notempty",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "attr",
            UserContext = Guid.NewGuid(),
            PluginImages = new List<PluginImageEntity>()
        };
        var pluginType = new PluginTypeEntity { Name = "Type1", PluginSteps = new List<PluginStepEntity> { pluginStep1, pluginStep2 }, Id = Guid.NewGuid() };
        var reader = Substitute.For<IPluginReader>();
        reader.GetMissingUserContexts(Arg.Any<IEnumerable<PluginStepEntity>>()).Returns(new List<PluginStepEntity>());
        var plugin = new Plugin(Substitute.For<Microsoft.Extensions.Logging.ILogger>(), reader, Substitute.For<IPluginWriter>(), new Description());

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() => plugin.ValidatePlugins(new List<PluginTypeEntity> { pluginType }));
        Assert.Contains("Pre execution stages does not support asynchronous execution mode", ex.InnerExceptions[0].Message);
        Assert.Contains("Associate/Disassociate events can't have filtered attributes", ex.InnerExceptions[1].Message);
        Assert.Contains("Associate/Disassociate events must target all entities", ex.InnerExceptions[2].Message);
    }
}