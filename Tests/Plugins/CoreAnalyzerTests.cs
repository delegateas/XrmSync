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
	public void AnalyzeTypesWithAbstractPluginSkipsItSilently()
	{
		// Abstract types cannot be instantiated by Dataverse and are silently excluded
		var types = new[] { typeof(IPluginDefinition), typeof(AbstractPlugin) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithInterfacePluginSkipsItSilently()
	{
		// Interfaces are inherently abstract and must not be validated as concrete types
		var types = new[] { typeof(IPluginDefinition), typeof(IPluginExtension) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
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
	public void AnalyzeTypesWithAbstractAndNoCtorPluginReportsOnlyConcreteErrors()
	{
		// Abstract types are silently skipped; only concrete types missing a ctor are errors
		var types = new[] { typeof(IPluginDefinition), typeof(AbstractPlugin), typeof(NoCtorPlugin) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorPlugin", ex.InnerExceptions[0].Message);
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

	private interface IPluginExtension : IPluginDefinition { }

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
	public void AnalyzeTypesWithAbstractCustomApiSkipsItSilently()
	{
		// Abstract types cannot be instantiated by Dataverse and are silently excluded
		var types = new[] { typeof(ICustomApiDefinition), typeof(AbstractCustomApi) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
	}

	[Fact]
	public void AnalyzeTypesWithInterfaceCustomApiSkipsItSilently()
	{
		// Interfaces are inherently abstract and must not be validated as concrete types
		var types = new[] { typeof(ICustomApiDefinition), typeof(ICustomApiExtension) };

		var result = _analyzer.AnalyzeTypes(types, "new");

		Assert.Empty(result);
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
	public void AnalyzeTypesWithAbstractAndNoCtorCustomApiReportsOnlyConcreteErrors()
	{
		// Abstract types are silently skipped; only concrete types missing a ctor are errors
		var types = new[] { typeof(ICustomApiDefinition), typeof(AbstractCustomApi), typeof(NoCtorCustomApi) };

		var ex = Assert.Throws<AggregateException>(() => _analyzer.AnalyzeTypes(types, "new"));

		Assert.Single(ex.InnerExceptions);
		Assert.Contains("NoCtorCustomApi", ex.InnerExceptions[0].Message);
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

	private interface ICustomApiExtension : ICustomApiDefinition { }

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
