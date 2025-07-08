using DG.XrmSync.Dataverse.Interfaces;
using DG.XrmSync.Model;
using DG.XrmSync.Model.Plugin;
using DG.XrmSync.SyncService;
using DG.XrmSync.SyncService.Differences;
using DG.XrmSync.SyncService.AssemblyReader;
using Microsoft.Extensions.Logging;
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
            ExecutionStage = (int)ExecutionStage.Pre, // Pre
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
        var pluginReader = Substitute.For<IPluginReader>();
        var pluginWriter = Substitute.For<IPluginWriter>();
        var customApiReader = Substitute.For<ICustomApiReader>();
        var customApiWriter = Substitute.For<ICustomApiWriter>();
        var assemblyReader = Substitute.For<IAssemblyReader>();
        var solutionReader = Substitute.For<ISolutionReader>();
        var differenceUtility = Substitute.For<IDifferenceUtility>();
        var logger = Substitute.For<ILogger>();
        
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns(new List<Step>());
        
        var plugin = new PluginSyncService(
            pluginReader,
            pluginWriter,
            customApiReader,
            customApiWriter,
            assemblyReader,
            solutionReader,
            differenceUtility,
            new Description(),
            new XrmSyncOptions(),
            logger
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
            ExecutionStage = (int)ExecutionStage.Pre,
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
            ExecutionStage = (int)ExecutionStage.Pre,
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
        var pluginReader = Substitute.For<IPluginReader>();
        var pluginWriter = Substitute.For<IPluginWriter>();
        var customApiReader = Substitute.For<ICustomApiReader>();
        var customApiWriter = Substitute.For<ICustomApiWriter>();
        var assemblyReader = Substitute.For<IAssemblyReader>();
        var solutionReader = Substitute.For<ISolutionReader>();
        var differenceUtility = Substitute.For<IDifferenceUtility>();
        var logger = Substitute.For<ILogger>();
        
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns(new List<Step>());
        
        var plugin = new PluginSyncService(
            pluginReader,
            pluginWriter,
            customApiReader,
            customApiWriter,
            assemblyReader,
            solutionReader,
            differenceUtility,
            new Description(),
            new XrmSyncOptions(),
            logger
        );

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() => plugin.ValidatePlugins([pluginType]));
        Assert.Contains("Pre execution stages does not support asynchronous execution mode", ex.InnerExceptions[0].Message);
        Assert.Contains("Associate/Disassociate events can't have filtered attributes", ex.InnerExceptions[1].Message);
        Assert.Contains("Associate/Disassociate events must target all entities", ex.InnerExceptions[2].Message);
    }

    [Fact]
    public void ValidatePlugins_ThrowsException_ForMultipleRegistrationsOnSameMessageInSameType()
    {
        // Arrange
        var pluginStep1 = new Step
        {
            Name = "Step1",
            PluginTypeName = "Type1",
            ExecutionStage = (int)ExecutionStage.Post,
            ExecutionMode = (int)ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "accountnumber",
            UserContext = Guid.Empty,
            PluginImages = []
        };

        var pluginStep2 = new Step
        {
            Name = "Step2",
            PluginTypeName = "Type1",
            ExecutionStage = (int)ExecutionStage.Post,
            ExecutionMode = (int)ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 2,
            FilteredAttributes = "name",
            UserContext = Guid.Empty,
            PluginImages = []
        };

        var pluginType = new PluginDefinition { Name = "Type1", PluginSteps = [pluginStep1, pluginStep2], Id = Guid.NewGuid() };

        var pluginReader = Substitute.For<IPluginReader>();
        var pluginWriter = Substitute.For<IPluginWriter>();
        var customApiReader = Substitute.For<ICustomApiReader>();
        var customApiWriter = Substitute.For<ICustomApiWriter>();
        var assemblyReader = Substitute.For<IAssemblyReader>();
        var solutionReader = Substitute.For<ISolutionReader>();
        var differenceUtility = Substitute.For<IDifferenceUtility>();
        var logger = Substitute.For<ILogger>();
        
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns(new List<Step>());
        
        var plugin = new PluginSyncService(
            pluginReader,
            pluginWriter,
            customApiReader,
            customApiWriter,
            assemblyReader,
            solutionReader,
            differenceUtility,
            new Description(),
            new XrmSyncOptions(),
            logger
        );

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => plugin.ValidatePlugins([pluginType]));
        Assert.Contains("Plugin Step1: Multiple registrations on the same message, stage and entity are not allowed", ex.Message);
    }

    [Fact]
    public void ValidatePlugins_NoException_ForMultipleRegistrationsOnSameMessageInDifferentType()
    {
        // Arrange
        var pluginStep1 = new Step
        {
            Name = "Step1",
            PluginTypeName = "Type1",
            ExecutionStage = (int)ExecutionStage.Post,
            ExecutionMode = (int)ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "accountnumber",
            UserContext = Guid.Empty,
            PluginImages = []
        };

        var pluginStep2 = new Step
        {
            Name = "Step2",
            PluginTypeName = "Type2",
            ExecutionStage = (int)ExecutionStage.Post,
            ExecutionMode = (int)ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 2,
            FilteredAttributes = "name",
            UserContext = Guid.Empty,
            PluginImages = []
        };

        var pluginType1 = new PluginDefinition { Name = "Type1", PluginSteps = [pluginStep1], Id = Guid.NewGuid() };
        var pluginType2 = new PluginDefinition { Name = "Type2", PluginSteps = [pluginStep2], Id = Guid.NewGuid() };

        var pluginReader = Substitute.For<IPluginReader>();
        var pluginWriter = Substitute.For<IPluginWriter>();
        var customApiReader = Substitute.For<ICustomApiReader>();
        var customApiWriter = Substitute.For<ICustomApiWriter>();
        var assemblyReader = Substitute.For<IAssemblyReader>();
        var solutionReader = Substitute.For<ISolutionReader>();
        var differenceUtility = Substitute.For<IDifferenceUtility>();
        var logger = Substitute.For<ILogger>();

        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns(new List<Step>());

        var plugin = new PluginSyncService(
            pluginReader,
            pluginWriter,
            customApiReader,
            customApiWriter,
            assemblyReader,
            solutionReader,
            differenceUtility,
            new Description(),
            new XrmSyncOptions(),
            logger
        );

        // Act & Assert
        plugin.ValidatePlugins([pluginType1, pluginType2]);
    }
}