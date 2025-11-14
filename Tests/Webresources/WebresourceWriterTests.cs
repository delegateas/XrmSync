using Microsoft.Extensions.Options;
using NSubstitute;
using XrmSync.Dataverse;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Webresource;

namespace Tests.Webresources;

public class WebresourceWriterTests
{
    private readonly IDataverseWriter _dataverseWriter = Substitute.For<IDataverseWriter>();
    private readonly WebresourceSyncOptions _options = new("C:\\WebResources", "TestSolution");
    private readonly WebresourceWriter _writer;

    public WebresourceWriterTests()
    {
        _writer = new WebresourceWriter(_dataverseWriter, Options.Create(_options));
    }

    [Fact]
    public void Create_CallsDataverseWriter_WithCorrectParameters()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>
        {
            new("test_solution/test.js", "Test Script", WebresourceType.JS, "Y29uc29sZS5sb2coJ3Rlc3QnKTs=")
        };

        // Act
        _writer.Create(webresources);

        // Assert
        _dataverseWriter.Received(1).Create(
            Arg.Is<WebResource>(wr =>
                wr.Name == "test_solution/test.js"
                && wr.DisplayName == "Test Script"
                && wr.Content == "Y29uc29sZS5sb2coJ3Rlc3QnKTs="
                && wr.WebResourceType == webresource_webresourcetype.ScriptJScript),
            Arg.Any<Dictionary<string, object>>()
        );
    }

    [Fact]
    public void Create_PassesSolutionNameInParameters()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>
        {
            new("test.js", "Test", WebresourceType.JS, "dGVzdA==")
        };

        Dictionary<string, object>? capturedParams = null;
        _dataverseWriter.Create(Arg.Any<WebResource>(), Arg.Do<Dictionary<string, object>>(x => capturedParams = x));

        // Act
        _writer.Create(webresources);

        // Assert
        Assert.NotNull(capturedParams);
        Assert.True(capturedParams.ContainsKey("SolutionUniqueName"));
        Assert.Equal("TestSolution", capturedParams["SolutionUniqueName"]);
    }

    [Fact]
    public void Create_HandlesMultipleWebresources()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>
        {
            new("test_solution/script.js", "Script", WebresourceType.JS, "anM="),
            new("test_solution/style.css", "Style", WebresourceType.CSS, "Y3Nz"),
            new("test_solution/page.html", "Page", WebresourceType.HTML, "aHRtbA==")
        };

        // Act
        _writer.Create(webresources);

        // Assert
        _dataverseWriter.Received(3).Create(Arg.Any<WebResource>(), Arg.Any<Dictionary<string, object>>());
    }

    [Fact]
    public void Create_MapsWebresourceTypeCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            (WebresourceType.HTML, webresource_webresourcetype.WebpageHTML),
            (WebresourceType.CSS, webresource_webresourcetype.StyleSheetCSS),
            (WebresourceType.JS, webresource_webresourcetype.ScriptJScript),
            (WebresourceType.XML, webresource_webresourcetype.DataXML),
            (WebresourceType.PNG, webresource_webresourcetype.PNGformat),
            (WebresourceType.JPG, webresource_webresourcetype.JPGformat),
            (WebresourceType.GIF, webresource_webresourcetype.GIFformat),
            (WebresourceType.XAP, webresource_webresourcetype.SilverlightXAP),
            (WebresourceType.XSL, webresource_webresourcetype.StyleSheetXSL),
            (WebresourceType.ICO, webresource_webresourcetype.ICOformat),
            (WebresourceType.SVG, webresource_webresourcetype.VectorformatSVG),
            (WebresourceType.RSX, webresource_webresourcetype.StringRESX)
        };

        foreach (var (modelType, entityType) in testCases)
        {
            _dataverseWriter.ClearReceivedCalls();

            var webresources = new List<WebresourceDefinition>
            {
                new("test.file", "Test", modelType, "dGVzdA==")
            };

            // Act
            _writer.Create(webresources);

            // Assert
            _dataverseWriter.Received(1).Create(
                Arg.Is<WebResource>(wr => wr.WebResourceType == entityType),
                Arg.Any<Dictionary<string, object>>()
            );
        }
    }

    [Fact]
    public void Update_CallsDataverseWriter_WithCorrectParameters()
    {
        // Arrange
        var webresourceId = Guid.NewGuid();
        var webresources = new List<WebresourceDefinition>
        {
            new("test_solution/test.js", "Updated Display Name", WebresourceType.JS, "dXBkYXRlZENvbnRlbnQ=")
            {
                Id = webresourceId
            }
        };

        // Act
        _writer.Update(webresources);

        // Assert
        _dataverseWriter.Received(1).UpdateMultiple(
            Arg.Is<IEnumerable<WebResource>>(list =>
                list.Count() == 1
                && list.First().Id == webresourceId
                && list.First().DisplayName == "Updated Display Name"
                && list.First().Content == "dXBkYXRlZENvbnRlbnQ=")
        );
    }

    [Fact]
    public void Update_HandlesMultipleWebresources()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>
        {
            new("test1.js", "Test 1", WebresourceType.JS, "dGVzdDE=") { Id = Guid.NewGuid() },
            new("test2.js", "Test 2", WebresourceType.JS, "dGVzdDI=") { Id = Guid.NewGuid() },
            new("test3.js", "Test 3", WebresourceType.JS, "dGVzdDM=") { Id = Guid.NewGuid() }
        };

        // Act
        _writer.Update(webresources);

        // Assert
        _dataverseWriter.Received(1).UpdateMultiple(
            Arg.Is<IEnumerable<WebResource>>(list => list.Count() == 3)
        );
    }

    [Fact]
    public void Update_DoesNotIncludeNameInUpdate()
    {
        // Arrange
        var webresourceId = Guid.NewGuid();
        var webresources = new List<WebresourceDefinition>
        {
            new("test_solution/test.js", "Display Name", WebresourceType.JS, "Y29udGVudA==")
            {
                Id = webresourceId
            }
        };

        // Act
        _writer.Update(webresources);

        // Assert
        _dataverseWriter.Received(1).UpdateMultiple(
            Arg.Is<IEnumerable<WebResource>>(list =>
                list.First().Id == webresourceId
                && list.First().DisplayName == "Display Name"
                && list.First().Content == "Y29udGVudA=="
                && string.IsNullOrEmpty(list.First().Name))
        );
    }

    [Fact]
    public void Delete_CallsDataverseWriter_WithCorrectParameters()
    {
        // Arrange
        var webresourceId = Guid.NewGuid();
        var webresources = new List<WebresourceDefinition>
        {
            new("test_solution/test.js", "Test Script", WebresourceType.JS, "dGVzdA==")
            {
                Id = webresourceId
            }
        };

        // Act
        _writer.Delete(webresources);

        // Assert
        _dataverseWriter.Received(1).DeleteMultiple(
            Arg.Is<IEnumerable<Microsoft.Xrm.Sdk.Messages.DeleteRequest>>(list =>
                list.Count() == 1
                && list.First().Target.LogicalName == WebResource.EntityLogicalName
                && list.First().Target.Id == webresourceId)
        );
    }

    [Fact]
    public void Delete_HandlesMultipleWebresources()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>
        {
            new("test1.js", "Test 1", WebresourceType.JS, "dGVzdDE=") { Id = Guid.NewGuid() },
            new("test2.js", "Test 2", WebresourceType.JS, "dGVzdDI=") { Id = Guid.NewGuid() },
            new("test3.js", "Test 3", WebresourceType.JS, "dGVzdDM=") { Id = Guid.NewGuid() }
        };

        // Act
        _writer.Delete(webresources);

        // Assert
        _dataverseWriter.Received(1).DeleteMultiple(
            Arg.Is<IEnumerable<Microsoft.Xrm.Sdk.Messages.DeleteRequest>>(list => list.Count() == 3)
        );
    }

    [Fact]
    public void Create_HandlesEmptyList()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>();

        // Act
        _writer.Create(webresources);

        // Assert
        _dataverseWriter.DidNotReceive().Create(Arg.Any<WebResource>(), Arg.Any<Dictionary<string, object>>());
    }

    [Fact]
    public void Update_HandlesEmptyList()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>();

        // Act
        _writer.Update(webresources);

        // Assert
        _dataverseWriter.Received(1).UpdateMultiple(Arg.Is<IEnumerable<WebResource>>(list => !list.Any()));
    }

    [Fact]
    public void Delete_HandlesEmptyList()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>();

        // Act
        _writer.Delete(webresources);

        // Assert
        _dataverseWriter.Received(1).DeleteMultiple(
            Arg.Is<IEnumerable<Microsoft.Xrm.Sdk.Messages.DeleteRequest>>(list => !list.Any())
        );
    }
}
