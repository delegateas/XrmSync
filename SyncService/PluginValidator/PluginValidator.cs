using DG.XrmPluginCore.Enums;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Exceptions;

namespace XrmSync.SyncService.PluginValidator;

internal record ValidationRule<T>(Func<IEnumerable<T>, IEnumerable<T>> ViolationSelector, string Message);

internal class PluginValidator(IPluginReader pluginReader) : IPluginValidator
{
    private static void Validate<T>(string prefix, IEnumerable<T> items, IEnumerable<ValidationRule<T>> rules, string aggregateException)
        where T : EntityBase
    {
        List<Exception> exceptions = [];
        foreach (var rule in rules)
        {
            var violatingItems = rule.ViolationSelector(items);
            exceptions.AddRange(violatingItems.Select(x => new ValidationException($"{prefix} {x.Name}: {rule.Message}")));
        }

        if (exceptions.Count == 1) throw exceptions[0];
        else if (exceptions.Count > 1) throw new AggregateException(aggregateException, exceptions);
    }

    public void Validate(List<PluginDefinition> pluginTypes)
    {
        var pluginSteps = pluginTypes.SelectMany(x => x.PluginSteps);
        var adSteps = pluginSteps.Where(x => x.EventOperation == nameof(EventOperation.Associate) || x.EventOperation == nameof(EventOperation.Disassociate));

        Validate("Plugin", pluginSteps, [
            new (s => s.Where(x => x.ExecutionMode == ExecutionMode.Asynchronous && x.ExecutionStage != ExecutionStage.PostOperation), "Pre execution stages does not support asynchronous execution mode"),
            new (s => s.Where(x => (x.ExecutionStage == ExecutionStage.PreOperation || x.ExecutionStage == ExecutionStage.PreValidation) && x.PluginImages.Any(image => image.ImageType == ImageType.PostImage)), "Pre execution stages does not support post-images"),
            new (_ => adSteps.Where(x => x.FilteredAttributes != null), "Associate/Disassociate events can't have filtered attributes"),
            new (_ => adSteps.Where(x => x.PluginImages.Any()), "Associate/Disassociate events can't have images"),
            new (_ => adSteps.Where(x => x.LogicalName != ""), "Associate/Disassociate events must target all entities"),
            new (s => s.Where(x => x.EventOperation == nameof(EventOperation.Create) && x.PluginImages.Any(image => image.ImageType == ImageType.PreImage)), "Create events does not support pre-images"),
            new (s => s.Where(x => x.EventOperation == nameof(EventOperation.Delete) && x.PluginImages.Any(image => image.ImageType == ImageType.PostImage)), "Delete events does not support post-images"),
            new (s => s.GroupBy(x => (x.PluginType, x.EventOperation, x.ExecutionStage, x.LogicalName)).Where(g => g.Count() > 1).Select(g => g.First()), "Multiple registrations on the same message, stage and entity are not allowed in the same plugin type"),
            new (_ => pluginReader.GetMissingUserContexts(pluginSteps), "Defined user context is not in the system")
        ], "Some plugins can't be validated");
    }

    public void Validate(List<CustomApiDefinition> customApis)
    {
        Validate("CustomAPI", customApis, [
            new (s => s.Where(x => x.BindingType != 0 && string.IsNullOrWhiteSpace(x.BoundEntityLogicalName)), "Bound Custom API must specify an entity type"),
            new (s => s.Where(x => x.BindingType == 0 && !string.IsNullOrWhiteSpace(x.BoundEntityLogicalName)), "Unbound Custom API cannot specify an entity type"),
        ], "Some custom apis can't be validated");
    }
}
