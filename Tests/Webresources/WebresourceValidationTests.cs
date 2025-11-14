using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.Extensions;
using XrmSync.SyncService.Validation;
using XrmSync.SyncService.Validation.Webresource;
using XrmSync.SyncService.Validation.Webresource.Rules;

namespace Tests.Webresources;

public class WebresourceValidationTests
{
    private static IValidator<WebresourceDefinition> CreateValidator(IWebresourceReader? webresourceReader = null)
    {
        var services = new ServiceCollection();

        // Register validation rules using the extension method
        services.AddValidationRules();

        // Register mock or provided webresource reader
        var mockWebresourceReader = webresourceReader ?? Substitute.For<IWebresourceReader>();

        services.AddSingleton(mockWebresourceReader);

        // Register the validator
        services.AddSingleton<IValidator<WebresourceDefinition>, WebresourceValidator>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IValidator<WebresourceDefinition>>();
    }

    [Fact]
    public void ValidationRulesAreDiscoveredCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddValidationRules();

        // Register mock webresource reader
        services.AddSingleton(Substitute.For<IWebresourceReader>());

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Verify WebresourceDefinition validation rules are registered
        var webresourceRules = serviceProvider.GetServices<IValidationRule<WebresourceDefinition>>().ToList();
        Assert.NotEmpty(webresourceRules);
        Assert.Contains(webresourceRules, r => r is WebresourceDependencyRule);
        Assert.Contains(webresourceRules, r => r is WebresourceNameConflictRule);
    }

    [Fact]
    public void ValidateWebresourcesThrowsExceptionForWebresourceWithDependencies()
    {
        // Arrange
        var webresourceWithDependency = new WebresourceDefinition(
            "test_solution/js/script.js",
            "Test Script",
            WebresourceType.JS,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresourcesWithoutDependency = new WebresourceDefinition(
            "test_solution/css/style.css",
            "Test Style",
            WebresourceType.CSS,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresourcesToDelete = new List<WebresourceDefinition>
        {
            webresourceWithDependency,
            webresourcesWithoutDependency
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([new (webresourceWithDependency, "SystemForm", Guid.NewGuid())]); // Only the first one has dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)); // No name conflicts

        var validator = CreateValidator(webresourceReader);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow(webresourcesToDelete));
        Assert.Contains("Cannot delete webresource", exception.Message);
        Assert.Contains("test_solution/js/script.js", exception.Message);
        Assert.Contains("SystemForm with ID", exception.Message);
    }

    [Fact]
    public void ValidateWebresourcesDoesNotThrowForWebresourceWithoutDependencies()
    {
        // Arrange
        var webresource1 = new WebresourceDefinition(
            "test_solution/js/script.js",
            "Test Script",
            WebresourceType.JS,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresource2 = new WebresourceDefinition(
            "test_solution/css/style.css",
            "Test Style",
            WebresourceType.CSS,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresourcesToDelete = new List<WebresourceDefinition>
        {
            webresource1,
            webresource2
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([]); // No webresources have dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)); // No name conflicts

        var validator = CreateValidator(webresourceReader);

        // Act & Assert - Should not throw
        validator.ValidateOrThrow(webresourcesToDelete);
    }

    [Fact]
    public void ValidateWebresourcesThrowsAggregateExceptionForMultipleWebresourcesWithDependencies()
    {
        // Arrange
        var webresource1 = new WebresourceDefinition(
            "test_solution/js/script1.js",
            "Test Script 1",
            WebresourceType.JS,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresource2 = new WebresourceDefinition(
            "test_solution/js/script2.js",
            "Test Script 2",
            WebresourceType.JS,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresourcesToDelete = new List<WebresourceDefinition>
        {
            webresource1,
            webresource2
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([
                new (webresource1, "SystemForm", Guid.NewGuid()),
                new (webresource2, "SystemForm", Guid.NewGuid())
            ]); // Both have dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)); // No name conflicts

        var validator = CreateValidator(webresourceReader);

        // Act & Assert
        var exception = Assert.Throws<AggregateException>(() => validator.ValidateOrThrow(webresourcesToDelete));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.All(exception.InnerExceptions, ex =>
        {
            Assert.IsType<ValidationException>(ex);
            Assert.Contains("Cannot delete webresource", ex.Message);
        });
    }

    [Fact]
    public void ValidateWebresourcesThrowsExceptionForNameConflict()
    {
        // Arrange
        var existingWebresourceId = Guid.NewGuid();
        var webresourceToCreate = new WebresourceDefinition(
            "test_solution/js/existing.js",
            "Existing Script",
            WebresourceType.JS,
            "Y29udGVudA=="
        );

        var webresourcesToCreate = new List<WebresourceDefinition>
        {
            webresourceToCreate
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([]); // No dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
            {
                { "test_solution/js/existing.js", existingWebresourceId }
            });

        var validator = CreateValidator(webresourceReader);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow(webresourcesToCreate));
        Assert.Contains("Cannot create webresource", exception.Message);
        Assert.Contains("test_solution/js/existing.js", exception.Message);
        Assert.Contains(existingWebresourceId.ToString(), exception.Message);
    }

    [Fact]
    public void ValidateWebresourcesDoesNotThrowForNoNameConflict()
    {
        // Arrange
        var webresource1 = new WebresourceDefinition(
            "test_solution/js/new.js",
            "New Script",
            WebresourceType.JS,
            "Y29udGVudA=="
        );

        var webresource2 = new WebresourceDefinition(
            "test_solution/css/new.css",
            "New Style",
            WebresourceType.CSS,
            "Y29udGVudA=="
        );

        var webresourcesToCreate = new List<WebresourceDefinition>
        {
            webresource1,
            webresource2
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([]); // No dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)); // Empty - no conflicts

        var validator = CreateValidator(webresourceReader);

        // Act & Assert - Should not throw
        validator.ValidateOrThrow(webresourcesToCreate);
    }

    [Fact]
    public void ValidateWebresourcesThrowsAggregateExceptionForMultipleNameConflicts()
    {
        // Arrange
        var existingId1 = Guid.NewGuid();
        var existingId2 = Guid.NewGuid();

        var webresource1 = new WebresourceDefinition(
            "test_solution/js/conflict1.js",
            "Conflict 1",
            WebresourceType.JS,
            "Y29udGVudA=="
        );

        var webresource2 = new WebresourceDefinition(
            "test_solution/js/conflict2.js",
            "Conflict 2",
            WebresourceType.JS,
            "Y29udGVudA=="
        );

        var webresourcesToCreate = new List<WebresourceDefinition>
        {
            webresource1,
            webresource2
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([]); // No dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
            {
                { "test_solution/js/conflict1.js", existingId1 },
                { "test_solution/js/conflict2.js", existingId2 }
            });

        var validator = CreateValidator(webresourceReader);

        // Act & Assert
        var exception = Assert.Throws<AggregateException>(() => validator.ValidateOrThrow(webresourcesToCreate));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.All(exception.InnerExceptions, ex =>
        {
            Assert.IsType<ValidationException>(ex);
            Assert.Contains("Cannot create webresource", ex.Message);
        });
    }

    [Fact]
    public void ValidateWebresourcesNameConflictIsCaseInsensitive()
    {
        // Arrange
        var existingWebresourceId = Guid.NewGuid();
        var webresourceToCreate = new WebresourceDefinition(
            "test_solution/js/SCRIPT.JS", // Upper case
            "Script",
            WebresourceType.JS,
            "Y29udGVudA=="
        );

        var webresourcesToCreate = new List<WebresourceDefinition>
        {
            webresourceToCreate
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([]); // No dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
            {
                { "test_solution/js/script.js", existingWebresourceId } // Lower case
            });

        var validator = CreateValidator(webresourceReader);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow(webresourcesToCreate));
        Assert.Contains("Cannot create webresource", exception.Message);
    }

    [Fact]
    public void ValidateWebresourcesNameConflictRuleDoesNotApplyToDeletes()
    {
        // Arrange - Webresource with ID (simulating delete operation)
        var webresourceToDelete = new WebresourceDefinition(
            "test_solution/js/existing.js",
            "Existing Script",
            WebresourceType.JS,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid() // Has ID - it's a delete, not a create
        };

        var webresourcesToDelete = new List<WebresourceDefinition>
        {
            webresourceToDelete
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([]); // No dependencies
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
            {
                { "test_solution/js/existing.js", Guid.NewGuid() } // Name exists in environment
            });

        var validator = CreateValidator(webresourceReader);

        // Act & Assert - Should not throw because name conflict rule only applies to creates
        validator.ValidateOrThrow(webresourcesToDelete);

        // Verify GetWebresourcesByNames was NOT called (rule filtered out items with IDs)
        webresourceReader.DidNotReceive().GetWebresourcesByNames(Arg.Any<IEnumerable<string>>());
    }

    [Fact]
    public void ValidateWebresourcesDependencyRuleDoesNotApplyToCreates()
    {
        // Arrange - Webresource without ID (simulating create operation)
        var webresourceToCreate = new WebresourceDefinition(
            "test_solution/js/new.js",
            "New Script",
            WebresourceType.JS,
            "Y29udGVudA=="
        );
        // No Id set - it's a create, not a delete

        var webresourcesToCreate = new List<WebresourceDefinition>
        {
            webresourceToCreate
        };

        var webresourceReader = Substitute.For<IWebresourceReader>();
        webresourceReader.GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>())
            .Returns([new (webresourceToCreate, "SystemForm", Guid.NewGuid())]); // Has dependency in mock
        webresourceReader.GetWebresourcesByNames(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)); // No name conflicts

        var validator = CreateValidator(webresourceReader);

        // Act & Assert - Should not throw because dependency rule only applies to deletes
        validator.ValidateOrThrow(webresourcesToCreate);

        // Verify GetWebresourcesWithDependencies was NOT called (rule filtered out items without IDs)
        webresourceReader.DidNotReceive().GetWebresourcesWithDependencies(Arg.Any<IEnumerable<WebresourceDefinition>>());
    }
}
