using DG.XrmPluginCore;
using DG.XrmPluginCore.Interfaces.Plugin;
using System.Collections;
using System.Linq.Expressions;
using XrmSync.Model.Plugin;

namespace XrmSync.AssemblyAnalyzer.Analyzers;

internal class CorePluginAnalyzer : CoreAnalyzer, IPluginAnalyzer
{
    public List<PluginDefinition> GetPluginDefinitions(IEnumerable<Type> types)
    {
        var pluginBaseType = types.FirstOrDefault(t => t.FullName == typeof(IPluginDefinition).FullName);
        if (pluginBaseType is null) return [];

        var validTypes = types
            .Where(t => t.IsAssignableTo(pluginBaseType) && t.GetConstructor(Type.EmptyTypes) != null && !t.IsAbstract);

        return [.. validTypes
            .SelectMany(t => GetPluginSteps(t)
                    .GroupBy(s => s.PluginTypeName)
                    .Select(g => new PluginDefinition
                    {
                        Name = g.Key,
                        PluginSteps = [.. g]
                    }))];
    }

    private static IEnumerable<Step> GetPluginSteps(Type pluginType)
    {
        return GetRegistrationFromType<IEnumerable>(nameof(IPluginDefinition.GetRegistrations), pluginType)
            .Cast<object>()
            .Select(r => ConvertRegistrationToStep(r, pluginType));
    }

    private static Step ConvertRegistrationToStep(object registration, Type pluginType)
    {        
        // Use type-safe reflection to safely extract properties
        var entityLogicalName = GetRegistrationValue(registration, x => x.EntityLogicalName) ?? string.Empty;
        var executionMode = GetRegistrationEnum(registration, x => x.ExecutionMode);
        var executionStage = GetRegistrationEnum(registration, x => x.ExecutionStage);
        var eventOperation = GetRegistrationValue<object>(registration, x => x.EventOperation)?.ToString() ?? string.Empty;
        var deployment = GetRegistrationEnum(registration, x => x.Deployment);
        var executionOrder = GetRegistrationValue(registration, x => x.ExecutionOrder);
        var filteredAttributes = GetRegistrationValue(registration, x => x.FilteredAttributes) ?? string.Empty;
        var impersonatingUserId = GetRegistrationValue(registration, x => x.ImpersonatingUserId);
        var asyncAutoDelete = GetRegistrationValue(registration, x => x.AsyncAutoDelete);
        var imageSpecs = GetRegistrationValue<IEnumerable>(registration, x => x.ImageSpecifications) ?? Enumerable.Empty<object>();

        var stepName = StepName(pluginType.FullName ?? string.Empty, executionMode, executionStage, eventOperation, entityLogicalName);

        return new Step
        {
            Name = stepName,
            PluginTypeName = pluginType.FullName ?? string.Empty,
            ExecutionStage = executionStage,
            EventOperation = eventOperation,
            LogicalName = entityLogicalName,
            Deployment = deployment,
            ExecutionMode = executionMode,
            ExecutionOrder = executionOrder,
            FilteredAttributes = filteredAttributes,
            UserContext = impersonatingUserId.GetValueOrDefault(),
            AsyncAutoDelete = asyncAutoDelete,
            PluginImages = [.. imageSpecs.Cast<object>().Select(i => ConvertImageSpecification(i, stepName))]
        };
    }

    private static Image ConvertImageSpecification(object imageSpec, string stepName)
    {        
        return new Image
        {
            Name = GetImageValue(imageSpec, x => x.ImageName) ?? string.Empty,
            ImageType = GetEnumIntValue<IImageSpecification, DG.XrmPluginCore.Enums.ImageType>(imageSpec, x => x.ImageType),
            Attributes = GetImageValue(imageSpec, x => x.Attributes) ?? string.Empty,
            EntityAlias = GetImageValue(imageSpec, x => x.EntityAlias) ?? string.Empty,
            PluginStepName = stepName
        };
    }

    private static int GetRegistrationEnum<TENum>(object obj, Expression<Func<IPluginStepConfig, TENum>> propertyExpression)
        where TENum : Enum => GetEnumIntValue(obj, propertyExpression);

    private static T? GetRegistrationValue<T>(object obj, Expression<Func<IPluginStepConfig, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);

    private static T? GetImageValue<T>(object obj, Expression<Func<IImageSpecification, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);
}
