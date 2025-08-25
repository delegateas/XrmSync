using DG.XrmPluginCore.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using XrmSync.Dataverse;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.Extensions;
using XrmSync.SyncService.PluginValidator;
using XrmSync.SyncService.PluginValidator.Rules;
using XrmSync.SyncService.PluginValidator.Rules.CustomApi;
using XrmSync.SyncService.PluginValidator.Rules.Plugin;

namespace Tests;

public class PluginValidationTests
{
    private static IPluginValidator CreateValidator(IPluginReader? pluginReader = null)
    {
        var services = new ServiceCollection();
        
        // Register validation rules using the extension method
        services.AddValidationRules();
        
        // Register mock or provided plugin reader
        var mockPluginReader = pluginReader ?? Substitute.For<IPluginReader>();
        services.AddSingleton(mockPluginReader);
        
        // Register the validator
        services.AddSingleton<IPluginValidator, PluginValidator>();
        
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IPluginValidator>();
    }

    [Fact]
    public void ValidationRules_AreDiscovered_Correctly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValidationRules();

        // Register mock or provided plugin reader
        services.AddSingleton(Substitute.For<IPluginReader>());

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Verify Step validation rules are registered
        var stepRules = serviceProvider.GetServices<IValidationRule<Step>>().ToList();
        Assert.NotEmpty(stepRules);
        Assert.Contains(stepRules, r => r is AsyncPreOperationRule);
        Assert.Contains(stepRules, r => r is PreImageInPreStageRule);
        Assert.Contains(stepRules, r => r is DuplicateRegistrationRule);

        // Act & Assert - Verify CustomApi validation rules are registered
        var customApiRules = serviceProvider.GetServices<IValidationRule<CustomApiDefinition>>().ToList();
        Assert.NotEmpty(customApiRules);
        Assert.Contains(customApiRules, r => r is BoundApiEntityRule);
        Assert.Contains(customApiRules, r => r is UnboundApiEntityRule);
    }

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
        var validator = CreateValidator(pluginReader);

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
        var validator = CreateValidator(pluginReader);

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() => validator.Validate([pluginType]));
        var messages = ex.InnerExceptions.Select(e => e.Message).ToList();
        Assert.Contains(messages, x => x.Contains("Pre execution stages does not support asynchronous execution mode"));
        Assert.Contains(messages, x => x.Contains("Plugin Step2: Associate/Disassociate events can't have filtered attributes"));
        Assert.Contains(messages, x => x.Contains("Plugin Step2: Associate/Disassociate events must target all entities"));
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
        var validator = CreateValidator(pluginReader);

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
        var validator = CreateValidator(pluginReader);

        // Act & Assert
        validator.Validate([pluginType1, pluginType2]);
    }

    [Fact]
    public void ValidateCustomAPI_ThrowsException_BoundWithoutEntity()
    {
        // Arrange
        var customAPI = new CustomApiDefinition
        {
            Name = "TestBoundAPI",
            DisplayName = "TestBoundAPI",
            UniqueName = "new_TestBoundAPI",
            BindingType = BindingType.Entity, // or similar enum value indicating bound
            BoundEntityLogicalName = string.Empty, // or string.Empty - this is the violation
            Description = "A test bound custom API",
            IsFunction = false,
            IsPrivate = false,
            IsCustomizable = true,
            EnabledForWorkflow = false,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.AsyncOnly,
            ExecutePrivilegeName = string.Empty,
            PluginType = new PluginType { Name = "TestPluginType", Id = Guid.NewGuid() }
        };

        var validator = CreateValidator();

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.Validate([customAPI]));
        Assert.Contains("Bound Custom API must specify an entity type", ex.Message);
    }

    [Fact]
    public void ValidateCustomAPI_ThrowsException_UnboundWithEntity()
    {
        // Arrange
        var customAPI = new CustomApiDefinition
        {
            Name = "TestUnboundAPI",
            DisplayName = "TestUnboundAPI",
            UniqueName = "new_TestUnboundAPI",
            BindingType = 0, // Unbound has type Global, which isn't mapped in the enum
            BoundEntityLogicalName = "account", // or similar entity name - this is the violation
            Description = "A test unbound custom API",
            IsFunction = false,
            IsPrivate = false,
            IsCustomizable = true,
            EnabledForWorkflow = false,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.AsyncOnly,
            ExecutePrivilegeName = string.Empty,
            PluginType = new PluginType { Name = "TestPluginType", Id = Guid.NewGuid() }
        };

        var validator = CreateValidator();

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.Validate([customAPI]));
        Assert.Contains("Unbound Custom API cannot specify an entity type", ex.Message);
    }
}