using XrmPluginCore;
using XrmPluginCore.Interfaces.CustomApi;
using XrmPluginCore.Interfaces.Plugin;
using XrmSync.Analyzer.Analyzers.XrmPluginCore;
using XrmSync.Analyzer;

namespace Tests.Plugins;

public class CorePluginAnalyzerTests
{
	private readonly CorePluginAnalyzer _analyzer = new();

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
	public void AnalyzeTypesWithOnlyBaseTypePresentReturnsEmpty()
	{
		// The base interface itself must not be analysed as a concrete plugin
		var result = _analyzer.AnalyzeTypes([typeof(IPluginDefinition)], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithAbstractPluginThrowsAggregateException()
	{
		var types = new[] { typeof(IPluginDefinition), typeof(AbstractPlugin) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("AbstractPlugin", ex.InnerExceptions[0].Message);
		Assert.Contains("abstract", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithPluginHavingNoPublicParameterlessConstructorThrowsAggregateException()
	{
		var types = new[] { typeof(IPluginDefinition), typeof(NoCtorPlugin) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorPlugin", ex.InnerExceptions[0].Message);
		Assert.Contains("public parameterless constructor", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithMultipleInvalidPluginsReportsAllErrors()
	{
		var types = new[] { typeof(IPluginDefinition), typeof(AbstractPlugin), typeof(NoCtorPlugin) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Equal(2, ex.InnerExceptions.Count);
	}

	[Fact]
	public void AnalyzeTypesWithValidPluginPassesValidation()
	{
		// A plugin with no step registrations is skipped by design — this test
		// verifies the type passes constructor validation without throwing.
		var types = new[] { typeof(IPluginDefinition), typeof(ValidPlugin) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
	}

	private abstract class AbstractPlugin : IPluginDefinition
	{
		public abstract IEnumerable<IPluginStepConfig> GetRegistrations();
	}

	private class NoCtorPlugin : IPluginDefinition
	{
		public NoCtorPlugin(string _) { }
		public IEnumerable<IPluginStepConfig> GetRegistrations() => [];
	}

	private class ValidPlugin : IPluginDefinition
	{
		public IEnumerable<IPluginStepConfig> GetRegistrations() => [];
	}
}

public class CoreCustomApiAnalyzerTests
{
	private readonly CoreCustomApiAnalyzer _analyzer = new();

	[Fact]
	public void AnalyzeTypesWithEmptyTypeListReturnsEmpty()
	{
		var result = _analyzer.AnalyzeTypes([], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithNoCustomApiBaseTypeReturnsEmpty()
	{
		// Tests the null guard added to CoreCustomApiAnalyzer
		var result = _analyzer.AnalyzeTypes([typeof(string), typeof(int)], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithOnlyBaseTypePresentReturnsEmpty()
	{
		// The base interface itself must not be analysed as a concrete custom API
		var result = _analyzer.AnalyzeTypes([typeof(ICustomApiDefinition)], "new");
		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithAbstractCustomApiThrowsAggregateException()
	{
		var types = new[] { typeof(ICustomApiDefinition), typeof(AbstractCustomApi) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("AbstractCustomApi", ex.InnerExceptions[0].Message);
		Assert.Contains("abstract", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithCustomApiHavingNoPublicParameterlessConstructorThrowsAggregateException()
	{
		var types = new[] { typeof(ICustomApiDefinition), typeof(NoCtorCustomApi) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorCustomApi", ex.InnerExceptions[0].Message);
		Assert.Contains("public parameterless constructor", ex.InnerExceptions[0].Message);
	}

	[Fact]
	public void AnalyzeTypesWithMultipleInvalidCustomApisReportsAllErrors()
	{
		var types = new[] { typeof(ICustomApiDefinition), typeof(AbstractCustomApi), typeof(NoCtorCustomApi) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Equal(2, ex.InnerExceptions.Count);
	}

	[Fact]
	public void AnalyzeTypesWithValidCustomApiPassesValidation()
	{
		// A custom API returning null from GetRegistration() is skipped by design — this
		// test verifies the type passes constructor validation without throwing.
		var types = new[] { typeof(ICustomApiDefinition), typeof(ValidCustomApi) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
	}

	private abstract class AbstractCustomApi : ICustomApiDefinition
	{
		public abstract ICustomApiConfig? GetRegistration();
	}

	private class NoCtorCustomApi : ICustomApiDefinition
	{
		public NoCtorCustomApi(string _) { }
		public ICustomApiConfig? GetRegistration() => null;
	}

	private class ValidCustomApi : ICustomApiDefinition
	{
		public ICustomApiConfig? GetRegistration() => null;
	}
}
