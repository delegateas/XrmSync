using DG.XrmPluginSync.SyncService.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.Dataverse.Interfaces;

namespace DG.XrmPluginSync.SyncService;

public class Plugin(ILogger log, IPluginReader pluginReader, IPluginWriter pluginWriter, Description description)
{
	public PluginAssembly? GetPluginAssembly(Guid solutionId, string assemblyName)
	{
		var assemblyEntity = pluginReader.GetPluginAssembly(solutionId, assemblyName);
		if (assemblyEntity == null) return null;

        return new()
		{
			Id = assemblyEntity.Id,
			Name = assemblyEntity.GetAttributeValue<string>("name"),
			Version = assemblyEntity.GetAttributeValue<string>("version"),
			Hash = assemblyEntity.GetAttributeValue<string>("sourcehash"),
            PluginTypes = GetPluginTypes(solutionId, assemblyEntity.Id),
            DllPath = string.Empty,
		};
    }

    private List<PluginTypeEntity> GetPluginTypes(Guid solutionId, Guid pluginAssemblyId)
    {
        var pluginTypes = pluginReader.GetPluginTypes(pluginAssemblyId);
        return pluginTypes
            .ConvertAll(type =>
            {
                var pluginSteps = pluginReader.GetPluginSteps(solutionId, type.Id);
                var pluginStepDtos = pluginSteps.ConvertAll(step =>
                {
                    var pluginImages = GetPluginImages(step);

                    return new PluginStepEntity
                    {
                        Id = step.Id,
                        LogicalName = string.Empty, // TODO step.GetAttributeValue<string>(),
                        EventOperation = string.Empty, // TODO step.GetAttributeValue<string>("eventoperation"),
                        ExecutionStage = step.GetAttributeValue<OptionSetValue>("stage").Value,
                        Deployment = step.GetAttributeValue<OptionSetValue>("supporteddeployment").Value,
                        ExecutionMode = step.GetAttributeValue<OptionSetValue>("mode").Value,
                        ExecutionOrder = step.GetAttributeValue<int>("rank"),
                        FilteredAttributes = step.GetAttributeValue<string>("filteringattributes"),
                        UserContext = step.GetAttributeValue<EntityReference>("impersonatinguserid")?.Id ?? Guid.Empty,
                        PluginTypeName = type.GetAttributeValue<string>("name"),
                        Name = step.GetAttributeValue<string>("name"),
                        PluginImages = pluginImages
                    };
                });

                return new PluginTypeEntity
                {
                    Name = type.GetAttributeValue<string>("name"),
                    PluginSteps = pluginStepDtos,
                    Id = type.Id
                };
            });
    }

    private List<PluginImageEntity> GetPluginImages(Entity step)
    {
        return pluginReader.GetPluginImages(step.Id)
                            .ConvertAll(image => new PluginImageEntity
                            {
                                Id = image.Id,
                                PluginStepName = step.GetAttributeValue<string>("name"),
                                Name = image.GetAttributeValue<string>("name"),
                                EntityAlias = image.GetAttributeValue<string>("entityalias"),
                                ImageType = image.GetAttributeValue<OptionSetValue>("imagetype").Value,
                                Attributes = image.GetAttributeValue<string>("attributes"),
                            });
    }

    public PluginAssembly CreatePluginAssembly(PluginAssembly localAssembly, string solutionName)
    {
        log.LogInformation($"Creating assembly {localAssembly.Name}");
        var assemblyId = pluginWriter.CreatePluginAssembly(localAssembly.Name, solutionName, localAssembly.DllPath, localAssembly.Hash, localAssembly.Version, description.SyncDescription);

        return localAssembly with { Id = assemblyId };
    }

    public void UpdatePluginAssembly(Guid assemblyId, PluginAssembly localAssembly)
    {
        log.LogInformation($"Updating assembly {localAssembly.Name}");
        pluginWriter.UpdatePluginAssembly(assemblyId, localAssembly.Name, localAssembly.DllPath, localAssembly.Hash, localAssembly.Version, description.SyncDescription);
    }

