using XrmSync.Analyzer.Analyzers.DAXIF;
using XrmSync.Analyzer;

// Type aliases matching the internal aliases used in DAXIFPluginAnalyzer and DAXIFCustomApiAnalyzer
using StepConfig = System.Tuple<string?, int, string?, string?>;
using ExtendedStepConfig = System.Tuple<int, int, string?, int, string?, string?>;
using ImageTuple = System.Tuple<string?, string?, int, string?>;
using MainCustomAPIConfig = System.Tuple<string?, bool, int, int, int, string?>;
using ExtendedCustomAPIConfig = System.Tuple<string?, string?, string?, bool, bool, string?, string?>;
using RequestParameterConfig = System.Tuple<string?, string?, string?, bool, bool, string?, int>;
using ResponsePropertyConfig = System.Tuple<string?, string?, string?, bool, string?, int>;

namespace Tests.Plugins;

public class DAXIFPluginAnalyzerTests
{
	private readonly DAXIFPluginAnalyzer _analyzer = new();

	[Fact]
	public void AnalyzeTypesWithEmptyTypeListReturnsEmpty()
	{
		var result = _analyzer.AnalyzeTypes([], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithNoPluginBaseTypeReturnsEmpty()
	{
		var result = _analyzer.AnalyzeTypes([typeof(string), typeof(int)], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithAbstractPluginSkipsItSilently()
	{
		// Abstract types cannot be instantiated by Dataverse and are silently excluded
		var types = new[] { typeof(Plugin), typeof(AbstractPlugin) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithPluginHavingNoPublicParameterlessConstructorThrowsAggregateException()
	{
		var types = new[] { typeof(Plugin), typeof(NoCtorPlugin) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorPlugin", ex.InnerExceptions[0].Message);
		Assert.Contains("public parameterless constructor", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithAbstractAndNoCtorPluginReportsOnlyConcreteErrors()
	{
		// Abstract types are silently skipped; only concrete types missing a ctor are errors
		var types = new[] { typeof(Plugin), typeof(AbstractPlugin), typeof(NoCtorPlugin) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorPlugin", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithPluginWithNoStepsIsExcluded()
	{
		// A plugin that registers no steps should be silently excluded
		var types = new[] { typeof(Plugin), typeof(EmptyPlugin) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithValidPluginWithStepsReturnsPluginDefinition()
	{
		var types = new[] { typeof(Plugin), typeof(ValidPluginWithSteps) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Single(result);
		Assert.Equal(typeof(ValidPluginWithSteps).FullName, result[0].Name);
	}

	// The DAXIF analyzer identifies the base type by the class name "Plugin"
	// and the presence of PluginProcessingStepConfigs().
	private class Plugin
	{
		public IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>
			PluginProcessingStepConfigs() => [];
	}

	private abstract class AbstractPlugin : Plugin { }

	private class NoCtorPlugin : Plugin
	{
		public NoCtorPlugin(string _) { }
	}

	private class EmptyPlugin : Plugin
	{
		public EmptyPlugin() { }
	}

	private class ValidPluginWithSteps : Plugin
	{
		public ValidPluginWithSteps() { }

		public new IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>
			PluginProcessingStepConfigs() => [
				Tuple.Create(
					Tuple.Create<string?, int, string?, string?>(typeof(ValidPluginWithSteps).FullName, 40, "Create", "account"),
					Tuple.Create<int, int, string?, int, string?, string?>(0, 0, null, 1, null, null),
					Enumerable.Empty<ImageTuple>())
			];
	}
}

public class DAXIFCustomApiAnalyzerTests
{
	private readonly DAXIFCustomApiAnalyzer _analyzer = new();

	[Fact]
	public void AnalyzeTypesWithEmptyTypeListReturnsEmpty()
	{
		var result = _analyzer.AnalyzeTypes([], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithNoCustomApiBaseTypeReturnsEmpty()
	{
		var result = _analyzer.AnalyzeTypes([typeof(string), typeof(int)], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithAbstractCustomApiSkipsItSilently()
	{
		// Abstract types cannot be instantiated by Dataverse and are silently excluded
		var types = new[] { typeof(CustomAPI), typeof(AbstractCustomApi) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithCustomApiHavingNoPublicParameterlessConstructorThrowsAggregateException()
	{
		var types = new[] { typeof(CustomAPI), typeof(NoCtorCustomApi) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorCustomApi", ex.InnerExceptions[0].Message);
		Assert.Contains("public parameterless constructor", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithAbstractAndNoCtorCustomApiReportsOnlyConcreteErrors()
	{
		// Abstract types are silently skipped; only concrete types missing a ctor are errors
		var types = new[] { typeof(CustomAPI), typeof(AbstractCustomApi), typeof(NoCtorCustomApi) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorCustomApi", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithValidCustomApiReturnsCustomApiDefinition()
	{
		var types = new[] { typeof(CustomAPI), typeof(ValidCustomApi) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Single(result);
		Assert.Equal("new_TestApi", result[0].UniqueName);
	}

	// The DAXIF analyzer identifies the base type by the class name "CustomAPI"
	// and the presence of GetCustomAPIConfig().
	private class CustomAPI
	{
		public Tuple<MainCustomAPIConfig, ExtendedCustomAPIConfig,
			IEnumerable<RequestParameterConfig>, IEnumerable<ResponsePropertyConfig>>
			GetCustomAPIConfig() =>
			Tuple.Create(
				Tuple.Create<string?, bool, int, int, int, string?>("TestApi", false, 0, 0, 0, null),
				Tuple.Create<string?, string?, string?, bool, bool, string?, string?>(
					null, null, null, false, false, null, null),
				Enumerable.Empty<RequestParameterConfig>(),
				Enumerable.Empty<ResponsePropertyConfig>()
			);
	}

	private abstract class AbstractCustomApi : CustomAPI { }

	private class NoCtorCustomApi : CustomAPI
	{
		public NoCtorCustomApi(string _) { }
	}

	private class ValidCustomApi : CustomAPI
	{
		public ValidCustomApi() { }
	}
}
