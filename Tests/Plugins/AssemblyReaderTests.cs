using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using XrmSync.Analyzer;
using XrmSync.Analyzer.Analyzers.DAXIF;
using XrmSync.Analyzer.Analyzers.XrmPluginCore;
using XrmSync.Analyzer.Extensions;
using XrmSync.Analyzer.Reader;
using XrmSync.Model;

namespace Tests.Plugins;

public class AssemblyReaderTests
{
	private readonly ILogger<LocalReader> logger = Substitute.For<ILogger<LocalReader>>();
	private readonly IAssemblyAnalyzer analyzer = Substitute.For<IAssemblyAnalyzer>();
	private readonly LocalReader assemblyReader;

	public AssemblyReaderTests()
	{
		assemblyReader = new LocalReader(logger, analyzer);
	}

	private static AssemblyAnalyzer CreateAnalyzer() => new(
		NullLogger<AssemblyAnalyzer>.Instance,
		[new DAXIFPluginAnalyzer(), new CorePluginAnalyzer()],
		[new DAXIFCustomApiAnalyzer(), new CoreCustomApiAnalyzer()]
	);

	[Fact]
	public async Task ReadAssemblyAsyncWithNullPathThrowsArgumentException()
	{
		// Arrange
		string? assemblyPath = null;

		// Act & Assert
		await Assert.ThrowsAsync<AnalysisException>(() => assemblyReader.ReadAssemblyAsync(assemblyPath!, "new", CancellationToken.None));
	}

	[Fact]
	public async Task ReadAssemblyAsyncWithEmptyPathThrowsAnalysisException()
	{
		// Arrange
		var assemblyPath = "";

		// Act & Assert
		await Assert.ThrowsAsync<AnalysisException>(() => assemblyReader.ReadAssemblyAsync(assemblyPath, "new", CancellationToken.None));
	}

	[Fact]
	public async Task ReadAssemblyAsyncWithWhitespacePathThrowsAnalysisException()
	{
		// Arrange
		var assemblyPath = "   ";

		// Act & Assert
		await Assert.ThrowsAsync<AnalysisException>(() => assemblyReader.ReadAssemblyAsync(assemblyPath, "new", CancellationToken.None));
	}

	[Fact]
	public async Task ReadAssemblyAsyncWithSamePathReturnsCachedResult()
	{
		// Arrange
		var assemblyPath = "test.dll";
		var expected = new AssemblyInfo("test") { Version = "1.0.0.0", Hash = "ABC" };
		analyzer.AnalyzeAssembly(assemblyPath, "new").Returns(expected);

		// Act
		var first = await assemblyReader.ReadAssemblyAsync(assemblyPath, "new", CancellationToken.None);
		var second = await assemblyReader.ReadAssemblyAsync(assemblyPath, "new", CancellationToken.None);

		// Assert
		Assert.Same(expected, first);
		Assert.Same(expected, second);
		analyzer.Received(1).AnalyzeAssembly(assemblyPath, "new");
	}

	[Theory]
	[InlineData("1-DAXIF")]
	[InlineData("2-Hybrid")]
	[InlineData("3-XrmPluginCore")]
	[InlineData("4-Full-DAXIF")]
	[Trait("Category", "AssemblyAnalyzer")]
	public void ReadAssemblyAsyncCanReadAssembly(string sampleFolder)
	{
		// Arange
		var assemblyPath = SamplePath(sampleFolder);

		// Act
		var assemblyInfo = CreateAnalyzer().AnalyzeAssembly(assemblyPath, "new");

		// Assert
		Assert.NotNull(assemblyInfo);
		Assert.Equal("SamplePlugins", assemblyInfo.Name);
		Assert.Equal(Guid.Empty, assemblyInfo.Id);
		Assert.Equal("1.0.0.0", assemblyInfo.Version);
		Assert.Equal(Path.GetFullPath(assemblyPath), assemblyInfo.DllPath);
		assemblyInfo.Plugins.ForEach(plugin =>
		{
			Assert.NotNull(plugin);
			Assert.NotEmpty(plugin.Name);
			Assert.Equal(Guid.Empty, plugin.Id);
			Assert.NotEmpty(plugin.PluginSteps);
			plugin.PluginSteps.ForEach(step => Assert.NotEmpty(step.Name));
		});

		assemblyInfo.CustomApis.ForEach(customApi =>
		{
			Assert.NotNull(customApi);
			Assert.NotEmpty(customApi.Name);
			Assert.NotEmpty(customApi.DisplayName);
			Assert.StartsWith("new_", customApi.UniqueName);
			Assert.Equal(Guid.Empty, customApi.Id);
		});
	}

	[Theory]
	[InlineData("1-DAXIF")]
	[InlineData("2-Hybrid")]
	[InlineData("3-XrmPluginCore")]
	[InlineData("4-Full-DAXIF")]
	[Trait("Category", "AssemblyAnalyzer")]
	public void AnalyzingAnAssemblyUnloadsItsLoadContext(string sampleFolder)
	{
		// Arrange
		var assemblyPath = Path.GetFullPath(SamplePath(sampleFolder));
		var bytes = File.ReadAllBytes(assemblyPath);
		var pluginAnalyzer = new CorePluginAnalyzer();
		var daxifAnalyzer = new DAXIFPluginAnalyzer();

		// Act - mirrors what AssemblyAnalyzer does, including instantiating the plugin types
		var result = IsolatedAssemblyLoader.Run(assemblyPath, bytes, assembly =>
		{
			var types = assembly.GetLoadableTypes();
			return daxifAnalyzer.AnalyzeTypes(types, "new").Count + pluginAnalyzer.AnalyzeTypes(types, "new").Count;
		});

		// Assert
		Assert.True(result.Value > 0, "Expected the sample assembly to contain plugins");
		Assert.True(result.Unloaded, "The plugin load context was still referenced after analysis");
	}

	[Fact]
	[Trait("Category", "AssemblyAnalyzer")]
	public void AnalyzingAnAssemblyDoesNotLockTheFile()
	{
		// Arrange
		var assemblyPath = Path.GetFullPath(SamplePath("1-DAXIF"));

		// Act
		CreateAnalyzer().AnalyzeAssembly(assemblyPath, "new");

		// Assert - a rebuild must be able to overwrite the DLL while XrmSync is running
		using var stream = File.Open(assemblyPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
		Assert.True(stream.CanWrite);
	}

	private static string SamplePath(string sampleFolder)
	{
#if DEBUG
		return $"../../../../Samples/{sampleFolder}/bin/Debug/net462/SamplePlugins.dll";
#else
		return $"../../../../Samples/{sampleFolder}/bin/Release/net462/SamplePlugins.dll";
#endif
	}
}
