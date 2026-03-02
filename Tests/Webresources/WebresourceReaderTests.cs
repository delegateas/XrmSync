using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NSubstitute;
using XrmSync.Dataverse;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;

namespace Tests.Webresources;

public class WebresourceReaderTests
{
	private readonly IDataverseReader _dataverseReader = Substitute.For<IDataverseReader>();
	private readonly IOrganizationServiceProvider _serviceProvider = Substitute.For<IOrganizationServiceProvider>();
	private readonly IOrganizationService _orgService = Substitute.For<IOrganizationService>();
	private readonly WebresourceReader _reader;

	public WebresourceReaderTests()
	{
		_serviceProvider.Service.Returns(_orgService);
		_reader = new WebresourceReader(_dataverseReader, _serviceProvider);
	}

	private static WebResource CreateWebResource(Guid id, string name, string displayName,
		webresource_webresourcetype type, string content, bool isManaged = false)
	{
		var wr = new WebResource { Id = id };
		wr["name"] = name;
		wr["displayname"] = displayName;
		wr["webresourcetype"] = new OptionSetValue((int)type);
		wr["content"] = content;
		wr["ismanaged"] = isManaged;
		return wr;
	}

	private void SetupRetrieveMultiple(params WebResource[] webresources)
	{
		var collection = new EntityCollection();
		foreach (var wr in webresources)
			collection.Entities.Add(wr);

		_orgService.RetrieveMultiple(Arg.Any<QueryExpression>()).Returns(collection);
	}

	[Fact]
	public void GetWebresourcesReturnsWebresourcesForSolution()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var wr1 = CreateWebResource(Guid.NewGuid(), "test_solution/test1.js", "Test 1",
			webresource_webresourcetype.ScriptJScript, "Y29uc29sZS5sb2coJ3Rlc3QxJyk7");
		var wr2 = CreateWebResource(Guid.NewGuid(), "test_solution/test2.js", "Test 2",
			webresource_webresourcetype.ScriptJScript, "Y29uc29sZS5sb2coJ3Rlc3QyJyk7");

		SetupRetrieveMultiple(wr1, wr2);

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Contains(result, w => w.Name == "test_solution/test1.js");
		Assert.Contains(result, w => w.Name == "test_solution/test2.js");
		Assert.All(result, w => Assert.NotEqual(Guid.Empty, w.Id));
	}

	[Fact]
	public void GetWebresourcesMapsWebresourceTypeCorrectly()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var wr = CreateWebResource(Guid.NewGuid(), "test.css", "Test CSS",
			webresource_webresourcetype.StyleSheetCSS, "Ym9keXt9");

		SetupRetrieveMultiple(wr);

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
		SetupRetrieveMultiple();

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetWebresourcesHandlesNullValues()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		var wr = new WebResource { Id = Guid.NewGuid() };
		wr["ismanaged"] = false;

		SetupRetrieveMultiple(wr);

		// Act
		var result = _reader.GetWebresources(solutionId);

		// Assert
		Assert.Single(result);
		Assert.Equal(string.Empty, result[0].Name);
		Assert.Equal(string.Empty, result[0].DisplayName);
		Assert.Equal(string.Empty, result[0].Content);
		Assert.Equal((WebresourceType)0, result[0].Type);
	}

	[Fact]
	public void GetWebresourcesPassesAllowedTypesToQuery()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		SetupRetrieveMultiple();

		var allowedTypes = new[] { WebresourceType.JS, WebresourceType.CSS };

		// Act
		_reader.GetWebresources(solutionId, allowedTypes);

		// Assert - verify the QueryExpression includes a webresourcetype condition
		_orgService.Received(1).RetrieveMultiple(Arg.Is<QueryExpression>(q =>
			q.Criteria.Conditions.Any(c =>
				c.AttributeName == "webresourcetype" &&
				c.Operator == ConditionOperator.In)));
	}

	[Fact]
	public void GetWebresourcesOmitsTypeFilterWhenNoAllowedTypes()
	{
		// Arrange
		var solutionId = Guid.NewGuid();
		SetupRetrieveMultiple();

		// Act
		_reader.GetWebresources(solutionId);

		// Assert - verify no webresourcetype condition exists
		_orgService.Received(1).RetrieveMultiple(Arg.Is<QueryExpression>(q =>
			!q.Criteria.Conditions.Any(c => c.AttributeName == "webresourcetype")));
	}
}
