using XrmPluginCore.Enums;

namespace XrmSync.Analyzer.Analyzers;

internal abstract class Analyzer
{
	protected static IEnumerable<Type> ValidateCandidates(IEnumerable<Type> candidates, string entityKind)
	{
		var list = candidates.ToList();
		var errors = new List<AnalysisException>();

		foreach (var t in list)
		{
			if (t.IsAbstract) // covers both abstract classes and interfaces — skip silently
				continue;
			if (t.GetConstructor(Type.EmptyTypes) == null)
				errors.Add(new AnalysisException($"The {entityKind} '{t.Name}' does not have a public parameterless constructor and is therefore not valid. The {entityKind} will not be synchronized"));
		}

		if (errors.Count > 0)
			throw new AggregateException(errors);

		return list.Where(t => !t.IsAbstract);
	}

	protected static T? GetRegistrationFromType<T>(string methodName, Type pluginType) where T : class
	{
		var getRegistrationMethod = pluginType.GetMethod(methodName)
			?? throw new AnalysisException($"Type {pluginType.FullName} does not have a {methodName} method");

		var instance = Activator.CreateInstance(pluginType)
			?? throw new AnalysisException($"Failed to create instance of type {pluginType.FullName}");

		return getRegistrationMethod.Invoke(instance, null) as T;
	}

	protected static string StepName(string className, ExecutionMode executionMode, ExecutionStage executionStage, string eventOperation, string? entityLogicalName)
	{
		var entity = string.IsNullOrEmpty(entityLogicalName) ? "any Entity" : entityLogicalName;

		var executionModeName = executionMode.ToString();
		var executionStageName = executionStage switch
		{
			ExecutionStage.PreValidation => "PreValidation",
			ExecutionStage.PreOperation => "Pre",
			ExecutionStage.PostOperation => "Post",
			_ => "Unknown"
		};

		return $"{className}: {executionModeName} {executionStageName} {eventOperation} of {entity}";
	}
}
