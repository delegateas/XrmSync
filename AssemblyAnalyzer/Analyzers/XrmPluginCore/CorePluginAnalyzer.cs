using XrmPluginCore;
using XrmPluginCore.Interfaces.Plugin;
using System.Collections;
using System.Linq.Expressions;
using XrmSync.Model.Plugin;

namespace XrmSync.Analyzer.Analyzers.XrmPluginCore;

internal class CorePluginAnalyzer : CoreAnalyzer, IAnalyzer<PluginDefinition>
{
	public List<PluginDefinition> AnalyzeTypes(IEnumerable<Type> types, string prefix)
	{
		var typeNames = types.Where(t => (t.FullName ?? string.Empty).StartsWith("XrmPluginCore")).Select(t => t.FullName).ToList();

		var pluginBaseType = types.FirstOrDefault(t => t.FullName == typeof(IPluginDefinition).FullName);
		if (pluginBaseType is null)
			return [];

		var validTypes = types
			.Where(t => t.IsAssignableTo(pluginBaseType) && t.GetConstructor(Type.EmptyTypes) != null && !t.IsAbstract);

		return [.. AnalyzeTypesInner(validTypes)];
	}

	private static IEnumerable<PluginDefinition> AnalyzeTypesInner(IEnumerable<Type> types)
	{
		foreach (var pluginType in types)
		{
			var pluginDefinitionName = pluginType.FullName ?? string.Empty;
			var steps = GetPluginSteps(pluginType, pluginDefinitionName);

			if (!steps.Any())
			{
				continue;
			}

			yield return new PluginDefinition(pluginDefinitionName)
			{
				PluginSteps = [.. steps]
			};
		}
	}

	private static IEnumerable<Step> GetPluginSteps(Type pluginType, string pluginDefinitionName)
	{
		var registrations = GetRegistrationFromType<IEnumerable>(nameof(IPluginDefinition.GetRegistrations), pluginType)?.Cast<object>() ?? [];

		return registrations
			.Select(r => ConvertRegistrationToStep(r, pluginDefinitionName));
	}

	private static Step ConvertRegistrationToStep(object registration, string pluginDefinitionName)
	{
		// Use type-safe reflection to safely extract properties
		var entityLogicalName = GetRegistrationValue(registration, x => x.EntityLogicalName) ?? string.Empty;
		var executionMode = GetRegistrationValue(registration, x => x.ExecutionMode);
		var executionStage = GetRegistrationValue(registration, x => x.ExecutionStage);
		var eventOperation = GetRegistrationValue<object>(registration, x => x.EventOperation)?.ToString() ?? string.Empty;
		var deployment = GetRegistrationValue(registration, x => x.Deployment);
		var executionOrder = GetRegistrationValue(registration, x => x.ExecutionOrder);
		var filteredAttributes = GetRegistrationValue(registration, x => x.FilteredAttributes) ?? string.Empty;
		var impersonatingUserId = GetRegistrationValue(registration, x => x.ImpersonatingUserId);
		var asyncAutoDelete = GetRegistrationValue(registration, x => x.AsyncAutoDelete);
		var imageSpecs = GetRegistrationValue<IEnumerable>(registration, x => x.ImageSpecifications) ?? Enumerable.Empty<object>();

		var stepName = StepName(pluginDefinitionName, executionMode, executionStage, eventOperation, entityLogicalName);

		return new Step(stepName)
		{
			ExecutionStage = executionStage,
			EventOperation = eventOperation,
			LogicalName = entityLogicalName,
			Deployment = deployment,
			ExecutionMode = executionMode,
			ExecutionOrder = executionOrder,
			FilteredAttributes = filteredAttributes,
			UserContext = impersonatingUserId.GetValueOrDefault(),
			AsyncAutoDelete = asyncAutoDelete,
			PluginImages = [.. imageSpecs.Cast<object>().Select(i => ConvertImageSpecification(i))]
		};
	}

	private static Image ConvertImageSpecification(object imageSpec) => new(GetImageValue(imageSpec, x => x.ImageName) ?? string.Empty)
	{
		ImageType = GetImageValue(imageSpec, x => x.ImageType),
		Attributes = GetImageValue(imageSpec, x => x.Attributes) ?? string.Empty,
		EntityAlias = GetImageValue(imageSpec, x => x.EntityAlias) ?? string.Empty
	};

	private static T? GetRegistrationValue<T>(object obj, Expression<Func<IPluginStepConfig, T>> propertyExpression) =>
		GetPropertyValue(obj, propertyExpression);

	private static T? GetImageValue<T>(object obj, Expression<Func<IImageSpecification, T>> propertyExpression) =>
		GetPropertyValue(obj, propertyExpression);
}
