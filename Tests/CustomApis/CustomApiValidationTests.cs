using XrmPluginCore.Enums;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.CustomApi;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.Extensions;
using XrmSync.SyncService.Validation;
using XrmSync.SyncService.Validation.CustomApi;

namespace Tests.CustomApis;

public class CustomApiValidationTests
{
	private static IValidator<CustomApiDefinition> CreateValidator()
	{
		var services = new ServiceCollection();
		services.AddValidationRules();
		services.AddSingleton(Substitute.For<IPluginReader>());
		services.AddSingleton<IValidator<CustomApiDefinition>, CustomApiValidator>();
		var sp = services.BuildServiceProvider();
		return sp.GetRequiredService<IValidator<CustomApiDefinition>>();
	}

	private static CustomApiDefinition CreateCustomApi(BindingType bindingType, string boundEntityLogicalName) => new("TestAPI")
	{
		DisplayName = "TestAPI",
		UniqueName = "new_TestAPI",
		BindingType = bindingType,
		BoundEntityLogicalName = boundEntityLogicalName,
		Description = "A test custom API",
		IsFunction = false,
		IsPrivate = false,
		IsCustomizable = true,
		EnabledForWorkflow = false,
		AllowedCustomProcessingStepType = (AllowedCustomProcessingStepType)0,
		ExecutePrivilegeName = string.Empty,
		PluginType = new PluginType("TestPluginType") { Id = Guid.NewGuid() }
	};

	[Fact]
	public void ValidBoundApiPassesValidation()
	{
		// Arrange
		var api = CreateCustomApi(BindingType.Entity, "account");
		var validator = CreateValidator();

		// Act & Assert — should not throw
		validator.ValidateOrThrow([api]);
	}

	[Fact]
	public void ValidUnboundApiPassesValidation()
	{
		// Arrange
		var api = CreateCustomApi((BindingType)0, string.Empty);
		var validator = CreateValidator();

		// Act & Assert — should not throw
		validator.ValidateOrThrow([api]);
	}

	[Fact]
	public void BoundApiWithoutEntityNameThrows()
	{
		// Arrange
		var api = CreateCustomApi(BindingType.Entity, string.Empty);
		var validator = CreateValidator();

		// Act & Assert
		var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([api]));
		Assert.Contains("Bound Custom API must specify an entity type", ex.Message);
	}

	[Fact]
	public void UnboundApiWithEntityNameThrows()
	{
		// Arrange
		var api = CreateCustomApi((BindingType)0, "account");
		var validator = CreateValidator();

		// Act & Assert
		var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([api]));
		Assert.Contains("Unbound Custom API cannot specify an entity type", ex.Message);
	}

	[Fact]
	public void EntityCollectionBindingWithoutEntityNameThrows()
	{
		// Arrange
		var api = CreateCustomApi(BindingType.EntityCollection, string.Empty);
		var validator = CreateValidator();

		// Act & Assert
		var ex = Assert.Throws<ValidationException>(() => validator.ValidateOrThrow([api]));
		Assert.Contains("Bound Custom API must specify an entity type", ex.Message);
	}

	[Fact]
	public void MultipleViolationsThrowsAggregateException()
	{
		// Arrange
		var boundWithoutEntity = CreateCustomApi(BindingType.Entity, string.Empty);
		var unboundWithEntity = CreateCustomApi((BindingType)0, "account");
		var validator = CreateValidator();

		// Act & Assert
		var ex = Assert.Throws<AggregateException>(() => validator.ValidateOrThrow([boundWithoutEntity, unboundWithEntity]));
		Assert.Equal(2, ex.InnerExceptions.Count);
		var messages = ex.InnerExceptions.Select(e => e.Message).ToList();
		Assert.Contains(messages, m => m.Contains("Bound Custom API must specify an entity type"));
		Assert.Contains(messages, m => m.Contains("Unbound Custom API cannot specify an entity type"));
	}

	[Fact]
	public void MixOfValidApisPassesValidation()
	{
		// Arrange
		var validBound = CreateCustomApi(BindingType.Entity, "account");
		var validUnbound = CreateCustomApi((BindingType)0, string.Empty);
		var validator = CreateValidator();

		// Act & Assert — should not throw
		validator.ValidateOrThrow([validBound, validUnbound]);
	}

	[Fact]
	public void EmptyCollectionPassesValidation()
	{
		// Arrange
		var validator = CreateValidator();

		// Act & Assert — should not throw
		validator.ValidateOrThrow([]);
	}
}
