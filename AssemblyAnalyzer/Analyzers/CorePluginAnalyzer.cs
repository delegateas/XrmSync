using DG.XrmPluginCore;
using DG.XrmPluginCore.Enums;
using DG.XrmPluginCore.Interfaces.Plugin;
using System.Collections;
using System.Linq.Expressions;
using XrmSync.Model;
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
            .SelectMany(t =>
            {
                return GetPluginSteps(t)
                    .GroupBy(s => s.PluginTypeName)
                    .Select(g => new PluginDefinition
                    {
                        Name = g.Key,
                        PluginSteps = [.. g]
                    });
            })];
    }

    private static IEnumerable<Step> GetPluginSteps(Type pluginType)
    {
        var plugin = Activator.CreateInstance(pluginType) ?? throw new AnalysisException($"Failed to create instance of type {pluginType.FullName}");

        // Use reflection to call GetRegistrations() method safely
        const string GetRegistrations = nameof(IPluginDefinition.GetRegistrations);
        var getRegistrationsMethod = pluginType.GetMethod(GetRegistrations)
            ?? throw new AnalysisException($"Type {pluginType.FullName} does not have a {GetRegistrations} method");
        
        var registrations = getRegistrationsMethod.Invoke(plugin, null) as IEnumerable
            ?? throw new AnalysisException($"{GetRegistrations}() returned null for type {pluginType.FullName}");
        
        return registrations.Cast<object>().Select(r => ConvertRegistrationToStep(r, pluginType));
    }

    private static Step ConvertRegistrationToStep(object registration, Type pluginType)
    {        
        // Use type-safe reflection to safely extract properties
        var entityLogicalName = GetRegistrationValue(registration, x => x.EntityLogicalName) ?? string.Empty;
        var executionMode = ConvertEnumToInt(GetRegistrationValue<object>(registration, x => x.ExecutionMode));
        var executionStage = ConvertEnumToInt(GetRegistrationValue<object>(registration, x => x.ExecutionStage));
        var eventOperation = GetRegistrationValue<object>(registration, x => x.EventOperation)?.ToString() ?? string.Empty;
        var deployment = ConvertEnumToInt(GetRegistrationValue<object>(registration, x => x.Deployment));
        var executionOrder = GetRegistrationValue(registration, x => x.ExecutionOrder);
        var filteredAttributes = GetRegistrationValue(registration, x => x.FilteredAttributes) ?? string.Empty;
        var impersonatingUserId = GetRegistrationValue(registration, x => x.ImpersonatingUserId);
        var asyncAutoDelete = GetRegistrationValue(registration, x => x.AsyncAutoDelete);
        var imageSpecs = GetRegistrationValue<IEnumerable>(registration, x => x.ImageSpecifications) ?? Enumerable.Empty<object>();

        var entity = string.IsNullOrEmpty(entityLogicalName) ? "any Entity" : entityLogicalName;
        var stepName = $"{pluginType.Name}: {Enum.GetName(typeof(Model.ExecutionMode), executionMode)} {Enum.GetName(typeof(Model.ExecutionStage), executionStage)} {eventOperation} of {entity}";

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
            ImageType = ConvertEnumToInt(GetImageValue<object>(imageSpec, x => x.ImageType)),
            Attributes = GetImageValue(imageSpec, x => x.Attributes) ?? string.Empty,
            EntityAlias = GetImageValue(imageSpec, x => x.EntityAlias) ?? string.Empty,
            PluginStepName = stepName
        };
    }

    private static T? GetRegistrationValue<T>(object obj, Expression<Func<IPluginStepConfig, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);

    private static T? GetImageValue<T>(object obj, Expression<Func<IImageSpecification, T>> propertyExpression) =>
        GetPropertyValue(obj, propertyExpression);
}
