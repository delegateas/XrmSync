using DG.XrmPluginCore.Enums;
using Microsoft.Extensions.Logging;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.Difference;
using XrmSync.SyncService.Extensions;

namespace Tests;

public class DifferenceUtilityTests
{
    private readonly DifferenceUtility _differenceUtility;

    public DifferenceUtilityTests()
    {
        var logger = new LoggerFactory().CreateLogger<DifferenceUtility>();
        var description = new Description();
        _differenceUtility = new DifferenceUtility(
            logger,
            new PluginTypeComparer(),
            new PluginStepComparer(),
            new PluginImageComparer(),
            new CustomApiComparer(description),
            new RequestParameterComparer(),
            new ResponsePropertyComparer()
        );
    }

    [Fact]
    public void CalculateDifferences_ReturnsCorrectDifferences()
    {
        // Arrange
        var localType = new PluginType { Name = "LocalType", Id = Guid.NewGuid() };
        var remoteType = new PluginType { Name = "RemoteType", Id = Guid.NewGuid() };
        var sharedType = new PluginType { Name = "SharedType", Id = Guid.NewGuid() };
        
        var localStep = new Step {
            Name = "LocalStep",
            PluginTypeName = "TestType",
            ExecutionStage = ExecutionStage.PreValidation,
            EventOperation = "Create",
            LogicalName = "account",
            Deployment = 0,
            ExecutionMode = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "",
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };
        
        var remoteStep = new Step {
            Name = "RemoteStep",
            PluginTypeName = "TestType",
            ExecutionStage = ExecutionStage.PreValidation,
            EventOperation = "Create",
            LogicalName = "account",
            Deployment = 0,
            ExecutionMode = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "",
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };

        var localData = new CompiledData(
            [localType, sharedType],
            [localStep],
            [],
            [],
            [],
            []
        );
        
        var remoteData = new CompiledData(
            [remoteType, sharedType],
            [remoteStep],
            [],
            [],
            [],
            []
        );

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);

        // Assert
        Assert.Single(differences.Types.Creates);
        Assert.Equal("LocalType", differences.Types.Creates[0].Name);
        Assert.Single(differences.Types.Deletes);
        Assert.Equal("RemoteType", differences.Types.Deletes[0].Name);
        
        Assert.Single(differences.PluginSteps.Creates);
        Assert.Equal("LocalStep", differences.PluginSteps.Creates[0].Name);
        Assert.Single(differences.PluginSteps.Deletes);
        Assert.Equal("RemoteStep", differences.PluginSteps.Deletes[0].Name);