    public void CreatePlugins(PluginAssembly crmAssembly, List<PluginStepEntity> crmPluginSteps, string solutionName, List<PluginTypeEntity> pluginTypes, List<PluginStepEntity> pluginSteps, List<PluginImageEntity> pluginImages)
    {
        // Create - Doing it in the order type -> step -> image. Because there is a necessary relation. 
        // Creating plugin types and appending them to existing crm plugintypes
        var createdPluginTypes = pluginWriter.CreatePluginTypes(pluginTypes, crmAssembly.Id, description.SyncDescription);
        crmAssembly.PluginTypes.AddRange(createdPluginTypes);

        var createdPluginSteps = pluginWriter.CreatePluginSteps(pluginSteps, crmAssembly.PluginTypes, solutionName, description.SyncDescription);
        crmPluginSteps.AddRange(createdPluginSteps);

        pluginWriter.CreatePluginImages(pluginImages, crmPluginSteps);
    }

    public void DeletePlugins(List<PluginTypeEntity> pluginTypes, List<PluginStepEntity> pluginSteps, List<PluginImageEntity> pluginImages)
    {
        pluginWriter.DeletePlugins(pluginTypes, pluginSteps, pluginImages);
    }

    public void UpdatePlugins(List<PluginStepEntity> pluginSteps, List<PluginImageEntity> pluginImages)
    {
        pluginWriter.UpdatePlugins(pluginSteps, pluginImages, description.SyncDescription);
    }

    public void ValidatePlugins(List<PluginTypeEntity> pluginTypes)
	{
		List<Exception> exceptions = [];
		var pluginSteps = pluginTypes.SelectMany(x => x.PluginSteps);
		var preOperationAsyncPlugins = pluginSteps
			.Where(x =>
			x.ExecutionMode == (int)ExecutionMode.Asynchronous &&
			x.ExecutionStage != (int)ExecutionStage.Post)
			.ToList();
		exceptions.AddRange(preOperationAsyncPlugins.Select(x => new Exception($"Plugin {x.Name}: Pre execution stages does not support asynchronous execution mode")));

		var preOperationWithPostImagesPlugins = pluginSteps
			.Where(x =>
			{
				var postImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PostImage);

				return
				(x.ExecutionStage == (int)ExecutionStage.Pre ||
				 x.ExecutionStage == (int)ExecutionStage.PreValidation) && postImages.Any();
			});
		exceptions.AddRange(preOperationWithPostImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Pre execution stages does not support post-images")));

		var associateDisassociateWithFilterPlugins = pluginSteps
			.Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
			.Where(x => x.FilteredAttributes != null);
		exceptions.AddRange(associateDisassociateWithFilterPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events can't have filtered attributes")));

		var associateDisassociateWithImagesPlugins = pluginSteps
			.Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
			.Where(x => x.PluginImages.Any());
		exceptions.AddRange(associateDisassociateWithImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events can't have images")));

		var associateDisassociateNotAllEntitiesPlugins = pluginSteps
			.Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
			.Where(x => x.LogicalName != "");
		exceptions.AddRange(associateDisassociateNotAllEntitiesPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events must target all entities")));

		var createWithPreImagesPlugins = pluginSteps
			.Where(x =>
			{
				var preImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PreImage);
				return x.EventOperation == "Create" && preImages.Any();
			});
		exceptions.AddRange(createWithPreImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Create events does not support pre-images")));

		var deleteWithPostImagesPLugins = pluginSteps
			.Where(x =>
			{
				var postImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PostImage);
				return x.EventOperation == "Delete" && postImages.Any();
			});
		exceptions.AddRange(deleteWithPostImagesPLugins.Select(x => new Exception($"Plugin {x.Name}: Delete events does not support post-images")));

        var userContextDoesNotExistPlugins = pluginReader.GetMissingUserContexts(pluginSteps);
		exceptions.AddRange(userContextDoesNotExistPlugins.Select(x => new Exception($"Plugin {x.Name}: Defined user context is not in the system")));

		if (exceptions.Count == 1) throw exceptions.First();
		else if (exceptions.Count > 1) throw new AggregateException("Some plugins can't be validated", exceptions);
	}
}
