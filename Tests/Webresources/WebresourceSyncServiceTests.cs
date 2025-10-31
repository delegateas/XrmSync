using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using XrmSync.Analyzer.Reader;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Webresource;
using XrmSync.SyncService;
using XrmSync.SyncService.Difference;
using XrmSync.SyncService.Validation;
using XrmSync.SyncService.Validation.Webresource;

namespace Tests.Webresources;

public class WebresourceSyncServiceTests
{
    private readonly ILogger<WebresourceSyncService> _logger = Substitute.For<ILogger<WebresourceSyncService>>();
    private readonly ILocalReader _localReader = Substitute.For<ILocalReader>();
    private readonly ISolutionReader _solutionReader = Substitute.For<ISolutionReader>();
    private readonly IWebresourceReader _webresourceReader = Substitute.For<IWebresourceReader>();
    private readonly IWebresourceWriter _webresourceWriter = Substitute.For<IWebresourceWriter>();
    private readonly IPrintService _printService = Substitute.For<IPrintService>();
    private readonly IValidator<WebresourceDefinition> _webresourceValidator = Substitute.For<IValidator<WebresourceDefinition>>();
    private readonly WebresourceSyncOptions _options = new("C:\\WebResources", "TestSolution");

    private readonly WebresourceSyncService _service;

    public WebresourceSyncServiceTests()
    {
        _service = new WebresourceSyncService(
            Options.Create(_options),
            _logger,
            _localReader,
            _solutionReader,
            _webresourceReader,
            _webresourceWriter,
            _webresourceValidator,
            _printService
        );
    }

