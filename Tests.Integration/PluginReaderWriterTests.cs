using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Tests.Integration.Infrastructure;
using XrmPluginCore.Enums;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Plugin;

namespace Tests.Integration;

/// <summary>
/// Integration tests for IPluginReader and IPluginWriter.
/// </summary>
public sealed class PluginReaderWriterTests : TestBase
{
	#region Reader Tests

	[Fact]
	public void GetPluginTypes_ReturnsTypesForAssembly()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("ReaderAssembly");
		var typeId1 = Producer.ProducePluginType(assemblyId, "Namespace.Plugin1");
		var typeId2 = Producer.ProducePluginType(assemblyId, "Namespace.Plugin2");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetPluginTypes(assemblyId);

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Contains(result, pt => pt.Name == "Namespace.Plugin1" && pt.Id == typeId1);
		Assert.Contains(result, pt => pt.Name == "Namespace.Plugin2" && pt.Id == typeId2);
	}

	[Fact]
	public void GetPluginTypes_ReturnsEmpty_WhenNoTypesForAssembly()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("EmptyAssembly");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetPluginTypes(assemblyId);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetPluginSteps_ReturnsStepsWithResolvedNames()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("StepSolution");
		var assemblyId = Producer.ProducePluginAssembly("StepAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.StepPlugin");
		var messageId = Producer.ProduceSdkMessage("Create");
		var filterId = Producer.ProduceSdkMessageFilter(messageId, "account");

		var stepId = Producer.ProducePluginStep(
			typeId, messageId, filterId,
			name: "StepPlugin: Create of account",
			stage: 20, mode: 0, rank: 1,
			filteringAttributes: "name,address1_city");

		Producer.ProduceSolutionComponent(solutionId, stepId, componentType: 92); // 92 = SDKMessageProcessingStep

		var pluginTypes = new List<PluginDefinition>
		{
			new("Namespace.StepPlugin") { Id = typeId, PluginSteps = [] }
		};

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetPluginSteps(pluginTypes, solutionId);

		// Assert
		Assert.Single(result);
		var (step, parent) = result[0];
		Assert.Equal(stepId, step.Id);
		Assert.Equal("StepPlugin: Create of account", step.Name);
		Assert.Equal("Create", step.EventOperation);
		Assert.Equal("account", step.LogicalName);
		Assert.Equal(ExecutionStage.PreOperation, step.ExecutionStage);
		Assert.Equal(ExecutionMode.Synchronous, step.ExecutionMode);
		Assert.Equal(1, step.ExecutionOrder);
		Assert.Equal("name,address1_city", step.FilteredAttributes);
		Assert.Equal("Namespace.StepPlugin", parent.Name);
	}

	[Fact]
	public void GetPluginSteps_ReturnsEmpty_WhenNoPluginTypesProvided()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("EmptyStepSolution");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetPluginSteps([], solutionId);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void GetPluginSteps_ReturnsStepsWithImages()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("ImageSolution");
		var assemblyId = Producer.ProducePluginAssembly("ImageAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.ImagePlugin");
		var messageId = Producer.ProduceSdkMessage("Update");
		var filterId = Producer.ProduceSdkMessageFilter(messageId, "contact");

		var stepId = Producer.ProducePluginStep(
			typeId, messageId, filterId,
			name: "ImagePlugin: Update of contact",
			stage: 40, mode: 0, rank: 1);
		Producer.ProduceSolutionComponent(solutionId, stepId, componentType: 92);

		var preImageId = Producer.ProducePluginStepImage(
			stepId, "PreImage", imageType: 0,
			attributes: "firstname,lastname", entityAlias: "PreImg");
		var postImageId = Producer.ProducePluginStepImage(
			stepId, "PostImage", imageType: 1,
			attributes: "email", entityAlias: "PostImg");

		var pluginTypes = new List<PluginDefinition>
		{
			new("Namespace.ImagePlugin") { Id = typeId, PluginSteps = [] }
		};

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetPluginSteps(pluginTypes, solutionId);

		// Assert
		Assert.Single(result);
		var step = result[0].Entity;
		Assert.Equal(2, step.PluginImages.Count);

		var preImage = step.PluginImages.First(i => i.Name == "PreImage");
		Assert.Equal(preImageId, preImage.Id);
		Assert.Equal("PreImg", preImage.EntityAlias);
		Assert.Equal("firstname,lastname", preImage.Attributes);
		Assert.Equal(ImageType.PreImage, preImage.ImageType);

		var postImage = step.PluginImages.First(i => i.Name == "PostImage");
		Assert.Equal(postImageId, postImage.Id);
		Assert.Equal("PostImg", postImage.EntityAlias);
		Assert.Equal("email", postImage.Attributes);
		Assert.Equal(ImageType.PostImage, postImage.ImageType);
	}

	[Fact]
	public void GetPluginSteps_FiltersStepsBySolution()
	{
		// Arrange
		var (solution1Id, _) = Producer.ProduceSolution("FilterSolution1", "f1");
		var (solution2Id, _) = Producer.ProduceSolution("FilterSolution2", "f2");
		var assemblyId = Producer.ProducePluginAssembly("FilterAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.FilterPlugin");
		var messageId = Producer.ProduceSdkMessage("Delete");
		var filterId = Producer.ProduceSdkMessageFilter(messageId, "account");

		var step1Id = Producer.ProducePluginStep(typeId, messageId, filterId, name: "Step In Solution 1");
		var step2Id = Producer.ProducePluginStep(typeId, messageId, filterId, name: "Step In Solution 2");

		Producer.ProduceSolutionComponent(solution1Id, step1Id, componentType: 92);
		Producer.ProduceSolutionComponent(solution2Id, step2Id, componentType: 92);

		var pluginTypes = new List<PluginDefinition>
		{
			new("Namespace.FilterPlugin") { Id = typeId, PluginSteps = [] }
		};

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetPluginSteps(pluginTypes, solution1Id);

		// Assert
		Assert.Single(result);
		Assert.Equal("Step In Solution 1", result[0].Entity.Name);
	}

	[Fact]
	public void GetMissingUserContexts_ReturnsStepsWithNonExistentUsers()
	{
		// Arrange
		var missingUserId = Guid.NewGuid();
		var steps = new List<Step>
		{
			new("StepWithMissingUser")
			{
				Id = Guid.NewGuid(),
				ExecutionStage = ExecutionStage.PreOperation,
				EventOperation = "Create",
				LogicalName = "account",
				Deployment = Deployment.ServerOnly,
				ExecutionMode = ExecutionMode.Synchronous,
				ExecutionOrder = 1,
				FilteredAttributes = "",
				UserContext = missingUserId,
				AsyncAutoDelete = false,
				PluginImages = []
			},
			new("StepWithNoUser")
			{
				Id = Guid.NewGuid(),
				ExecutionStage = ExecutionStage.PreOperation,
				EventOperation = "Create",
				LogicalName = "account",
				Deployment = Deployment.ServerOnly,
				ExecutionMode = ExecutionMode.Synchronous,
				ExecutionOrder = 1,
				FilteredAttributes = "",
				UserContext = Guid.Empty,
				AsyncAutoDelete = false,
				PluginImages = []
			}
		};

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetMissingUserContexts(steps).ToList();

		// Assert
		Assert.Single(result);
		Assert.Equal("StepWithMissingUser", result[0].Name);
	}

	[Fact]
	public void GetMissingUserContexts_ReturnsEmpty_WhenAllUserContextsAreEmpty()
	{
		// Arrange
		var steps = new List<Step>
		{
			new("Step1")
			{
				Id = Guid.NewGuid(),
				ExecutionStage = ExecutionStage.PreOperation,
				EventOperation = "Create",
				LogicalName = "account",
				Deployment = Deployment.ServerOnly,
				ExecutionMode = ExecutionMode.Synchronous,
				ExecutionOrder = 1,
				FilteredAttributes = "",
				UserContext = Guid.Empty,
				AsyncAutoDelete = false,
				PluginImages = []
			}
		};

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginReader>();

		// Act
		var result = reader.GetMissingUserContexts(steps).ToList();

		// Assert
		Assert.Empty(result);
	}

	#endregion

	#region Writer Tests

	[Fact]
	public void CreatePluginTypes_CreatesEntitiesAndSetsIds()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("CreateTypeSolution");
		var assemblyId = Producer.ProducePluginAssembly("CreateTypeAssembly");

		var pluginTypes = new List<PluginDefinition>
		{
			new("Namespace.NewPlugin1") { PluginSteps = [] },
			new("Namespace.NewPlugin2") { PluginSteps = [] }
		};

		var sp = BuildPluginServiceProvider("CreateTypeSolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		var result = writer.CreatePluginTypes(pluginTypes, assemblyId, "test description");

		// Assert
		Assert.All(result, pt => Assert.NotEqual(Guid.Empty, pt.Id));

		// Verify entities exist in Dataverse
		foreach (var pt in result)
		{
			var retrieved = Service.Retrieve("plugintype", pt.Id, new ColumnSet("typename"));
			Assert.Equal(pt.Name, retrieved.GetAttributeValue<string>("typename"));
		}
	}

	[Fact]
	public void CreatePluginTypes_ReturnsEmpty_WhenNoTypesProvided()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("EmptyCreateAssembly");

		var sp = BuildPluginServiceProvider("AnySolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		var result = writer.CreatePluginTypes([], assemblyId, "test");

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public void CreatePluginSteps_CreatesStepsWithMessageFilterLookup()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("CreateStepSolution");
		var assemblyId = Producer.ProducePluginAssembly("CreateStepAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.CreateStepPlugin");

		// The writer looks up message filters internally, so we need messages and filters
		var messageId = Producer.ProduceSdkMessage("Create");
		Producer.ProduceSdkMessageFilter(messageId, "account");

		var plugin = new PluginDefinition("Namespace.CreateStepPlugin") { Id = typeId, PluginSteps = [] };
		var step = new Step("CreateStepPlugin: Create of account")
		{
			ExecutionStage = ExecutionStage.PreOperation,
			EventOperation = "Create",
			LogicalName = "account",
			Deployment = Deployment.ServerOnly,
			ExecutionMode = ExecutionMode.Synchronous,
			ExecutionOrder = 1,
			FilteredAttributes = "name",
			UserContext = Guid.Empty,
			AsyncAutoDelete = false,
			PluginImages = []
		};

		var pluginSteps = new List<ParentReference<Step, PluginDefinition>>
		{
			new(step, plugin)
		};

		var sp = BuildPluginServiceProvider("CreateStepSolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		var result = writer.CreatePluginSteps(pluginSteps, "test description");

		// Assert
		Assert.Single(result);
		Assert.NotEqual(Guid.Empty, result.First().Entity.Id);

		// Verify entity exists in Dataverse
		var retrieved = Service.Retrieve("sdkmessageprocessingstep", result.First().Entity.Id, new ColumnSet("name", "filteringattributes"));
		Assert.Equal("CreateStepPlugin: Create of account", retrieved.GetAttributeValue<string>("name"));
		Assert.Equal("name", retrieved.GetAttributeValue<string>("filteringattributes"));
	}

	[Fact]
	public void CreatePluginImages_CreatesImagesLinkedToSteps()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("CreateImageSolution");
		var assemblyId = Producer.ProducePluginAssembly("CreateImageAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.ImagePlugin");
		var messageId = Producer.ProduceSdkMessage("Update");
		var filterId = Producer.ProduceSdkMessageFilter(messageId, "account");
		var stepId = Producer.ProducePluginStep(typeId, messageId, filterId, "ImagePlugin: Update of account");

		var step = new Step("ImagePlugin: Update of account")
		{
			Id = stepId,
			ExecutionStage = ExecutionStage.PreOperation,
			EventOperation = "Update",
			LogicalName = "account",
			Deployment = Deployment.ServerOnly,
			ExecutionMode = ExecutionMode.Synchronous,
			ExecutionOrder = 1,
			FilteredAttributes = "",
			UserContext = Guid.Empty,
			AsyncAutoDelete = false,
			PluginImages = []
		};

		var image = new Image("PreImage")
		{
			EntityAlias = "PreImg",
			ImageType = ImageType.PreImage,
			Attributes = "name,address1_city"
		};

		var pluginImages = new List<ParentReference<Image, Step>>
		{
			new(image, step)
		};

		var sp = BuildPluginServiceProvider("CreateImageSolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		var result = writer.CreatePluginImages(pluginImages);

		// Assert
		Assert.Single(result);
		Assert.NotEqual(Guid.Empty, result.First().Entity.Id);

		// Verify entity exists in Dataverse
		var retrieved = Service.Retrieve("sdkmessageprocessingstepimage", result.First().Entity.Id, new ColumnSet("name", "entityalias", "imagetype"));
		Assert.Equal("PreImage", retrieved.GetAttributeValue<string>("name"));
		Assert.Equal("PreImg", retrieved.GetAttributeValue<string>("entityalias"));
		Assert.Equal(0, retrieved.GetAttributeValue<OptionSetValue>("imagetype").Value); // 0 = PreImage
	}

	[Fact]
	public void UpdatePluginSteps_UpdatesProperties()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("UpdateStepAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.UpdatePlugin");
		var messageId = Producer.ProduceSdkMessage("Create");
		var filterId = Producer.ProduceSdkMessageFilter(messageId, "account");
		var stepId = Producer.ProducePluginStep(typeId, messageId, filterId, "UpdatePlugin: Create of account", rank: 1);

		var step = new Step("UpdatePlugin: Create of account")
		{
			Id = stepId,
			ExecutionStage = ExecutionStage.PreOperation,
			EventOperation = "Create",
			LogicalName = "account",
			Deployment = Deployment.ServerOnly,
			ExecutionMode = ExecutionMode.Synchronous,
			ExecutionOrder = 5, // Changed from 1 to 5
			FilteredAttributes = "name,revenue",
			UserContext = Guid.Empty,
			AsyncAutoDelete = false,
			PluginImages = []
		};

		var sp = BuildPluginServiceProvider("AnySolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		writer.UpdatePluginSteps([step], "updated description");

		// Assert
		var retrieved = Service.Retrieve("sdkmessageprocessingstep", stepId, new ColumnSet("rank", "filteringattributes", "description"));
		Assert.Equal(5, retrieved.GetAttributeValue<int>("rank"));
		Assert.Equal("name,revenue", retrieved.GetAttributeValue<string>("filteringattributes"));
		Assert.Equal("updated description", retrieved.GetAttributeValue<string>("description"));
	}

	[Fact]
	public void DeletePluginTypes_RemovesEntities()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("DeleteTypeAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.DeletePlugin");

		var pluginTypes = new List<PluginDefinition>
		{
			new("Namespace.DeletePlugin") { Id = typeId, PluginSteps = [] }
		};

		var sp = BuildPluginServiceProvider("AnySolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		writer.DeletePluginTypes(pluginTypes);

		// Assert
		Assert.ThrowsAny<Exception>(() => Service.Retrieve("plugintype", typeId, new ColumnSet("typename")));
	}

	[Fact]
	public void DeletePluginSteps_RemovesEntities()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("DeleteStepAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.DeleteStepPlugin");
		var messageId = Producer.ProduceSdkMessage("Create");
		var filterId = Producer.ProduceSdkMessageFilter(messageId, "account");
		var stepId = Producer.ProducePluginStep(typeId, messageId, filterId, "DeleteStepPlugin: Create of account");

		var steps = new List<Step>
		{
			new("DeleteStepPlugin: Create of account")
			{
				Id = stepId,
				ExecutionStage = ExecutionStage.PreOperation,
				EventOperation = "Create",
				LogicalName = "account",
				Deployment = Deployment.ServerOnly,
				ExecutionMode = ExecutionMode.Synchronous,
				ExecutionOrder = 1,
				FilteredAttributes = "",
				UserContext = Guid.Empty,
				AsyncAutoDelete = false,
				PluginImages = []
			}
		};

		var sp = BuildPluginServiceProvider("AnySolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		writer.DeletePluginSteps(steps);

		// Assert
		Assert.ThrowsAny<Exception>(() => Service.Retrieve("sdkmessageprocessingstep", stepId, new ColumnSet("name")));
	}

	[Fact]
	public void DeletePluginImages_RemovesEntities()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("DeleteImageAssembly");
		var typeId = Producer.ProducePluginType(assemblyId, "Namespace.DeleteImagePlugin");
		var messageId = Producer.ProduceSdkMessage("Update");
		var filterId = Producer.ProduceSdkMessageFilter(messageId, "account");
		var stepId = Producer.ProducePluginStep(typeId, messageId, filterId, "DeleteImagePlugin: Update of account");
		var imageId = Producer.ProducePluginStepImage(stepId, "PreImage", imageType: 0, attributes: "name");

		var images = new List<Image>
		{
			new("PreImage") { Id = imageId, EntityAlias = "PreImg", ImageType = ImageType.PreImage, Attributes = "name" }
		};

		var sp = BuildPluginServiceProvider("AnySolution");
		var writer = sp.GetRequiredService<IPluginWriter>();

		// Act
		writer.DeletePluginImages(images);

		// Assert
		Assert.ThrowsAny<Exception>(() => Service.Retrieve("sdkmessageprocessingstepimage", imageId, new ColumnSet("name")));
	}

	#endregion
}
