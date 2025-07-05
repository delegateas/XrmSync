using DG.XrmPluginSync.Dataverse.Interfaces;
using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.Model.Plugin;
using DG.XrmPluginSync.SyncService;
using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.SyncService.Comparers;
using NSubstitute;

namespace Tests;

public class PluginValidationTests
{
    [Fact]
    public void ValidatePlugins_ThrowsException_ForPreOperationAsync()
    {
        // Arrange
        var pluginStep = new Step
        {
            Name = "TestStep",
            PluginTypeName = "TestType",
            ExecutionStage = (int)ExecutionStage.PreOperation, // Pre
            ExecutionMode = (int)ExecutionMode.Asynchronous, // Async
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            PluginImages = []
        };
        var pluginType = new PluginDefinition { Name = "TestType", PluginSteps = [pluginStep], Id = Guid.NewGuid() };
        var reader = Substitute.For<IPluginReader>();
        reader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns(new List<Step>());
        var plugin = new PluginService(
            Substitute.For<Microsoft.Extensions.Logging.ILogger>(),
            reader,
            Substitute.For<IPluginWriter>(),
            new Description(),
            new XrmPluginSyncOptions(),
            new PluginTypeComparer(),
            new PluginStepComparer(),
            new PluginImageComparer()
        );

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => plugin.ValidatePlugins([pluginType]));
        Assert.Contains("Pre execution stages does not support asynchronous execution mode", ex.Message);
    }

    [Fact]
    public void ValidatePlugins_ThrowsAggregateException_ForMultipleViolations()
    {
        // Arrange
        var pluginStep1 = new Step
        {
            Name = "Step1",
            PluginTypeName = "Type1",
            ExecutionStage = (int)ExecutionStage.PreOperation,
            ExecutionMode = (int)ExecutionMode.Asynchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            PluginImages = []
        };
        var pluginStep2 = new Step
        {
            Name = "Step2",
            PluginTypeName = "Type1",
            ExecutionStage = (int)ExecutionStage.PreOperation,
            ExecutionMode = (int)ExecutionMode.Synchronous,
            EventOperation = "Associate",
            LogicalName = "notempty",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "attr",
            UserContext = Guid.NewGuid(),
            PluginImages = []
        };
        var pluginType = new PluginDefinition { Name = "Type1", PluginSteps = [pluginStep1, pluginStep2], Id = Guid.NewGuid() };
        var reader = Substitute.For<IPluginReader>();
        reader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns(new List<Step>());
        var plugin = new PluginService(
            Substitute.For<Microsoft.Extensions.Logging.ILogger>(),
            reader,
            Substitute.For<IPluginWriter>(),
            new Description(),
            new XrmPluginSyncOptions(),
            new PluginTypeComparer(),
            new PluginStepComparer(),
            new PluginImageComparer()
        );

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() => plugin.ValidatePlugins([pluginType]));
        Assert.Contains("Pre execution stages does not support asynchronous execution mode", ex.InnerExceptions[0].Message);
        Assert.Contains("Associate/Disassociate events can't have filtered attributes", ex.InnerExceptions[1].Message);
        Assert.Contains("Associate/Disassociate events must target all entities", ex.InnerExceptions[2].Message);
    }
}