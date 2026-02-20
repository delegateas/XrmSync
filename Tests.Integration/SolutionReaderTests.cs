using Microsoft.Extensions.DependencyInjection;
using Tests.Integration.Infrastructure;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Exceptions;

namespace Tests.Integration;

/// <summary>
/// Integration tests for ISolutionReader.
/// </summary>
public sealed class SolutionReaderTests : TestBase
{
	[Fact]
	public void RetrieveSolution_ReturnsSolutionIdAndPrefix()
	{
		// Arrange
		var (solutionId, prefix) = Producer.ProduceSolution("TestSolution", "tst");

		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<ISolutionReader>();

		// Act
		var (retrievedSolutionId, retrievedPrefix) = reader.RetrieveSolution("TestSolution");

		// Assert
		Assert.Equal(solutionId, retrievedSolutionId);
		Assert.Equal("tst", retrievedPrefix);
	}

	[Fact]
	public void RetrieveSolution_ThrowsXrmSyncException_WhenSolutionDoesNotExist()
	{
		// Arrange
		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<ISolutionReader>();

		// Act & Assert
		Assert.Throws<XrmSyncException>(() => reader.RetrieveSolution("NonExistentSolution"));
	}

	[Fact]
	public void ConnectedHost_ReturnsExpectedValue()
	{
		// Arrange
		var sp = BuildServiceProvider();
		var reader = sp.GetRequiredService<ISolutionReader>();

		// Act
		var host = reader.ConnectedHost;

		// Assert
		Assert.Equal("https://test.crm.dynamics.com", host);
	}
}
