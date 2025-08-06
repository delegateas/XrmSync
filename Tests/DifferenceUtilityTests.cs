using Microsoft.Extensions.Logging;
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
            ExecutionStage = 10,
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
            ExecutionStage = 10,
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
            ExecutionStage = 10, // PreValidation
            EventOperation = "Create",
            LogicalName = "account",
            Deployment = 0,
            ExecutionMode = 0, // Synchronous
            ExecutionOrder = 1,
            FilteredAttributes = "name,description",
            UserContext = Guid.NewGuid(),
            AsyncAutoDelete = false,
            PluginImages = []
        };
        
        var remoteStep = new Step {
            Name = "TestStep", // Same name
            PluginTypeName = "TestType",
            ExecutionStage = 20, // Pre - Different execution stage
            EventOperation = "Create",
            LogicalName = "account",
            Deployment = 0,
            ExecutionMode = 1, // Asynchronous - Different execution mode
            ExecutionOrder = 2, // Different execution order
            FilteredAttributes = "name,description,subject", // Different filtered attributes
            UserContext = Guid.NewGuid(), // Different user context
            AsyncAutoDelete = false,
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

        // Verify that the differences are detected (ExecutionStage, ExecutionMode, ExecutionOrder, FilteredAttributes, UserContext)
        var differentProps = update.DifferentProperties.ToArray();
        var func = update.DifferentProperties.Select(p => p.Compile()).ToArray();
        Assert.Equal(5, func.Length);
        var propNames = update.DifferentProperties.Select(p => p.GetMemberName()).ToArray();
        Assert.Contains(propNames, p => p == "ExecutionStage");
        Assert.Contains(propNames, p => p == "ExecutionMode");
        Assert.Contains(propNames, p => p == "ExecutionOrder");
        Assert.Contains(propNames, p => p == "FilteredAttributes");
        Assert.Contains(propNames, p => p == "UserContext");

        var stageComp = func[0];
        var modeComp = func[1];
        var orderComp = func[2];
        var filteredAttributesComp = func[3];
        var userContextComp = func[4];

        Assert.NotNull(update.RemoteEntity); // Remote entity should not be null
        Assert.Equal(10, stageComp(update.LocalEntity)); // ExecutionStage
        Assert.Equal(20, stageComp(update.RemoteEntity)); // Remote ExecutionStage
        Assert.Equal(0, modeComp(update.LocalEntity)); // ExecutionMode
        Assert.Equal(1, modeComp(update.RemoteEntity)); // Remote ExecutionMode
        Assert.Equal(1, orderComp(update.LocalEntity)); // ExecutionOrder
        Assert.Equal(2, orderComp(update.RemoteEntity)); // Remote ExecutionOrder
        Assert.Equal("name,description", filteredAttributesComp(update.LocalEntity)); // FilteredAttributes
        Assert.Equal("name,description,subject", filteredAttributesComp(update.RemoteEntity)); // Remote FilteredAttributes
    }
}
