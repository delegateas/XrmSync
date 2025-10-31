using XrmPluginCore.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.Extensions;
using XrmSync.SyncService.Validation.Plugin;
using XrmSync.SyncService.Validation;
using XrmSync.SyncService.Validation.CustomApi;
using XrmSync.SyncService.Validation.CustomApi.Rules;
using XrmSync.SyncService.Validation.Plugin.Rules;

namespace Tests.Plugins;

public class PluginValidationTests
{
    private static IValidator<T> CreateValidator<T>(IPluginReader? pluginReader = null)
    {
        var services = new ServiceCollection();

        // Register validation rules using the extension method
        services.AddValidationRules();

        // Register mock or provided plugin reader
        var mockPluginReader = pluginReader ?? Substitute.For<IPluginReader>();
        services.AddSingleton(mockPluginReader);

        // Register the validators
        services.AddSingleton<IValidator<PluginDefinition>, PluginValidator>();
        services.AddSingleton<IValidator<CustomApiDefinition>, CustomApiValidator>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IValidator<T>>();
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
        Assert.Contains(stepRules, r => r is AssociateDisassociateEntityRule);
        Assert.Contains(stepRules, r => r is AssociateDisassociateFilterRule);
        Assert.Contains(stepRules, r => r is AsyncPreOperationRule);
        Assert.Contains(stepRules, r => r is CreatePreImageRule);
        Assert.Contains(stepRules, r => r is DeletePostImageRule);
        Assert.Contains(stepRules, r => r is MissingUserContextRule);
        Assert.Contains(stepRules, r => r is PreImageInPreStageRule);
        Assert.Contains(stepRules, r => r is AllowImageRule);

        var stepWithParentRules = serviceProvider.GetServices<IValidationRule<ParentReference<Step, PluginDefinition>>>().ToList();
        Assert.NotEmpty(stepWithParentRules);
        Assert.Contains(stepWithParentRules, r => r is DuplicateRegistrationRule);

        // Act & Assert - Verify CustomApi validation rules are registered
        var customApiRules = serviceProvider.GetServices<IValidationRule<CustomApiDefinition>>().ToList();
        Assert.NotEmpty(customApiRules);
        Assert.Contains(customApiRules, r => r is BoundApiEntityRule);
        Assert.Contains(customApiRules, r => r is UnboundApiEntityRule);
    }