    [Fact]
    public async Task Sync_CreatesNewWebresources_WhenOnlyExistLocally()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        var localWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/test.js", "Test Script", WebresourceType.ScriptJscript, "Y29uc29sZS5sb2coJ3Rlc3QnKTs=")
        };
        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(localWebresources);

        _webresourceReader.GetWebresources(solutionId).Returns(new List<WebresourceDefinition>());

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(
            list => list.Count() == 1 && list.First().Name == "test_TestSolution/test.js"));
        _webresourceWriter.Received(1).Delete(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Update(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
    }

    [Fact]
    public async Task Sync_DeletesWebresources_WhenOnlyExistRemotely()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(new List<WebresourceDefinition>());

        var remoteWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/old.js", "Old Script", WebresourceType.ScriptJscript, "b2xkQ29kZQ==") { Id = Guid.NewGuid() }
        };
        _webresourceReader.GetWebresources(solutionId).Returns(remoteWebresources);

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Delete(Arg.Is<IEnumerable<WebresourceDefinition>>(
            list => list.Count() == 1 && list.First().Name == "test_TestSolution/old.js"));
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Update(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
    }

    [Fact]
    public async Task Sync_UpdatesWebresources_WhenContentDiffers()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        var webresourceId = Guid.NewGuid();
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        var localWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/test.js", "Test Script", WebresourceType.ScriptJscript, "bmV3Q29kZQ==")
        };
        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(localWebresources);

        var remoteWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/test.js", "Test Script", WebresourceType.ScriptJscript, "b2xkQ29kZQ==") { Id = webresourceId }
        };
        _webresourceReader.GetWebresources(solutionId).Returns(remoteWebresources);

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Update(Arg.Is<IEnumerable<WebresourceDefinition>>(
            list => list.Count() == 1 
                && list.First().Name == "test_TestSolution/test.js" 
                && list.First().Id == webresourceId
                && list.First().Content == "bmV3Q29kZQ=="));
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Delete(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
    }

    [Fact]
    public async Task Sync_UpdatesWebresources_WhenDisplayNameDiffers()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        var webresourceId = Guid.NewGuid();
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        var localWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/test.js", "Updated Display Name", WebresourceType.ScriptJscript, "c2FtZUNvZGU=")
        };
        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(localWebresources);

        var remoteWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/test.js", "Old Display Name", WebresourceType.ScriptJscript, "c2FtZUNvZGU=") { Id = webresourceId }
        };
        _webresourceReader.GetWebresources(solutionId).Returns(remoteWebresources);

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Update(Arg.Is<IEnumerable<WebresourceDefinition>>(
            list => list.Count() == 1 
                && list.First().DisplayName == "Updated Display Name"
                && list.First().Id == webresourceId));
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Delete(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
    }

    [Fact]
    public async Task Sync_DoesNotUpdate_WhenWebresourcesAreIdentical()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        var webresourceId = Guid.NewGuid();
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        var localWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/test.js", "Test Script", WebresourceType.ScriptJscript, "c2FtZUNvZGU=")
        };
        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(localWebresources);

        var remoteWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/test.js", "Test Script", WebresourceType.ScriptJscript, "c2FtZUNvZGU=") { Id = webresourceId }
        };
        _webresourceReader.GetWebresources(solutionId).Returns(remoteWebresources);

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Update(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Delete(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
    }

    [Fact]
    public async Task Sync_HandlesMultipleOperations_Simultaneously()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        var existingId = Guid.NewGuid();
        var toUpdateId = Guid.NewGuid();
        var toDeleteId = Guid.NewGuid();
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        var localWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/new.js", "New Script", WebresourceType.ScriptJscript, "bmV3"),
            new("test_TestSolution/existing.js", "Existing", WebresourceType.ScriptJscript, "ZXhpc3Rpbmc="),
            new("test_TestSolution/update.js", "Updated", WebresourceType.ScriptJscript, "dXBkYXRlZA==")
        };
        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(localWebresources);

        var remoteWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/existing.js", "Existing", WebresourceType.ScriptJscript, "ZXhpc3Rpbmc=") { Id = existingId },
            new("test_TestSolution/update.js", "Updated", WebresourceType.ScriptJscript, "b2xk") { Id = toUpdateId },
            new("test_TestSolution/delete.js", "To Delete", WebresourceType.ScriptJscript, "ZGVsZXRl") { Id = toDeleteId }
        };
        _webresourceReader.GetWebresources(solutionId).Returns(remoteWebresources);

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(
            list => list.Count() == 1 && list.First().Name == "test_TestSolution/new.js"));
        _webresourceWriter.Received(1).Update(Arg.Is<IEnumerable<WebresourceDefinition>>(
            list => list.Count() == 1 && list.First().Name == "test_TestSolution/update.js"));
        _webresourceWriter.Received(1).Delete(Arg.Is<IEnumerable<WebresourceDefinition>>(
            list => list.Count() == 1 && list.First().Name == "test_TestSolution/delete.js"));
    }

    [Fact]
    public async Task Sync_IsCaseInsensitive_ForNameMatching()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        var webresourceId = Guid.NewGuid();
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        var localWebresources = new List<WebresourceDefinition>
        {
            new("test_testsolution/test.js", "Test Script", WebresourceType.ScriptJscript, "dGVzdA==")
        };
        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(localWebresources);

        var remoteWebresources = new List<WebresourceDefinition>
        {
            new("TEST_TESTSOLUTION/TEST.JS", "Test Script", WebresourceType.ScriptJscript, "dGVzdA==") { Id = webresourceId }
        };
        _webresourceReader.GetWebresources(solutionId).Returns(remoteWebresources);

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Delete(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
        _webresourceWriter.Received(1).Update(Arg.Is<IEnumerable<WebresourceDefinition>>(list => !list.Any()));
    }

    [Fact]
    public async Task Sync_HandlesMultipleWebresourceTypes()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));

        var localWebresources = new List<WebresourceDefinition>
        {
            new("test_TestSolution/script.js", "Script", WebresourceType.ScriptJscript, "anM="),
            new("test_TestSolution/style.css", "Style", WebresourceType.StyleSheetCss, "Y3Nz"),
            new("test_TestSolution/page.html", "Page", WebresourceType.WebpageHtml, "aHRtbA=="),
            new("test_TestSolution/image.png", "Image", WebresourceType.PngFormat, "cG5n"),
            new("test_TestSolution/data.xml", "Data", WebresourceType.DataXml, "eG1s")
        };
        _localReader.ReadWebResourceFolder(_options.FolderPath, $"{solutionPrefix}_{_options.SolutionName}")
            .Returns(localWebresources);

        _webresourceReader.GetWebresources(solutionId).Returns(new List<WebresourceDefinition>());

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _webresourceWriter.Received(1).Create(Arg.Is<IEnumerable<WebresourceDefinition>>(list => list.Count() == 5));
    }

    [Fact]
    public async Task Sync_CallsPrintService()
    {
        // Arrange
        var solutionId = Guid.NewGuid();
        var solutionPrefix = "test";
        _solutionReader.RetrieveSolution(_options.SolutionName).Returns((solutionId, solutionPrefix));
        _localReader.ReadWebResourceFolder(Arg.Any<string>(), Arg.Any<string>()).Returns(new List<WebresourceDefinition>());
        _webresourceReader.GetWebresources(solutionId).Returns(new List<WebresourceDefinition>());

        // Act
        await _service.Sync(CancellationToken.None);

        // Assert
        _printService.Received(1).PrintHeader(Arg.Any<PrintHeaderOptions>());
    }
}
