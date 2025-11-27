using NSubstitute;
using XrmSync.Dataverse;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;

namespace Tests.Webresources;

public class WebresourceReaderTests
{
	private readonly IDataverseReader _dataverseReader = Substitute.For<IDataverseReader>();
	private readonly WebresourceReader _reader;

	public WebresourceReaderTests()
	{
		_reader = new WebresourceReader(_dataverseReader);
	}

	private static WebResource CreateWebResource(Guid id, string name, string displayName,
		webresource_webresourcetype type, string content, bool isManaged = false)
	{
		var wr = new WebResource { Id = id };
		wr["name"] = name;
		wr["displayname"] = displayName;
		wr["webresourcetype"] = new Microsoft.Xrm.Sdk.OptionSetValue((int)type);
		wr["content"] = content;
		wr["ismanaged"] = isManaged;
		return wr;
	}

	private static SolutionComponent CreateSolutionComponent(Guid objectId, Guid solutionId)
	{
		var sc = new SolutionComponent { Id = Guid.NewGuid() };
		sc["objectid"] = objectId;
		sc["solutionid"] = new Microsoft.Xrm.Sdk.EntityReference(Solution.EntityLogicalName, solutionId);
		return sc;
	}

	[Fact]
	public void GetWebresourcesReturnsWebresourcesForSolution()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var webresourceId1 = Guid.NewGuid();
		var webresourceId2 = Guid.NewGuid();

		var webresources = new List<WebResource>
		{
			CreateWebResource(webresourceId1, "test_solution/test1.js", "Test 1",
				webresource_webresourcetype.ScriptJScript, "Y29uc29sZS5sb2coJ3Rlc3QxJyk7"),
			CreateWebResource(webresourceId2, "test_solution/test2.js", "Test 2",
				webresource_webresourcetype.ScriptJScript, "Y29uc29sZS5sb2coJ3Rlc3QyJyk7")
		}.AsQueryable();

		var solutionComponents = new List<SolutionComponent>
		{
			CreateSolutionComponent(webresourceId1, solutionId),
			CreateSolutionComponent(webresourceId2, solutionId)
		}.AsQueryable();

		_dataverseReader.WebResources.Returns(webresources);
		_dataverseReader.SolutionComponents.Returns(solutionComponents);

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Contains(result, w => w.Name == "test_solution/test1.js");
		Assert.Contains(result, w => w.Name == "test_solution/test2.js");
		Assert.All(result, w => Assert.NotEqual(Guid.Empty, w.Id));
	}

	[Fact]
	public void GetWebresourcesExcludesManagedWebresources()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var unmanagedId = Guid.NewGuid();
		var managedId = Guid.NewGuid();

		var webresources = new List<WebResource>
		{
			CreateWebResource(unmanagedId, "test_solution/unmanaged.js", "Unmanaged",
				webresource_webresourcetype.ScriptJScript, "dW5tYW5hZ2Vk", false),
			CreateWebResource(managedId, "test_solution/managed.js", "Managed",
				webresource_webresourcetype.ScriptJScript, "bWFuYWdlZA==", true)
		}.AsQueryable();

		var solutionComponents = new List<SolutionComponent>
		{
			CreateSolutionComponent(unmanagedId, solutionId),
			CreateSolutionComponent(managedId, solutionId)
		}.AsQueryable();

		_dataverseReader.WebResources.Returns(webresources);
		_dataverseReader.SolutionComponents.Returns(solutionComponents);

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Single(result);
		Assert.Equal("test_solution/unmanaged.js", result[0].Name);
	}

	[Fact]
	public void GetWebresourcesMapsWebresourceTypeCorrectly()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var webresourceId = Guid.NewGuid();

		var webresources = new List<WebResource>
		{
			CreateWebResource(webresourceId, "test.css", "Test CSS",
				webresource_webresourcetype.StyleSheetCSS, "Ym9keXt9")
		}.AsQueryable();

		var solutionComponents = new List<SolutionComponent>
		{
			CreateSolutionComponent(webresourceId, solutionId)
		}.AsQueryable();

		_dataverseReader.WebResources.Returns(webresources);
		_dataverseReader.SolutionComponents.Returns(solutionComponents);

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Single(result);
		Assert.Equal(WebresourceType.CSS, result[0].Type);
	}

	[Fact]
	public void GetWebresourcesReturnsEmptyListWhenNoWebresourcesInSolution()
	{
		// Arrange
		var solutionId = Guid.NewGuid();

		_dataverseReader.WebResources.Returns(new List<WebResource>().AsQueryable());
		_dataverseReader.SolutionComponents.Returns(new List<SolutionComponent>().AsQueryable());

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetWebresourcesOrdersByName()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var id1 = Guid.NewGuid();
		var id2 = Guid.NewGuid();
		var id3 = Guid.NewGuid();

		var webresources = new List<WebResource>
		{
			CreateWebResource(id1, "z_last.js", "Last",
				webresource_webresourcetype.ScriptJScript, "bGFzdA=="),
			CreateWebResource(id2, "a_first.js", "First",
				webresource_webresourcetype.ScriptJScript, "Zmlyc3Q="),
			CreateWebResource(id3, "m_middle.js", "Middle",
				webresource_webresourcetype.ScriptJScript, "bWlkZGxl")
		}.AsQueryable();

		var solutionComponents = new List<SolutionComponent>
		{
			CreateSolutionComponent(id1, solutionId),
			CreateSolutionComponent(id2, solutionId),
			CreateSolutionComponent(id3, solutionId)
		}.AsQueryable();

		_dataverseReader.WebResources.Returns(webresources);
		_dataverseReader.SolutionComponents.Returns(solutionComponents);

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Equal(3, result.Count);
		Assert.Equal("a_first.js", result[0].Name);
		Assert.Equal("m_middle.js", result[1].Name);
		Assert.Equal("z_last.js", result[2].Name);
	}

	[Fact]
	public void GetWebresourcesHandlesNullValues()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var webresourceId = Guid.NewGuid();

		var wr = new WebResource { Id = webresourceId };
		wr["ismanaged"] = false;
		// Leave other fields as null

		var webresources = new List<WebResource> { wr }.AsQueryable();

		var solutionComponents = new List<SolutionComponent>
		{
			CreateSolutionComponent(webresourceId, solutionId)
		}.AsQueryable();

		_dataverseReader.WebResources.Returns(webresources);
		_dataverseReader.SolutionComponents.Returns(solutionComponents);

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Single(result);
		Assert.Equal(string.Empty, result[0].Name);
		Assert.Equal(string.Empty, result[0].DisplayName);
		Assert.Equal(string.Empty, result[0].Content);
		Assert.Equal((WebresourceType)0, result[0].Type);
	}
}
