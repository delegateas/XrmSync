using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Infrastructure;
using XrmSync.Dataverse.Interfaces;

namespace Tests.Integration;

/// <summary>
/// Integration tests for IPluginAssemblyReader.
/// </summary>
public sealed class PluginAssemblyReaderTests : TestBase
{
	[Fact]
	public void GetPluginAssembly_ReturnsNull_WhenNoAssemblyInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("EmptySolution");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginAssemblyReader>();

		// Act
		var result = reader.GetPluginAssembly(solutionId, "NonExistentAssembly");

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void GetPluginAssembly_ReturnsAssemblyInfo_WhenAssemblyExistsInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("AssemblySolution");
		var assemblyId = Producer.ProducePluginAssembly("TestPlugin", "2.0.0.0", "abc123hash");
		Producer.ProduceSolutionComponent(solutionId, assemblyId, componentType: 91); // 91 = PluginAssembly

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginAssemblyReader>();

		// Act
		var result = reader.GetPluginAssembly(solutionId, "TestPlugin");

		// Assert
		Assert.NotNull(result);
		Assert.Equal(assemblyId, result.Id);
		Assert.Equal("TestPlugin", result.Name);
		Assert.Equal("2.0.0.0", result.Version);
		Assert.Equal("abc123hash", result.Hash);
	}

	[Fact]
	public void GetPluginAssembly_ReturnsNull_WhenAssemblyExistsButNotInSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("WrongSolution");
		Producer.ProducePluginAssembly("OrphanAssembly", "1.0.0.0");
		// Note: no solution component linking the assembly to the solution

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginAssemblyReader>();

		// Act
		var result = reader.GetPluginAssembly(solutionId, "OrphanAssembly");

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public void GetPluginAssembly_ReturnsNull_WhenAssemblyInDifferentSolution()
	{
		// Arrange
		var (solution1Id, _) = Producer.ProduceSolution("Solution1", "s1");
		var (solution2Id, _) = Producer.ProduceSolution("Solution2", "s2");
		var assemblyId = Producer.ProducePluginAssembly("SharedAssembly");
		Producer.ProduceSolutionComponent(solution1Id, assemblyId, componentType: 91);

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<IPluginAssemblyReader>();

		// Act
		var result = reader.GetPluginAssembly(solution2Id, "SharedAssembly");

		// Assert
		Assert.Null(result);
	}
}
