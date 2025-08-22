using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.SyncService.Exceptions;
using XrmSync.Model.Plugin;
using XrmSync.Model;
using XrmSync.SyncService.PluginValidator;
using DG.XrmPluginCore.Enums;

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
            PluginType = new PluginDefinition { Name = "TestType", PluginSteps = [], Id = Guid.NewGuid() },
            ExecutionStage = ExecutionStage.PreOperation, // Pre
            ExecutionMode = ExecutionMode.Asynchronous, // Async
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };
        var pluginType = pluginStep.PluginType;
        pluginType.PluginSteps.Add(pluginStep);

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = new PluginValidator(pluginReader);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.Validate([pluginType]));
        Assert.Contains("Pre execution stages does not support asynchronous execution mode", ex.Message);
    }

    [Fact]
    public void ValidatePlugins_ThrowsAggregateException_ForMultipleViolations()
    {
        // Arrange
        var pluginStep1 = new Step
        {
            Name = "Step1",
            PluginType = new PluginDefinition { Name = "Type1", PluginSteps = [], Id = Guid.NewGuid() },
            ExecutionStage = ExecutionStage.PreOperation,
            ExecutionMode = ExecutionMode.Asynchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginStep2 = new Step
        {
            Name = "Step2",
            PluginType = pluginStep1.PluginType,
            ExecutionStage = ExecutionStage.PreOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = "Associate",
            LogicalName = "notempty",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "attr",
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };
        var pluginType = pluginStep1.PluginType;
        pluginType.PluginSteps = [pluginStep1, pluginStep2];

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = new PluginValidator(pluginReader);

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() => validator.Validate([pluginType]));
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
            PluginType = new PluginDefinition { Name = "Type1", PluginSteps = [], Id = Guid.NewGuid() },
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "accountnumber",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginStep2 = new Step
        {
            Name = "Step2",
            PluginType = pluginStep1.PluginType,
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 2,
            FilteredAttributes = "name",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginType = pluginStep1.PluginType;
        pluginType.PluginSteps = [pluginStep1, pluginStep2];

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = new PluginValidator(pluginReader);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.Validate([pluginType]));
        Assert.Contains("Plugin Step1: Multiple registrations on the same message, stage and entity are not allowed", ex.Message);
    }

    [Fact]
    public void ValidatePlugins_NoException_ForMultipleRegistrationsOnSameMessageInDifferentType()
    {
        // Arrange
        var pluginStep1 = new Step
        {
            Name = "Step1",
            PluginType = new PluginDefinition { Name = "Type1", PluginSteps = [], Id = Guid.NewGuid() },
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "accountnumber",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginStep2 = new Step
        {
            Name = "Step2",
            PluginType = new PluginDefinition { Name = "Type2", PluginSteps = [], Id = Guid.NewGuid() },
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = "Update",
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 2,
            FilteredAttributes = "name",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginType1 = pluginStep1.PluginType;
        pluginType1.PluginSteps = [pluginStep1];

        var pluginType2 = pluginStep2.PluginType;
        pluginType2.PluginSteps = [pluginStep2];

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = new PluginValidator(pluginReader);

        // Act & Assert
        validator.Validate([pluginType1, pluginType2]);
    }
}