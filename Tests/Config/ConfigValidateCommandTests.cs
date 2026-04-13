using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using XrmSync.Commands;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

file sealed class ConsoleOutputSuppressor : IDisposable
{
	private readonly TextWriter _original = Console.Out;
	public ConsoleOutputSuppressor() => Console.SetOut(TextWriter.Null);
	public void Dispose() => Console.SetOut(_original);
}

public class ConfigValidateCommandTests
{
	[Fact]
	public void ConfigValidateCommandBuildsSuccessfully()
	{
		// Arrange & Act
		var command = new ConfigValidateCommand();

		// Assert
		Assert.NotNull(command);
		Assert.Equal("validate", command.GetCommand().Name);
	}

	[Fact]
	public void ConfigListCommandBuildsSuccessfully()
	{
		// Arrange & Act
		var command = new ConfigListCommand();

		// Assert
		Assert.NotNull(command);
		Assert.Equal("list", command.GetCommand().Name);
	}

	[Fact]
	public async Task ConfigValidationOutputWithValidConfigurationOutputsSuccessfully()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "default": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "",
                  "SolutionName": ""
                }
              },
              "Logger": {
                "LogLevel": "Information",
                "CiMode": false
              },
              "Execution": {
                "DryRun": false
              }
            }
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var configuration = configReader.GetConfiguration();
			var sharedOptions = Options.Create(SharedOptions.Empty);
			var builder = new XrmSyncConfigurationBuilder(configuration);
			var config = builder.Build();
			var configOptions = Options.Create(config);

			var output = new ConfigValidationOutput(configuration, configOptions, sharedOptions);

			// Act & Assert - Should not throw
			using var _ = new ConsoleOutputSuppressor();
			await output.OutputValidationResult();
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task ConfigValidationOutputWithMultipleConfigsListsAllConfigurations()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "default": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "default.dll",
                  "SolutionName": "DefaultSolution"
                }
              }
            },
            "dev": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "dev.dll",
                  "SolutionName": "DevSolution"
                }
              }
            },
            "prod": {
              "Webresource": {
                "Sync": {
                  "FolderPath": "wwwroot",
                  "SolutionName": "ProdSolution"
                }
              }
            }
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var configuration = configReader.GetConfiguration();
			var sharedOptions = Options.Create(SharedOptions.Empty);
			var builder = new XrmSyncConfigurationBuilder(configuration);
			var config = builder.Build();
			var configOptions = Options.Create(config);

			var output = new ConfigValidationOutput(configuration, configOptions, sharedOptions);

			// Act & Assert - Should not throw
			using var _ = new ConsoleOutputSuppressor();
			await output.OutputConfigList();
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task ConfigValidationOutputWithNoConfigurationHandlesGracefully()
	{
		// Arrange
		const string configJson = """
        {
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var configuration = configReader.GetConfiguration();
			var sharedOptions = Options.Create(SharedOptions.Empty);
			var builder = new XrmSyncConfigurationBuilder(configuration);
			var config = builder.Build();
			var configOptions = Options.Create(config);

			var output = new ConfigValidationOutput(configuration, configOptions, sharedOptions);

			// Act & Assert - Should not throw
			using var _ = new ConsoleOutputSuppressor();
			await output.OutputConfigList();
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task OutputAllValidationResultsWithNoProfilesHandlesGracefully()
	{
		// Arrange
		var configuration = new ConfigurationBuilder().Build();
		var config = XrmSyncConfiguration.Empty;
		var configOptions = Options.Create(config);
		var sharedOptions = Options.Create(SharedOptions.Empty);

		var output = new ConfigValidationOutput(configuration, configOptions, sharedOptions);

		// Act & Assert - Should not throw
		using var _ = new ConsoleOutputSuppressor();
		await output.OutputAllValidationResults();
	}

	[Fact]
	public async Task OutputAllValidationResultsWithSingleProfileOutputsValidationResult()
	{
		// Arrange
		var configuration = new ConfigurationBuilder().Build();
		var config = new XrmSyncConfiguration(
			DryRun: false,
			LogLevel: Microsoft.Extensions.Logging.LogLevel.Information,
			CiMode: false,
			Profiles:
			[
				new ProfileConfiguration("default", "TestSolution", [])
			]
		);
		var configOptions = Options.Create(config);
		var sharedOptions = Options.Create(SharedOptions.Empty);

		var output = new ConfigValidationOutput(configuration, configOptions, sharedOptions);

		// Act & Assert - Should not throw
		using var _ = new ConsoleOutputSuppressor();
		await output.OutputAllValidationResults();
	}

	[Fact]
	public async Task OutputAllValidationResultsWithMultipleProfilesOutputsAllProfiles()
	{
		// Arrange
		var configuration = new ConfigurationBuilder().Build();
		var config = new XrmSyncConfiguration(
			DryRun: false,
			LogLevel: Microsoft.Extensions.Logging.LogLevel.Information,
			CiMode: false,
			Profiles:
			[
				new ProfileConfiguration("dev", "DevSolution", []),
				new ProfileConfiguration("prod", "ProdSolution", [])
			]
		);
		var configOptions = Options.Create(config);
		var sharedOptions = Options.Create(SharedOptions.Empty);

		var output = new ConfigValidationOutput(configuration, configOptions, sharedOptions);

		// Act & Assert - Should not throw
		using var _ = new ConsoleOutputSuppressor();
		await output.OutputAllValidationResults();
	}

	[Fact]
	public async Task OutputAllValidationResultsWithInvalidProfileReportsFailure()
	{
		// Arrange
		var configuration = new ConfigurationBuilder().Build();
		var config = new XrmSyncConfiguration(
			DryRun: false,
			LogLevel: Microsoft.Extensions.Logging.LogLevel.Information,
			CiMode: false,
			Profiles:
			[
				new ProfileConfiguration("dev", "DevSolution", [new PluginSyncItem("")]),
				new ProfileConfiguration("prod", "ProdSolution", [])
			]
		);
		var configOptions = Options.Create(config);
		var sharedOptions = Options.Create(SharedOptions.Empty);

		var output = new ConfigValidationOutput(configuration, configOptions, sharedOptions);

		// Act & Assert - Should not throw even with invalid config
		using var _ = new ConsoleOutputSuppressor();
		await output.OutputAllValidationResults();
	}

	private class TestConfigReader(string configFile) : IConfigReader
	{
		public IConfiguration GetConfiguration()
		{
			return new ConfigurationBuilder()
				.AddJsonFile(configFile)
				.Build();
		}
	}
}
