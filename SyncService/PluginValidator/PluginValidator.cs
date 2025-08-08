using DG.XrmPluginCore.Enums;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Exceptions;

namespace XrmSync.SyncService.PluginValidator;

internal class PluginValidator(IPluginReader pluginReader) : IPluginValidator
{
    public void Validate(List<PluginDefinition> pluginTypes)
    {
        List<Exception> exceptions = [];
        var pluginSteps = pluginTypes.SelectMany(x => x.PluginSteps);
        var preOperationAsyncPlugins = pluginSteps
            .Where(x =>
            x.ExecutionMode == ExecutionMode.Asynchronous &&
            x.ExecutionStage != ExecutionStage.PostOperation)
            .ToList();
        exceptions.AddRange(preOperationAsyncPlugins.Select(x => new ValidationException($"Plugin {x.Name}: Pre execution stages does not support asynchronous execution mode")));

        var preOperationWithPostImagesPlugins = pluginSteps
            .Where(x =>
            {
                var postImages = x.PluginImages.Where(image => image.ImageType == ImageType.PostImage);

                return
                (x.ExecutionStage == ExecutionStage.PreOperation ||
                 x.ExecutionStage == ExecutionStage.PreValidation) && postImages.Any();
            });
        exceptions.AddRange(preOperationWithPostImagesPlugins.Select(x => new ValidationException($"Plugin {x.Name}: Pre execution stages does not support post-images")));

        var associateDisassociateWithFilterPlugins = pluginSteps
            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
            .Where(x => x.FilteredAttributes != null);
        exceptions.AddRange(associateDisassociateWithFilterPlugins.Select(x => new ValidationException($"Plugin {x.Name}: Associate/Disassociate events can't have filtered attributes")));

        var associateDisassociateWithImagesPlugins = pluginSteps
            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
            .Where(x => x.PluginImages.Any());
        exceptions.AddRange(associateDisassociateWithImagesPlugins.Select(x => new ValidationException($"Plugin {x.Name}: Associate/Disassociate events can't have images")));

        var associateDisassociateNotAllEntitiesPlugins = pluginSteps
            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
            .Where(x => x.LogicalName != "");
        exceptions.AddRange(associateDisassociateNotAllEntitiesPlugins.Select(x => new ValidationException($"Plugin {x.Name}: Associate/Disassociate events must target all entities")));

        var createWithPreImagesPlugins = pluginSteps
            .Where(x =>
            {
                var preImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PreImage);
                return x.EventOperation == "Create" && preImages.Any();
            });
        exceptions.AddRange(createWithPreImagesPlugins.Select(x => new ValidationException($"Plugin {x.Name}: Create events does not support pre-images")));

        var deleteWithPostImagesPLugins = pluginSteps
            .Where(x =>
            {
                var postImages = x.PluginImages.Where(image => image.ImageType == ImageType.PostImage);
                return x.EventOperation == "Delete" && postImages.Any();
            });
        exceptions.AddRange(deleteWithPostImagesPLugins.Select(x => new ValidationException($"Plugin {x.Name}: Delete events does not support post-images")));

        var stepsGroupedByMessageStageAndEntity =
            pluginSteps.GroupBy(x => (x.PluginTypeName, x.EventOperation, x.ExecutionStage, x.LogicalName))
            .Where(g => g.Count() > 1)
            .Select(g => g.First())
            .ToList();
        exceptions.AddRange(stepsGroupedByMessageStageAndEntity.Select(x => new ValidationException($"Plugin {x.Name}: Multiple registrations on the same message, stage and entity are not allowed")));

        var userContextDoesNotExistPlugins = pluginReader.GetMissingUserContexts(pluginSteps);
        exceptions.AddRange(userContextDoesNotExistPlugins.Select(x => new ValidationException($"Plugin {x.Name}: Defined user context is not in the system")));

        if (exceptions.Count == 1) throw exceptions.First();
        else if (exceptions.Count > 1) throw new AggregateException("Some plugins can't be validated", exceptions);
    }
}