    [Fact]
    public void ValidatePlugins_ThrowsException_ForAsyncPreOperation()
    {
        // Arrange
        var pluginType = new PluginDefinition("TestType")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [
                new Step("TestStep")
                {
                    ExecutionStage = ExecutionStage.PreOperation, // Pre
                    ExecutionMode = ExecutionMode.Asynchronous, // Async
                    EventOperation = nameof(EventOperation.Update),
                    LogicalName = "account",
                    Deployment = 0,
                    ExecutionOrder = 1,
                    FilteredAttributes = string.Empty,
                    UserContext = Guid.NewGuid(),
                    AsyncAutoDelete = false,
                    PluginImages = []
                }
                ]
        };

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = CreateValidator<PluginDefinition>(pluginReader);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([pluginType]));
        Assert.Contains("Pre-execution stages do not support asynchronous execution mode", ex.Message);
    }

    [Fact]
    public void ValidatePlugins_ThrowsAggregateException_ForMultipleViolations()
    {
        // Arrange
        var pluginStep1 = new Step("Step1")
        {
            ExecutionStage = ExecutionStage.PreOperation,
            ExecutionMode = ExecutionMode.Asynchronous,
            EventOperation = nameof(EventOperation.Update),
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginStep2 = new Step("Step2")
        {
            ExecutionStage = ExecutionStage.PreOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Associate),
            LogicalName = "notempty",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "attr",
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginType = new PluginDefinition("Type1")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStep1, pluginStep2]
        };

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = CreateValidator<PluginDefinition>(pluginReader);

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() => validator.ValidateOrThrow([pluginType]));
        var messages = ex.InnerExceptions.Select(e => e.Message).ToList();
        Assert.Contains(messages, x => x.Contains("Pre-execution stages do not support asynchronous execution mode"));
        Assert.Contains(messages, x => x.Contains("Plugin Step2: Associate event can't have filtered attributes"));
        Assert.Contains(messages, x => x.Contains("Plugin Step2: Associate event must target all entities"));
    }

    [Fact]
    public void ValidatePlugins_ThrowsException_ForMultipleRegistrationsOnSameMessageInSameType()
    {
        // Arrange
        var pluginStep1 = new Step("Step1")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Update),
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "accountnumber",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginStep2 = new Step("Step2")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Update),
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 2,
            FilteredAttributes = "name",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginType = new PluginDefinition("Type1")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStep1, pluginStep2]
        };

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = CreateValidator<PluginDefinition>(pluginReader);

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([pluginType]));
        Assert.Contains("Plugin Step1: Multiple registrations on the same message, stage and entity are not allowed", ex.Message);
    }

    [Fact]
    public void ValidatePlugins_NoException_ForMultipleRegistrationsOnSameMessageInDifferentType()
    {
        // Arrange
        var pluginStep1 = new Step("Step1")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Update),
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "accountnumber",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginStep2 = new Step("Step2")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Update),
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 2,
            FilteredAttributes = "name",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginType1 = new PluginDefinition("Type1") { Id = Guid.NewGuid(), PluginSteps = [pluginStep1] };

        var pluginType2 = new PluginDefinition("Type1") { Id = Guid.NewGuid(), PluginSteps = [pluginStep2] };

        var pluginReader = Substitute.For<IPluginReader>();
        pluginReader.GetMissingUserContexts(Arg.Any<IEnumerable<Step>>()).Returns([]);
        var validator = CreateValidator<PluginDefinition>(pluginReader);

        // Act & Assert
        validator.ValidateOrThrow([pluginType1, pluginType2]);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", null)]
    public void ValidatePlugins_NoException_AssociateDisassociateWithEmptyOrNullFilter(string logicalName, string filteredAttributes)
    {
        // Arrange
        var pluginStep1 = new Step("Step1")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Associate),
            LogicalName = logicalName,
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = filteredAttributes,
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginStep2 = new Step("Step2")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Disassociate),
            LogicalName = logicalName,
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = filteredAttributes,
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var pluginType1 = new PluginDefinition("Type") { Id = Guid.NewGuid(), PluginSteps = [pluginStep1, pluginStep2] };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert
        validator.ValidateOrThrow([pluginType1]);
    }

    [Theory]
    [InlineData(BindingType.Entity, "", "Bound Custom API must specify an entity type")]
    [InlineData((BindingType)0, "account", "Unbound Custom API cannot specify an entity type")] // 0 represents Global/Unbound
    public void ValidateCustomAPI_ThrowsException_ForInvalidEntityBinding(BindingType bindingType, string boundEntityLogicalName, string expectedErrorMessage)
    {
        // Arrange
        var customAPI = new CustomApiDefinition("TestAPI")
        {
            DisplayName = "TestAPI",
            UniqueName = "new_TestAPI",
            BindingType = bindingType,
            BoundEntityLogicalName = boundEntityLogicalName,
            Description = "A test custom API",
            IsFunction = false,
            IsPrivate = false,
            IsCustomizable = true,
            EnabledForWorkflow = false,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.AsyncOnly,
            ExecutePrivilegeName = string.Empty,
            PluginType = new PluginType("TestPluginType") { Id = Guid.NewGuid() }
        };

        var validator = CreateValidator<CustomApiDefinition>();

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([customAPI]));
        Assert.Contains(expectedErrorMessage, ex.Message);
    }

    [Theory]
    [InlineData(nameof(EventOperation.Create))]
    public void ValidatePlugins_ThrowsException_ForCreateOperationsWithPreImage(string eventOperation)
    {
        // Arrange - Create-type operations should not support pre-images
        var pluginStep = new Step($"{eventOperation}StepWithPreImage")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = eventOperation,
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = [
                new Image("PreImage")
                {
                    Id = Guid.NewGuid(),
                    EntityAlias = "PreImage",
                    ImageType = ImageType.PreImage,
                    Attributes = "name,accountnumber"
                }
            ]
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStep]
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([pluginType]));
        Assert.Contains("Create events do not support pre-images", ex.Message);
    }

    [Theory]
    [InlineData(nameof(EventOperation.Delete))]
    public void ValidatePlugins_ThrowsException_ForDeleteOperationsWithPostImage(string eventOperation)
    {
        // Arrange - Delete-type operations should not support post-images
        var pluginStep = new Step($"{eventOperation}StepWithPostImage")
        {
            ExecutionStage = ExecutionStage.PostOperation, // Use PostOperation to avoid triggering PreImageInPreStageRule
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = eventOperation,
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = [
                new Image("PostImage")
                {
                    Id = Guid.NewGuid(),
                    EntityAlias = "PostImage",
                    ImageType = ImageType.PostImage,
                    Attributes = "name,accountnumber"
                }
            ]
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStep]
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([pluginType]));
        Assert.Contains("Delete events do not support post-images", ex.Message);
    }

    [Theory]
    [InlineData(ExecutionStage.PreValidation)]
    [InlineData(ExecutionStage.PreOperation)]
    public void ValidatePlugins_ThrowsException_ForPreStagesWithPostImage(ExecutionStage executionStage)
    {
        // Arrange - Pre-execution stages should not support post-images
        var pluginStep = new Step($"{executionStage}WithPostImage")
        {
            ExecutionStage = executionStage,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = nameof(EventOperation.Update),
            LogicalName = "account",
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "name",
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = [
                new Image("PostImage")
                {
                    Id = Guid.NewGuid(),
                    EntityAlias = "PostImage",
                    ImageType = ImageType.PostImage,
                    Attributes = "name,accountnumber"
                }
            ]
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStep]
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert
        var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([pluginType]));
        Assert.Contains("Pre-execution stages do not support post-images", ex.Message);
    }

    [Theory]
    [InlineData(nameof(EventOperation.Update))]
    [InlineData(nameof(EventOperation.Merge))]
    [InlineData(nameof(EventOperation.SetState))]
    public void ValidatePlugins_NoException_ForUpdateOperationsWithBothImageTypes(string eventOperation)
    {
        // Arrange - Update-type operations should support both pre and post images
        // Create separate plugin types to avoid duplicate registration violation
        var pluginTypeWithPreImage = new PluginDefinition("TestPluginPre")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [
                new Step($"{eventOperation}StepWithPreImage")
                {
                    ExecutionStage = ExecutionStage.PostOperation,
                    ExecutionMode = ExecutionMode.Synchronous,
                    EventOperation = eventOperation,
                    LogicalName = "account",
                    Deployment = 0,
                    ExecutionOrder = 1,
                    FilteredAttributes = "name",
                    UserContext = Guid.Empty,
                    AsyncAutoDelete = false,
                    PluginImages = [
                        new Image("PreImage")
                        {
                            Id = Guid.NewGuid(),
                            EntityAlias = "PreImage",
                            ImageType = ImageType.PreImage,
                            Attributes = "name,accountnumber"
                        }
                    ]
                }
            ]
        };

        var pluginTypeWithPostImage = new PluginDefinition("TestPluginPost")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [
                new Step($"{eventOperation}StepWithPostImage")
                {
                    ExecutionStage = ExecutionStage.PostOperation,
                    ExecutionMode = ExecutionMode.Synchronous,
                    EventOperation = eventOperation,
                    LogicalName = "contact", // Different entity to avoid duplicate registration
                    Deployment = 0,
                    ExecutionOrder = 1,
                    FilteredAttributes = "fullname",
                    UserContext = Guid.Empty,
                    AsyncAutoDelete = false,
                    PluginImages = [
                        new Image("PostImage")
                        {
                            Id = Guid.NewGuid(),
                            EntityAlias = "PostImage",
                            ImageType = ImageType.PostImage,
                            Attributes = "fullname,emailaddress1"
                        }
                    ]
                }
            ]
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert - Should not throw any exceptions
        validator.ValidateOrThrow([pluginTypeWithPreImage]);
        validator.ValidateOrThrow([pluginTypeWithPostImage]);
    }

    [Fact]
    public void ValidatePlugins_NoException_ForValidImageConfigurations()
    {
        // Arrange - Valid image configurations should pass
        var pluginSteps = new[]
        {
            // Create with post-image is valid
            new Step("CreateWithPostImage")
            {
                ExecutionStage = ExecutionStage.PostOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Create),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PostImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PostImage",
                        ImageType = ImageType.PostImage,
                        Attributes = "name,accountnumber"
                    }
                ]
            },
            // Delete with pre-image is valid
            new Step("DeleteWithPreImage")
            {
                ExecutionStage = ExecutionStage.PreOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Delete),
                LogicalName = "contact", // Different entity to avoid duplicate registration
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PreImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PreImage",
                        ImageType = ImageType.PreImage,
                        Attributes = "fullname,emailaddress1"
                    }
                ]
            },
            // Update with pre-image in post-operation is valid
            new Step("UpdateWithPreImagePost")
            {
                ExecutionStage = ExecutionStage.PostOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Update),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = "name",
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PreImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PreImage",
                        ImageType = ImageType.PreImage,
                        Attributes = "name,accountnumber"
                    }
                ]
            },
            // Update with post-image in post-operation is valid (different entity)
            new Step("UpdateWithPostImagePost")
            {
                ExecutionStage = ExecutionStage.PostOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Update),
                LogicalName = "contact", // Different entity to avoid duplicate registration
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = "fullname",
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PostImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PostImage",
                        ImageType = ImageType.PostImage,
                        Attributes = "fullname,emailaddress1"
                    }
                ]
            },
            // Update with pre-image in pre-operation is valid (different entity)
            new Step("UpdateWithPreImagePre")
            {
                ExecutionStage = ExecutionStage.PreOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Update),
                LogicalName = "lead", // Different entity to avoid duplicate registration
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = "subject",
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PreImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PreImage",
                        ImageType = ImageType.PreImage,
                        Attributes = "subject,lastname"
                    }
                ]
            }
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = pluginSteps.ToList()
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert - Should not throw any exceptions
        validator.ValidateOrThrow([pluginType]);
    }

    [Fact]
    public void ValidatePlugins_NoException_ForStepsWithoutImages()
    {
        // Arrange - Steps without images should always be valid regardless of stage/operation
        var pluginSteps = new[]
        {
            new Step("CreateWithoutImages")
            {
                ExecutionStage = ExecutionStage.PostOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Create),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = []
            },
            new Step("DeleteWithoutImages")
            {
                ExecutionStage = ExecutionStage.PreOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Delete),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = []
            },
            new Step("UpdatePreValidationWithoutImages")
            {
                ExecutionStage = ExecutionStage.PreValidation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Update),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = "name",
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = []
            }
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = pluginSteps.ToList()
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert - Should not throw any exceptions
        validator.ValidateOrThrow([pluginType]);
    }

    [Fact]
    public void ValidatePlugins_ThrowsAggregateException_ForMultipleImageViolations()
    {
        // Arrange - Multiple image validation violations should be aggregated
        var pluginSteps = new[]
        {
            // Invalid: Create with pre-image
            new Step("CreateWithInvalidPreImage")
            {
                ExecutionStage = ExecutionStage.PostOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Create),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PreImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PreImage",
                        ImageType = ImageType.PreImage,
                        Attributes = "name"
                    }
                ]
            },
            // Invalid: Delete with post-image
            new Step("DeleteWithInvalidPostImage")
            {
                ExecutionStage = ExecutionStage.PreOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Delete),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = string.Empty,
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PostImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PostImage",
                        ImageType = ImageType.PostImage,
                        Attributes = "name"
                    }
                ]
            },
            // Invalid: Pre-operation with post-image
            new Step("PreOpWithInvalidPostImage")
            {
                ExecutionStage = ExecutionStage.PreOperation,
                ExecutionMode = ExecutionMode.Synchronous,
                EventOperation = nameof(EventOperation.Update),
                LogicalName = "account",
                Deployment = 0,
                ExecutionOrder = 1,
                FilteredAttributes = "name",
                UserContext = Guid.Empty,
                AsyncAutoDelete = false,
                PluginImages = [
                    new Image("PostImage")
                    {
                        Id = Guid.NewGuid(),
                        EntityAlias = "PostImage",
                        ImageType = ImageType.PostImage,
                        Attributes = "name"
                    }
                ]
            }
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = pluginSteps.ToList()
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() => validator.ValidateOrThrow([pluginType]));
        var messages = ex.InnerExceptions.Select(e => e.Message).ToList();

        Assert.Contains(messages, x => x.Contains("Create events do not support pre-images"));
        Assert.Contains(messages, x => x.Contains("Delete events do not support post-images"));
        Assert.Contains(messages, x => x.Contains("Pre-execution stages do not support post-images"));
    }

    [Theory]
    [MemberData(nameof(GetIllegalEventOperationsWithImages))]
    public void ValidatePlugins_ThrowsException_ForIllegalEventOperationsWithImages(string eventOperation)
    {
        // Arrange - Operations not explicitly allowed to support images should not support images
        var pluginStepWithPreImage = new Step($"{eventOperation}StepWithPreImage")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = eventOperation,
            LogicalName = string.Empty,
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = [
                new Image("PreImage")
                {
                    Id = Guid.NewGuid(),
                    EntityAlias = "PreImage",
                    ImageType = ImageType.PreImage,
                    Attributes = "name"
                }
            ]
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStepWithPreImage]
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert - Should throw validation exception indicating images are not supported
        var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([pluginType]));
        Assert.EndsWith(eventOperation + " message does not support entity images", ex.Message);
    }

    [Theory]
    [MemberData(nameof(GetSupportPreImages))]
    public void ValidatePlugins_NoException_ForLegalEventOperationsWithPreImages(string eventOperation)
    {
        // Arrange - Operations not explicitly allowed to support images should not support images
        var pluginStepWithPreImage = new Step($"{eventOperation}StepWithPreImage")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = eventOperation,
            LogicalName = string.Empty,
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = [
                new Image("PreImage")
                {
                    Id = Guid.NewGuid(),
                    EntityAlias = "PreImage",
                    ImageType = ImageType.PreImage,
                    Attributes = "name"
                }
            ]
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStepWithPreImage]
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert
        validator.ValidateOrThrow([pluginType]);
    }

    [Theory]
    [MemberData(nameof(GetSupportPostImages))]
    public void ValidatePlugins_NoException_ForLegalEventOperationsWithPostImages(string eventOperation)
    {
        // Arrange - Operations not explicitly allowed to support images should not support images
        var pluginStepWithPostImage = new Step($"{eventOperation}StepWithPostImage")
        {
            ExecutionStage = ExecutionStage.PostOperation,
            ExecutionMode = ExecutionMode.Synchronous,
            EventOperation = eventOperation,
            LogicalName = string.Empty,
            Deployment = 0,
            ExecutionOrder = 1,
            FilteredAttributes = string.Empty,
            UserContext = Guid.Empty,
            AsyncAutoDelete = false,
            PluginImages = [
                new Image("PostImage")
                {
                    Id = Guid.NewGuid(),
                    EntityAlias = "PostImage",
                    ImageType = ImageType.PostImage,
                    Attributes = "name"
                }
            ]
        };

        var pluginType = new PluginDefinition("TestPlugin")
        {
            Id = Guid.NewGuid(),
            PluginSteps = [pluginStepWithPostImage]
        };

        var validator = CreateValidator<PluginDefinition>();

        // Act & Assert
        validator.ValidateOrThrow([pluginType]);
    }

    /// <summary>
    /// Generates test data for event operations that should not support images.
    /// Based on Microsoft documentation: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/register-plug-in#define-entity-images
    /// Only Create, Delete, Update, and some related operations support images.
    /// </summary>
    public static IEnumerable<object[]> GetIllegalEventOperationsWithImages()
    {
        // Operations that explicitly support images according to Microsoft documentation
        var validImageOperations = new HashSet<string>
        {
            // Allowed operations for images according to Microsoft documentation
            nameof(EventOperation.Create),
            nameof(EventOperation.Delete),
            nameof(EventOperation.DeliverIncoming),
            nameof(EventOperation.DeliverPromote),
            nameof(EventOperation.Merge),
            nameof(EventOperation.Route),
            nameof(EventOperation.Send),
            nameof(EventOperation.SetState),
            nameof(EventOperation.Update)
        };

        // Get all EventOperation enum values and return those that should not support images
        var allEventOperations = Enum.GetNames(typeof(EventOperation));
        
        return allEventOperations
            .Where(op => !validImageOperations.Contains(op))
            .Select(op => new object[] { op })
            .ToArray(); // Force evaluation to help with test discovery
    }

    public static IEnumerable<object[]> GetSupportPreImages()
    {
        return [
            [ nameof(EventOperation.Delete) ],
            [ nameof(EventOperation.DeliverIncoming) ],
            [ nameof(EventOperation.DeliverPromote) ],
            [ nameof(EventOperation.Merge) ],
            [ nameof(EventOperation.Route) ],
            [ nameof(EventOperation.Send) ],
            [ nameof(EventOperation.SetState) ],
            [ nameof(EventOperation.Update) ]
        ];
    }

    public static IEnumerable<object[]> GetSupportPostImages()
    {
        return [
            [ nameof(EventOperation.Create) ],
            [ nameof(EventOperation.DeliverIncoming) ],
            [ nameof(EventOperation.DeliverPromote) ],
            [ nameof(EventOperation.Merge) ],
            [ nameof(EventOperation.Route) ],
            [ nameof(EventOperation.Send) ],
            [ nameof(EventOperation.SetState) ],
            [ nameof(EventOperation.Update) ]
        ];
    }
}