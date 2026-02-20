using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk.Query;
using Tests.Integration.Infrastructure;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;

namespace Tests.Integration;

/// <summary>
/// Integration tests for IWebresourceReader and IWebresourceWriter.
/// </summary>
public sealed class WebresourceReaderWriterTests : TestBase
{
	#region Reader Tests

	[Fact]
	public void GetWebresources_ReturnsWebresourcesInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("WrReaderSolution");
		var wrId = Producer.ProduceWebresource(
			"test_script.js", "Script", webresourceType: 3,
			content: Convert.ToBase64String("console.log('hello')"u8.ToArray()));
		Producer.ProduceSolutionComponent(solutionId, wrId, componentType: 61); // 61 = WebResource

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IWebresourceReader>();

		// Act
		var result = reader.GetWebresources(solutionId);

		// Assert
		Assert.Single(result);
		var wr = result[0];
		Assert.Equal(wrId, wr.Id);
		Assert.Equal("test_script.js", wr.Name);
		Assert.Equal("Script", wr.DisplayName);
		Assert.Equal(WebresourceType.JS, wr.Type);
		Assert.Equal(Convert.ToBase64String("console.log('hello')"u8.ToArray()), wr.Content);
	}

	[Fact]
	public void GetWebresources_ReturnsEmpty_WhenNoWebresourcesInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("EmptyWrSolution");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IWebresourceReader>();

		// Act
		var result = reader.GetWebresources(solutionId);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetWebresources_FiltersWebresourcesNotInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("FilterWrSolution");
		Producer.ProduceWebresource("test_orphan.js", "Orphan", webresourceType: 3);
		// Note: no solution component

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IWebresourceReader>();

		// Act
		var result = reader.GetWebresources(solutionId);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetWebresources_ReturnsOrderedByName()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("OrderedWrSolution");
		var wrId2 = Producer.ProduceWebresource("test_z_script.js", "Z Script", webresourceType: 3);
		var wrId1 = Producer.ProduceWebresource("test_a_script.js", "A Script", webresourceType: 3);
		Producer.ProduceSolutionComponent(solutionId, wrId1, componentType: 61);
		Producer.ProduceSolutionComponent(solutionId, wrId2, componentType: 61);

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IWebresourceReader>();

		// Act
		var result = reader.GetWebresources(solutionId);

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Equal("test_a_script.js", result[0].Name);
		Assert.Equal("test_z_script.js", result[1].Name);
	}

	[Fact]
	public void GetWebresourcesByNames_ReturnsNameToIdMapping()
	{
		// Arrange
		var wrId1 = Producer.ProduceWebresource("test_ByName1.js", "ByName1", webresourceType: 3);
		var wrId2 = Producer.ProduceWebresource("test_ByName2.css", "ByName2", webresourceType: 2);

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IWebresourceReader>();

		// Act
		var result = reader.GetWebresourcesByNames(["test_ByName1.js", "test_ByName2.css"]);

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Equal(wrId1, result["test_ByName1.js"]);
		Assert.Equal(wrId2, result["test_ByName2.css"]);
	}

	[Fact]
	public void GetWebresourcesByNames_ReturnsEmpty_WhenNoNamesProvided()
	{
		// Arrange
		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IWebresourceReader>();

		// Act
		var result = reader.GetWebresourcesByNames([]);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetWebresourcesByNames_ReturnsOnlyMatchingNames()
	{
		// Arrange
		Producer.ProduceWebresource("test_exists.js", "Exists", webresourceType: 3);

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IWebresourceReader>();

		// Act
		var result = reader.GetWebresourcesByNames(["test_exists.js", "test_nonexistent.js"]);

		// Assert
		Assert.Single(result);
		Assert.True(result.ContainsKey("test_exists.js"));
	}

	#endregion

	#region Writer Tests

	[Fact]
	public void Create_CreatesWebresources()
	{
		// Arrange
		Producer.ProduceSolution("CreateWrSolution");

		var webresources = new List<WebresourceDefinition>
		{
			new("test_new.js", "New Script", WebresourceType.JS, Convert.ToBase64String("var x = 1;"u8.ToArray()))
		};

		var sp = BuildWebresourceServiceProvider("CreateWrSolution");
		var writer = sp.GetRequiredService<IWebresourceWriter>();

		// Act
		writer.Create(webresources);

		// Assert - verify entity was created by querying for it
		var query = new QueryExpression("webresource")
		{
			ColumnSet = new ColumnSet("name", "content", "displayname", "webresourcetype"),
			Criteria = { Conditions = { new ConditionExpression("name", ConditionOperator.Equal, "test_new.js") } }
		};
		var results = Service.RetrieveMultiple(query);
		Assert.Single(results.Entities);
		Assert.Equal("test_new.js", results.Entities[0].GetAttributeValue<string>("name"));
		Assert.Equal("New Script", results.Entities[0].GetAttributeValue<string>("displayname"));
		Assert.Equal(Convert.ToBase64String("var x = 1;"u8.ToArray()), results.Entities[0].GetAttributeValue<string>("content"));
	}

	[Fact]
	public void Update_UpdatesWebresourceContent()
	{
		// Arrange
		Producer.ProduceSolution("UpdateWrSolution");
		var wrId = Producer.ProduceWebresource(
			"test_update.js", "Update Script", webresourceType: 3,
			content: Convert.ToBase64String("old content"u8.ToArray()));

		var webresources = new List<WebresourceDefinition>
		{
			new("test_update.js", "Updated Script", WebresourceType.JS, Convert.ToBase64String("new content"u8.ToArray()))
			{
				Id = wrId
			}
		};

		var sp = BuildWebresourceServiceProvider("UpdateWrSolution");
		var writer = sp.GetRequiredService<IWebresourceWriter>();

		// Act
		writer.Update(webresources);

		// Assert
		var retrieved = Service.Retrieve("webresource", wrId, new ColumnSet("content", "displayname"));
		Assert.Equal(Convert.ToBase64String("new content"u8.ToArray()), retrieved.GetAttributeValue<string>("content"));
		Assert.Equal("Updated Script", retrieved.GetAttributeValue<string>("displayname"));
	}

	[Fact]
	public void Delete_RemovesWebresources()
	{
		// Arrange
		Producer.ProduceSolution("DeleteWrSolution");
		var wrId = Producer.ProduceWebresource("test_delete.js", "Delete Script", webresourceType: 3);

		var webresources = new List<WebresourceDefinition>
		{
			new("test_delete.js", "Delete Script", WebresourceType.JS, "")
			{
				Id = wrId
			}
		};

		var sp = BuildWebresourceServiceProvider("DeleteWrSolution");
		var writer = sp.GetRequiredService<IWebresourceWriter>();

		// Act
		writer.Delete(webresources);

		// Assert
		Assert.ThrowsAny<Exception>(() => Service.Retrieve("webresource", wrId, new ColumnSet("name")));
	}

	#endregion
}