        Assert.Empty(differences.PluginSteps.UpdatesWithDifferences);
    }

    [Fact]
    public void CalculateDifferences_EmptyData_ReturnsEmptyDifferences()
    {
        // Arrange
        var emptyData = new CompiledData([], [], [], [], [], []);

        // Act
        var differences = _differenceUtility.CalculateDifferences(emptyData, emptyData);

        // Assert
        Assert.Empty(differences.Types.Creates);
        Assert.Empty(differences.Types.Deletes);
        Assert.Empty(differences.Types.Updates);
        Assert.Empty(differences.PluginSteps.Creates);
        Assert.Empty(differences.PluginSteps.Deletes);
        Assert.Empty(differences.PluginSteps.Updates);
        Assert.Empty(differences.PluginImages.Creates);
        Assert.Empty(differences.PluginImages.Deletes);
        Assert.Empty(differences.PluginImages.Updates);
    }

    [Fact]
    public void CalculateDifferences_UpdatesDetected_ReturnsUpdates()
    {
        // Arrange
        var localStep = new Step {
            Name = "TestStep",
            PluginTypeName = "TestType",
            ExecutionStage = ExecutionStage.PreValidation,
            EventOperation = "Create",
            LogicalName = "account",
            Deployment = Deployment.ServerOnly,
            ExecutionMode = ExecutionMode.Synchronous,
            ExecutionOrder = 1,
            FilteredAttributes = "name,description",
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };
        
        var remoteStep = new Step {
            Name = "TestStep", // Same name
            PluginTypeName = "TestType",
            ExecutionStage =  ExecutionStage.PreValidation,
            EventOperation = "Create",
            LogicalName = "account",
            Deployment = Deployment.ServerOnly,
            ExecutionMode = ExecutionMode.Synchronous,
            ExecutionOrder = 2, // Different execution order
            FilteredAttributes = "name,description,subject", // Different filtered attributes
            UserContext = Guid.NewGuid(), // Different user context
            AsyncAutoDelete = true,
            PluginImages = []
        };
        
        var localData = new CompiledData([], [localStep], [], [], [], []);
        var remoteData = new CompiledData([], [remoteStep], [], [], [], []);

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);

        // Assert
        Assert.Empty(differences.PluginSteps.Creates);
        Assert.Empty(differences.PluginSteps.Deletes);
        Assert.Single(differences.PluginSteps.UpdatesWithDifferences);

        var update = differences.PluginSteps.UpdatesWithDifferences[0];
        Assert.Equal("TestStep", update.LocalEntity.Name);
        Assert.Equal("TestStep", update.RemoteEntity?.Name);
        Assert.NotEmpty(update.DifferentProperties); // Should have multiple different properties

        // Verify that the differences are detected
        var funcs = update.DifferentProperties.Select(p => p.Compile()).ToArray();
        Assert.Equal(4, funcs.Length);
        var propNames = update.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
        Assert.Equal([
            nameof(Step.AsyncAutoDelete),
            nameof(Step.ExecutionOrder),
            nameof(Step.FilteredAttributes),
            nameof(Step.UserContext)
        ], propNames);

        var orderComp = funcs[0];
        var filteredAttributesComp = funcs[1];
        var userContextComp = funcs[2];
        var asyncComp = funcs[3];

        Assert.NotNull(update.RemoteEntity); // Remote entity should not be null
        Assert.Equal(1, orderComp(update.LocalEntity)); // ExecutionOrder
        Assert.Equal(2, orderComp(update.RemoteEntity)); // Remote ExecutionOrder
        Assert.Equal("name,description", filteredAttributesComp(update.LocalEntity)); // FilteredAttributes
        Assert.Equal("name,description,subject", filteredAttributesComp(update.RemoteEntity)); // Remote FilteredAttributes
        Assert.Equal(localStep.UserContext, userContextComp(update.LocalEntity));
        Assert.Equal(remoteStep.UserContext, userContextComp(update.RemoteEntity));
        Assert.False((bool)asyncComp(update.LocalEntity));
        Assert.True((bool)asyncComp(update.RemoteEntity));
    }

    [Fact]
    public void CalculateDifference_UnchangableUpdatesDetected_RequireRecreate()
    {
        var localCustomApi = new CustomApiDefinition
        {
            Name = "test_custom_api",
            PluginTypeName = "TestPluginType",
            UniqueName = "new_test_custom_api",
            IsFunction = false,
            EnabledForWorkflow = true,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.SyncAndAsync,
            BindingType = BindingType.Entity,
            BoundEntityLogicalName = "account",
            IsCustomizable = true,
            OwnerId = Guid.NewGuid(),
            IsPrivate = false,
            ExecutePrivilegeName = "new_execute_privilege",
            Description = "Test Custom API",
            DisplayName = "Test Custom API"
        };

        var remoteCustomApi = new CustomApiDefinition
        {
            Id = Guid.NewGuid(),
            Name = "test_custom_api",
            PluginTypeName = "TestPluginType",
            UniqueName = "new_test_custom_api",
            IsFunction = true,
            EnabledForWorkflow = false,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.AsyncOnly,
            BindingType = BindingType.EntityCollection,
            BoundEntityLogicalName = "contact",
            IsCustomizable = false,
            OwnerId = Guid.NewGuid(),
            IsPrivate = false,
            ExecutePrivilegeName = "new_execute_privilege",
            Description = "Test Custom API",
            DisplayName = "Test Custom API"
        };

        var localData = new CompiledData([], [], [], [localCustomApi], [], []);
        var remoteData = new CompiledData([], [], [], [remoteCustomApi], [], []);

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);

        // Assert
        Assert.Equal([localCustomApi], differences.CustomApis.Creates);
        Assert.Equal([remoteCustomApi], differences.CustomApis.Deletes);
        Assert.Empty(differences.CustomApis.UpdatesWithDifferences);

        var recreates = differences.CustomApis.Recreates;
        Assert.Single(recreates);
        var recreate = recreates[0];
        Assert.Equal(localCustomApi, recreate.LocalEntity);
        Assert.Equal(remoteCustomApi, recreate.RemoteEntity);

        var funcs = recreate.DifferentProperties.Select(p => p.Compile()).ToArray();
        var propNames = recreate.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
        Assert.Equal([
            nameof(CustomApiDefinition.AllowedCustomProcessingStepType),
            nameof(CustomApiDefinition.BindingType),
            nameof(CustomApiDefinition.BoundEntityLogicalName),
            nameof(CustomApiDefinition.EnabledForWorkflow),
            nameof(CustomApiDefinition.IsCustomizable),
            nameof(CustomApiDefinition.IsFunction)
            ], propNames);
    }

    [Fact]
    public void CalculateDifference_UnchangableUpdatesAndChangableDetected_RequiresRecreate()
    {
        var localCustomApi = new CustomApiDefinition
        {
            Name = "test_custom_api",
            PluginTypeName = "TestPluginType",
            UniqueName = "new_test_custom_api",
            IsFunction = false,
            EnabledForWorkflow = true,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.SyncAndAsync,
            BindingType = BindingType.Entity,
            BoundEntityLogicalName = "account",
            IsCustomizable = true,
            OwnerId = Guid.NewGuid(),
            IsPrivate = false,
            ExecutePrivilegeName = "new_execute_privilege",
            Description = "Test Custom API",
            DisplayName = "Test Custom API"
        };

        var remoteCustomApi = new CustomApiDefinition
        {
            Id = Guid.NewGuid(),
            Name = "test_custom_api",
            PluginTypeName = "TestPluginType",
            UniqueName = "new_test_custom_api",
            IsFunction = true, // RECREATE
            EnabledForWorkflow = true,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.SyncAndAsync,
            BindingType = BindingType.Entity,
            BoundEntityLogicalName = "account",
            IsCustomizable = true,
            OwnerId = Guid.NewGuid(),
            IsPrivate = true, // UPDATE
            ExecutePrivilegeName = "new_execute_privilege",
            Description = "Test Custom API",
            DisplayName = "Test Custom API"
        };

        var remoteCustomApiTwo = new CustomApiDefinition
        {
            Name = "test_custom_api",
            PluginTypeName = "TestPluginType",
            UniqueName = "new_test_custom_api",
            IsFunction = false,
            EnabledForWorkflow = true,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.SyncAndAsync,
            BindingType = BindingType.Entity,
            BoundEntityLogicalName = "account",
            IsCustomizable = true,
            OwnerId = Guid.NewGuid(),
            IsPrivate = true, // UPDATE
            ExecutePrivilegeName = "new_execute_privilege",
            Description = "Test Custom API",
            DisplayName = "Test Custom API"
        };

        var localData = new CompiledData([], [], [], [localCustomApi], [], []);
        var remoteData = new CompiledData([], [], [], [remoteCustomApi], [], []);
        var remoteDataTwo = new CompiledData([], [], [], [remoteCustomApiTwo], [], []);

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);
        var differencesTwo = _differenceUtility.CalculateDifferences(localData, remoteDataTwo);

        // Assert
        
        // Local -> Remote, Recreate, no update
        Assert.Equal([localCustomApi], differences.CustomApis.Creates);
        Assert.Equal([remoteCustomApi], differences.CustomApis.Deletes);
        Assert.Empty(differences.CustomApis.UpdatesWithDifferences);
        Assert.Single(differences.CustomApis.Recreates);

        Assert.Equal(localCustomApi, differences.CustomApis.Recreates[0].LocalEntity);
        Assert.Equal(remoteCustomApi, differences.CustomApis.Recreates[0].RemoteEntity);

        var propNames = differences.CustomApis.Recreates[0].DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
        Assert.Equal([
            nameof(CustomApiDefinition.IsFunction),
            nameof(CustomApiDefinition.IsPrivate)
        ], propNames);

        // Local -> RemoteTwo, Update, no recreate
        Assert.Empty(differencesTwo.CustomApis.Creates);
        Assert.Empty(differencesTwo.CustomApis.Deletes);
        Assert.Single(differencesTwo.CustomApis.UpdatesWithDifferences);
        Assert.Empty(differencesTwo.CustomApis.Recreates);

        Assert.Equal(localCustomApi, differencesTwo.CustomApis.UpdatesWithDifferences[0].LocalEntity);
        Assert.Equal(remoteCustomApiTwo, differencesTwo.CustomApis.UpdatesWithDifferences[0].RemoteEntity);
        propNames = differencesTwo.CustomApis.UpdatesWithDifferences[0]
            .DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
        Assert.Equal([
            nameof(CustomApiDefinition.IsPrivate)
        ], propNames);
    }
}
