using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.Extensions;
using XrmSync.SyncService.PluginValidator.Rules;
using XrmSync.SyncService.WebresourceValidator;
using XrmSync.SyncService.WebresourceValidator.Rules;

namespace Tests.Webresources;

public class WebresourceValidationTests
{
    private static IWebresourceValidator CreateValidator(IWebresourceReader? webresourceReader = null)
    {
        var services = new ServiceCollection();

        // Register validation rules using the extension method
        services.AddValidationRules();

        // Register mock or provided webresource reader
        var mockWebresourceReader = webresourceReader ?? Substitute.For<IWebresourceReader>();
        services.AddSingleton(mockWebresourceReader);

        // Register the validator
        services.AddSingleton<IWebresourceValidator, WebresourceValidator>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IWebresourceValidator>();
    }

    [Fact]
    public void ValidationRules_AreDiscovered_Correctly()
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
    }

    [Fact]
    public void ValidateWebresources_ThrowsException_ForWebresourceWithDependencies()
    {
        // Arrange
        var webresourceWithDependency = new WebresourceDefinition(
            "test_solution/js/script.js",
            "Test Script",
            WebresourceType.ScriptJscript,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresourcesWithoutDependency = new WebresourceDefinition(
            "test_solution/css/style.css",
            "Test Style",
            WebresourceType.StyleSheetCss,
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
            .Returns([webresourceWithDependency]); // Only the first one has dependencies

        var validator = CreateValidator(webresourceReader);

        // Act & Assert
        var exception = Assert.Throws<ValidationException>(() => validator.Validate(webresourcesToDelete));
        Assert.Contains("Cannot delete webresource", exception.Message);
        Assert.Contains("test_solution/js/script.js", exception.Message);
    }

    [Fact]
    public void ValidateWebresources_DoesNotThrow_ForWebresourceWithoutDependencies()
    {
        // Arrange
        var webresource1 = new WebresourceDefinition(
            "test_solution/js/script.js",
            "Test Script",
            WebresourceType.ScriptJscript,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresource2 = new WebresourceDefinition(
            "test_solution/css/style.css",
            "Test Style",
            WebresourceType.StyleSheetCss,
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

        var validator = CreateValidator(webresourceReader);

        // Act & Assert - Should not throw
        validator.Validate(webresourcesToDelete);
    }

    [Fact]
    public void ValidateWebresources_ThrowsAggregateException_ForMultipleWebresourcesWithDependencies()
    {
        // Arrange
        var webresource1 = new WebresourceDefinition(
            "test_solution/js/script1.js",
            "Test Script 1",
            WebresourceType.ScriptJscript,
            "Y29udGVudA=="
        )
        {
            Id = Guid.NewGuid()
        };

        var webresource2 = new WebresourceDefinition(
            "test_solution/js/script2.js",
            "Test Script 2",
            WebresourceType.ScriptJscript,
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
            .Returns([webresource1, webresource2]); // Both have dependencies

        var validator = CreateValidator(webresourceReader);

        // Act & Assert
        var exception = Assert.Throws<AggregateException>(() => validator.Validate(webresourcesToDelete));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.All(exception.InnerExceptions, ex =>
        {
            Assert.IsType<ValidationException>(ex);
            Assert.Contains("Cannot delete webresource", ex.Message);
        });
    }
}
