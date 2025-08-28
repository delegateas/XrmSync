using DG.XrmPluginCore.Enums;
using Microsoft.Extensions.Logging;
using XrmSync.Model;
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
            new PluginDefinitionComparer(),
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
        var localImage = new Image
        {
            Name = "LocalImage",
            ImageType = ImageType.PreImage,
            Attributes = "",
            EntityAlias = "account"
        };

        var localStep = new Step {
            Name = "LocalStep",
            ExecutionStage = ExecutionStage.PreValidation,
            EventOperation = "Create",
            LogicalName = "account",
            Deployment = 0,
            ExecutionMode = 0,
            ExecutionOrder = 1,
            FilteredAttributes = "",
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = [ localImage ]
        };
        var localType = new PluginDefinition { Name = "LocalType", PluginSteps = [ localStep ] };

        var remoteImage = localImage with
        {
            Id = Guid.NewGuid(),
            Name = "RemoteImage"
        };
        
        var remoteStep = localStep with
        {
            Id = Guid.NewGuid(),
            Name = "RemoteStep",
            PluginImages = [ remoteImage ]
        };
        var remoteType = new PluginDefinition { Name = "RemoteType", Id = Guid.NewGuid(), PluginSteps = [ remoteStep ] };
        var sharedType = new PluginDefinition { Name = "SharedType", Id = Guid.NewGuid(), PluginSteps = [] };

        var localData = new AssemblyInfo
        {
            Id = Guid.Empty,
            DllPath = "local.dll",
            Hash = Guid.NewGuid().ToString(),
            Name = "LocalAssembly",
            Version = "1.0.0",
            CustomApis = [],
            Plugins = [localType, sharedType]
        };

        var remoteData = localData with
        {
            Plugins = [remoteType, sharedType]
        };

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);

        // Assert
        Assert.Single(differences.Types.Creates);
        Assert.Equal("LocalType", differences.Types.Creates[0].Local.Name);
        Assert.Single(differences.Types.Deletes);
        Assert.Equal("RemoteType", differences.Types.Deletes[0].Name);
        Assert.Empty(differences.Types.Updates);
        
        Assert.Single(differences.PluginSteps.Creates);
        Assert.Equal("LocalStep", differences.PluginSteps.Creates[0].Local.Entity.Name);
        Assert.Single(differences.PluginSteps.Deletes);
        Assert.Equal("RemoteStep", differences.PluginSteps.Deletes[0].Entity.Name);
        Assert.Empty(differences.PluginSteps.Updates);

        Assert.Single(differences.PluginImages.Creates);
        Assert.Equal("LocalImage", differences.PluginImages.Creates[0].Local.Entity.Name);
        Assert.Single(differences.PluginImages.Deletes);
        Assert.Equal("RemoteImage", differences.PluginImages.Deletes[0].Entity.Name);
        Assert.Empty(differences.PluginImages.Updates);
    }

    [Fact]
    public void CalculateDifferences_EmptyData_ReturnsEmptyDifferences()
    {
        // Arrange
        var emptyData = new AssemblyInfo {
            Id = Guid.Empty,
            DllPath = string.Empty,
            Hash = string.Empty,
            Name = string.Empty,
            Version = string.Empty,
            CustomApis = [],
            Plugins = []
        };

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
            Id = Guid.NewGuid(),
            Name = "TestStep",
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
        var localType = new PluginDefinition { Name = "LocalType", Id = Guid.NewGuid(), PluginSteps = [ localStep ] };

        var remoteStep = localStep with
        {
            ExecutionOrder = 2, // Different execution order
            FilteredAttributes = "name,description,subject", // Different filtered attributes
            UserContext = Guid.NewGuid(), // Different user context
            AsyncAutoDelete = true // Different async auto delete
        };
        var remoteType = localType with { Name = "RemoteType", PluginSteps = [ remoteStep ] };

        var localData = new AssemblyInfo
        {
            Id = Guid.Empty,
            DllPath = "local.dll",
            Hash = Guid.NewGuid().ToString(),
            Name = "LocalAssembly",
            Version = "1.0.0",
            CustomApis = [],
            Plugins = [localType]
        };

        var remoteData = localData with
        {
            Plugins = [remoteType]
        };

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);

        // Assert
        Assert.Empty(differences.PluginSteps.Creates);
        Assert.Empty(differences.PluginSteps.Deletes);
        Assert.Single(differences.PluginSteps.Updates);

        var update = differences.PluginSteps.Updates[0];
        Assert.Equal(localStep, update.Local.Entity);
        Assert.Equal(localType, update.Local.Parent);
        Assert.Equal(remoteStep, update.Remote?.Entity);
        Assert.Equal(remoteType, update.Remote?.Parent);
        Assert.Equal("TestStep", update.Local.Entity.Name);
        Assert.Equal("TestStep", update.Remote?.Entity.Name);
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

        Assert.NotNull(update.Remote); // Remote entity should not be null
        Assert.Equal(1, orderComp(update.Local.Entity)); // ExecutionOrder
        Assert.Equal(2, orderComp(update.Remote.Entity)); // Remote ExecutionOrder
        Assert.Equal("name,description", filteredAttributesComp(update.Local.Entity)); // FilteredAttributes
        Assert.Equal("name,description,subject", filteredAttributesComp(update.Remote.Entity)); // Remote FilteredAttributes
        Assert.Equal(localStep.UserContext, userContextComp(update.Local.Entity));
        Assert.Equal(remoteStep.UserContext, userContextComp(update.Remote.Entity));
        Assert.False((bool)asyncComp(update.Local.Entity));
        Assert.True((bool)asyncComp(update.Remote.Entity));
    }

    [Fact]
    public void CalculateDifference_UnchangableUpdatesDetected_RequireRecreate()
    {
        var localCustomApi = new CustomApiDefinition
        {
            Id = Guid.NewGuid(),
            Name = "test_custom_api",
            PluginType = new PluginType { Id = Guid.NewGuid(), Name = "Type1" },
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

        var remoteCustomApi = localCustomApi with
        {
            PluginType = localCustomApi.PluginType with { },
            EnabledForWorkflow = false,
            AllowedCustomProcessingStepType = AllowedCustomProcessingStepType.AsyncOnly,
            BindingType = BindingType.EntityCollection,
            BoundEntityLogicalName = "contact",
            IsCustomizable = false,
            IsFunction = true
        };

        var localData = new AssemblyInfo
        {
            Id = Guid.Empty,
            DllPath = "local.dll",
            Hash = Guid.NewGuid().ToString(),
            Name = "LocalAssembly",
            Version = "1.0.0",
            CustomApis = [localCustomApi],
            Plugins = []
        };

        var remoteData = localData with
        {
            CustomApis = [remoteCustomApi]
        };

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);

        // Assert
        Assert.Equal([remoteCustomApi], differences.CustomApis.Deletes);
        Assert.Empty(differences.CustomApis.Updates);

        var creates = differences.CustomApis.Creates;
        Assert.Single(creates);
        var create = creates[0];
        Assert.Equal(localCustomApi, create.Local);
        Assert.Equal(remoteCustomApi, create.Remote);

        var funcs = create.DifferentProperties.Select(p => p.Compile()).ToArray();
        var propNames = create.DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
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
            Id = Guid.NewGuid(),
            Name = "test_custom_api",
            PluginType = new PluginType { Id = Guid.NewGuid(), Name = "Type" },
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

        var remoteCustomApi = localCustomApi with
        {
            PluginType = localCustomApi.PluginType with { },
            IsFunction = true, // RECREATE
            IsPrivate = true // UPDATE
        };

        var remoteCustomApiTwo = localCustomApi with
        {
            PluginType = localCustomApi.PluginType with { },
            IsPrivate = true // UPDATE
        };

        var localData = new AssemblyInfo
        {
            Id = Guid.Empty,
            DllPath = "local.dll",
            Hash = Guid.NewGuid().ToString(),
            Name = "LocalAssembly",
            Version = "1.0.0",
            CustomApis = [localCustomApi],
            Plugins = []
        };

        var remoteData = localData with
        {
            CustomApis = [remoteCustomApi]
        };

        var remoteDataTwo = localData with
        {
            CustomApis = [remoteCustomApiTwo]
        };

        // Act
        var differences = _differenceUtility.CalculateDifferences(localData, remoteData);
        var differencesTwo = _differenceUtility.CalculateDifferences(localData, remoteDataTwo);

        // Assert
        
        // Local -> Remote, Recreate, no update
        Assert.Single(differences.CustomApis.Creates);
        Assert.Equal([remoteCustomApi], differences.CustomApis.Deletes);
        Assert.Empty(differences.CustomApis.Updates);

        Assert.Equal(localCustomApi, differences.CustomApis.Creates[0].Local);
        Assert.Equal(remoteCustomApi, differences.CustomApis.Creates[0].Remote);

        var propNames = differences.CustomApis.Creates[0].DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
        Assert.Equal([
            nameof(CustomApiDefinition.IsFunction),
            nameof(CustomApiDefinition.IsPrivate)
        ], propNames);

        // Local -> RemoteTwo, Update, no recreate
        Assert.Empty(differencesTwo.CustomApis.Creates);
        Assert.Empty(differencesTwo.CustomApis.Deletes);
        Assert.Single(differencesTwo.CustomApis.Updates);

        Assert.Equal(localCustomApi, differencesTwo.CustomApis.Updates[0].Local);
        Assert.Equal(remoteCustomApiTwo, differencesTwo.CustomApis.Updates[0].Remote);
        propNames = differencesTwo.CustomApis.Updates[0]
            .DifferentProperties.Select(p => p.GetMemberName()).Order().ToArray();
        Assert.Equal([
            nameof(CustomApiDefinition.IsPrivate)
        ], propNames);
    }
}
