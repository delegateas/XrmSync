using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace Tests;

public class OptionsValidationTests
{
    [Fact]
    public void SyncOptionsValidator_ValidOptions_PassesValidation()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: "TestSolution",
            LogLevel: LogLevel.Information,
            DryRun: false
        );

        // Create a test DLL file
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        options = options with { AssemblyPath = dllPath };

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(options, null!))));
            validator.Validate(ConfigurationScope.PluginSync); // Should not throw
        }
        finally
        {
            // Cleanup
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Fact]
    public void SyncOptionsValidator_EmptyAssemblyPath_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "",
            SolutionName: "TestSolution",
            LogLevel: LogLevel.Information,
            DryRun: false
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(options, null!))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly path is required", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidator_EmptySolutionName_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: "",
            LogLevel: LogLevel.Information,
            DryRun: false
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(options, null!))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Solution name is required", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidator_NonExistentAssemblyFile_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "nonexistent.dll",
            SolutionName: "TestSolution",
            LogLevel: LogLevel.Information,
            DryRun: false
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(options, null!))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly file does not exist", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidator_WrongFileExtension_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "test.exe",
            SolutionName: "TestSolution",
            LogLevel: LogLevel.Information,
            DryRun: false
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(options, null!))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly file must have a .dll extension", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidator_SolutionNameTooLong_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: new string('a', 66), // 66 characters
            LogLevel: LogLevel.Information,
            DryRun: false
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(options, null!))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Solution name cannot exceed 65 characters", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidator_ValidOptions_PassesValidation()
    {
        // Arrange

        // Create a test DLL file
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        var options = new PluginAnalysisOptions(
            AssemblyPath: dllPath,
            PublisherPrefix: "contoso",
            PrettyPrint: true
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
            validator.Validate(ConfigurationScope.PluginAnalysis); // Should not throw
        }
        finally
        {
            // Cleanup
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Fact]
    public void AnalysisOptionsValidator_EmptyAssemblyPath_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "",
            PublisherPrefix: "contoso",
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Assembly path is required", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidator_EmptyPublisherPrefix_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "",
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix is required", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidator_InvalidPublisherPrefixTooShort_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "a", // Only 1 character
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must be between 2 and 8 characters", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidator_InvalidPublisherPrefixTooLong_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "toolongprefix", // More than 8 characters
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must be between 2 and 8 characters", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidator_InvalidPublisherPrefixFormat_ThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "Con2so", // Contains uppercase and numbers not at end
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must start with a lowercase letter and contain only lowercase letters and numbers", exception.Message);
    }

    [Theory]
    [InlineData("contoso")]
    [InlineData("ms")]
    [InlineData("contoso1")]
    [InlineData("abc123")]
    public void AnalysisOptionsValidator_ValidPublisherPrefixes_PassValidation(string publisherPrefix)
    {
        // Arrange

        // Create a test DLL file
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        var options = new PluginAnalysisOptions(
            AssemblyPath: dllPath,
            PublisherPrefix: publisherPrefix,
            PrettyPrint: true
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
            validator.Validate(ConfigurationScope.PluginAnalysis); // Should not throw
        }
        finally
        {
            // Cleanup
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Theory]
    [InlineData("Contoso")] // Starts with uppercase
    [InlineData("1contoso")] // Starts with number
    [InlineData("con-toso")] // Contains hyphen
    [InlineData("con_toso")] // Contains underscore
    [InlineData("con.toso")] // Contains dot
    public void AnalysisOptionsValidator_InvalidPublisherPrefixFormats_ThrowValidationException(string publisherPrefix)
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: publisherPrefix,
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create<XrmSyncConfiguration>(new(new(null!, options))));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must start with a lowercase letter and contain only lowercase letters and numbers", exception.Message);
    }
}