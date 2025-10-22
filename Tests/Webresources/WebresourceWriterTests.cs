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
            new("test_solution/test.js", "Test Script", WebresourceType.ScriptJscript, "Y29uc29sZS5sb2coJ3Rlc3QnKTs=")
        };

        // Act
        _writer.Create(webresources);

        // Assert
        _dataverseWriter.Received(1).Create(
            Arg.Is<WebResource>(wr =>
                wr.Name == "test_solution/test.js"
                && wr.DisplayName == "Test Script"
                && wr.Content == "Y29uc29sZS5sb2coJ3Rlc3QnKTs="
                && wr.WebResourceType == WebResource_WebResourceType.ScriptJscript),
            Arg.Any<Dictionary<string, object>>()
        );
    }

    [Fact]
    public void Create_PassesSolutionNameInParameters()
    {
        // Arrange
        var webresources = new List<WebresourceDefinition>
        {
            new("test.js", "Test", WebresourceType.ScriptJscript, "dGVzdA==")
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
            new("test_solution/script.js", "Script", WebresourceType.ScriptJscript, "anM="),
            new("test_solution/style.css", "Style", WebresourceType.StyleSheetCss, "Y3Nz"),
            new("test_solution/page.html", "Page", WebresourceType.WebpageHtml, "aHRtbA==")
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
            (WebresourceType.WebpageHtml, WebResource_WebResourceType.WebpageHtml),
            (WebresourceType.StyleSheetCss, WebResource_WebResourceType.StyleSheetCss),
            (WebresourceType.ScriptJscript, WebResource_WebResourceType.ScriptJscript),
            (WebresourceType.DataXml, WebResource_WebResourceType.DataXml),
            (WebresourceType.PngFormat, WebResource_WebResourceType.PngFormat),
            (WebresourceType.JpgFormat, WebResource_WebResourceType.JpgFormat),
            (WebresourceType.GifFormat, WebResource_WebResourceType.GifFormat),
            (WebresourceType.SilverlightXap, WebResource_WebResourceType.SilverlightXap),
            (WebresourceType.StyleSheetXsl, WebResource_WebResourceType.StyleSheetXsl),
            (WebresourceType.IcoFormat, WebResource_WebResourceType.IcoFormat),
            (WebresourceType.VectorFormatSvg, WebResource_WebResourceType.VectorFormatSvg),
            (WebresourceType.StringResx, WebResource_WebResourceType.StringResx)
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
            new("test_solution/test.js", "Updated Display Name", WebresourceType.ScriptJscript, "dXBkYXRlZENvbnRlbnQ=")
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
            new("test1.js", "Test 1", WebresourceType.ScriptJscript, "dGVzdDE=") { Id = Guid.NewGuid() },
            new("test2.js", "Test 2", WebresourceType.ScriptJscript, "dGVzdDI=") { Id = Guid.NewGuid() },
            new("test3.js", "Test 3", WebresourceType.ScriptJscript, "dGVzdDM=") { Id = Guid.NewGuid() }
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
            new("test_solution/test.js", "Display Name", WebresourceType.ScriptJscript, "Y29udGVudA==")
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
            new("test_solution/test.js", "Test Script", WebresourceType.ScriptJscript, "dGVzdA==")
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
            new("test1.js", "Test 1", WebresourceType.ScriptJscript, "dGVzdDE=") { Id = Guid.NewGuid() },
            new("test2.js", "Test 2", WebresourceType.ScriptJscript, "dGVzdDI=") { Id = Guid.NewGuid() },
            new("test3.js", "Test 3", WebresourceType.ScriptJscript, "dGVzdDM=") { Id = Guid.NewGuid() }
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
