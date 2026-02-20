using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Infrastructure;
using XrmSync.Dataverse.Interfaces;

namespace Tests.Integration;

/// <summary>
/// Integration tests for IMessageReader.
/// </summary>
public sealed class MessageReaderTests : TestBase
{
	[Fact]
	public void GetMessageFilters_ReturnsCorrectMapping()
	{
		// Arrange
		var createMessageId = Producer.ProduceSdkMessage("Create");
		var updateMessageId = Producer.ProduceSdkMessage("Update");
		var createFilterId = Producer.ProduceSdkMessageFilter(createMessageId, "account");
		var updateFilterId = Producer.ProduceSdkMessageFilter(updateMessageId, "contact");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IMessageReader>();

		// Act
		var result = reader.GetMessageFilters(["Create", "Update"], ["account", "contact"]);

		// Assert
		Assert.Equal(2, result.Count);

		Assert.True(result.ContainsKey("Create"));
		Assert.Equal(createMessageId, result["Create"].MessageId);
		Assert.True(result["Create"].FilterMap.ContainsKey("account"));
		Assert.Equal(createFilterId, result["Create"].FilterMap["account"]);

		Assert.True(result.ContainsKey("Update"));
		Assert.Equal(updateMessageId, result["Update"].MessageId);
		Assert.True(result["Update"].FilterMap.ContainsKey("contact"));
		Assert.Equal(updateFilterId, result["Update"].FilterMap["contact"]);
	}

	[Fact]
	public void GetMessageFilters_ReturnsEmptyDictionary_WhenNoMessageNamesProvided()
	{
		// Arrange
		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IMessageReader>();

		// Act
		var result = reader.GetMessageFilters([], ["account"]);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetMessageFilters_ReturnsMessageWithEmptyFilterMap_WhenNoMatchingEntities()
	{
		// Arrange
		Producer.ProduceSdkMessage("Delete");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IMessageReader>();

		// Act
		var result = reader.GetMessageFilters(["Delete"], ["nonexistent"]);

		// Assert
		Assert.Single(result);
		Assert.True(result.ContainsKey("Delete"));
		Assert.Empty(result["Delete"].FilterMap);
	}

	[Fact]
	public void GetMessageFilters_ReturnsMultipleFiltersForSameMessage()
	{
		// Arrange
		var createMessageId = Producer.ProduceSdkMessage("Create");
		var accountFilterId = Producer.ProduceSdkMessageFilter(createMessageId, "account");
		var contactFilterId = Producer.ProduceSdkMessageFilter(createMessageId, "contact");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IMessageReader>();

		// Act
		var result = reader.GetMessageFilters(["Create"], ["account", "contact"]);

		// Assert
		Assert.Single(result);
		Assert.Equal(2, result["Create"].FilterMap.Count);
		Assert.Equal(accountFilterId, result["Create"].FilterMap["account"]);
		Assert.Equal(contactFilterId, result["Create"].FilterMap["contact"]);
	}
}
