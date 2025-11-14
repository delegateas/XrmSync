using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

public class OptionsValidationTests
{
    [Fact]
    public void SyncOptionsValidatorValidOptionsPassesValidation()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: "TestSolution"
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
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Sync = options } }));
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
    public void SyncOptionsValidatorEmptyAssemblyPathThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "",
            SolutionName: "TestSolution"
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly path is required", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidatorEmptySolutionNameThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: ""
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Solution name is required", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidatorNonExistentAssemblyFileThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "nonexistent.dll",
            SolutionName: "TestSolution"
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly file does not exist", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidatorWrongFileExtensionThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "testhost.exe",
            SolutionName: "TestSolution"
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly file must have a .dll extension", exception.Message);
    }

    [Fact]
    public void SyncOptionsValidatorSolutionNameTooLongThrowsValidationException()
    {
        // Arrange
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: new string('a', 66) // 66 characters
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Solution name cannot exceed 65 characters", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidatorValidOptionsPassesValidation()
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
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
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
    public void AnalysisOptionsValidatorEmptyAssemblyPathThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "",
            PublisherPrefix: "contoso",
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Assembly path is required", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidatorEmptyPublisherPrefixThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "",
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix is required", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidatorInvalidPublisherPrefixTooShortThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "a", // Only 1 character
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must be between 2 and 8 characters", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidatorInvalidPublisherPrefixTooLongThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "toolongprefix", // More than 8 characters
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must be between 2 and 8 characters", exception.Message);
    }

    [Fact]
    public void AnalysisOptionsValidatorInvalidPublisherPrefixFormatThrowsValidationException()
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: "Con2so", // Contains uppercase and numbers not at end
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must start with a lowercase letter and contain only lowercase letters and numbers", exception.Message);
    }

    [Theory]
    [InlineData("contoso")]
    [InlineData("ms")]
    [InlineData("contoso1")]
    [InlineData("abc123")]
    public void AnalysisOptionsValidatorValidPublisherPrefixesPassValidation(string publisherPrefix)
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
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
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
    public void AnalysisOptionsValidatorInvalidPublisherPrefixFormatsThrowValidationException(string publisherPrefix)
    {
        // Arrange
        var options = new PluginAnalysisOptions(
            AssemblyPath: "test.dll",
            PublisherPrefix: publisherPrefix,
            PrettyPrint: true
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Plugin = PluginOptions.Empty with { Analysis = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.PluginAnalysis));
        Assert.Contains("Publisher prefix must start with a lowercase letter and contain only lowercase letters and numbers", exception.Message);
    }

    // Webresource validation tests start here
    [Fact]
    public void WebresourceSyncOptionsValidatorValidOptionsPassesValidation()
    {
        // Arrange
        // Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var options = new WebresourceSyncOptions(
            FolderPath: tempDir,
            SolutionName: "TestSolution"
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            validator.Validate(ConfigurationScope.WebresourceSync); // Should not throw
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorEmptyFolderPathThrowsValidationException()
    {
        // Arrange
        var options = new WebresourceSyncOptions(
            FolderPath: "",
            SolutionName: "TestSolution"
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
        Assert.Contains("Webresource root path is required", exception.Message);
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorNullFolderPathThrowsValidationException()
    {
        // Arrange
        var options = new WebresourceSyncOptions(
            FolderPath: null!,
            SolutionName: "TestSolution"
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
        Assert.Contains("Webresource root path is required", exception.Message);
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorWhitespaceFolderPathThrowsValidationException()
    {
        // Arrange
        var options = new WebresourceSyncOptions(
            FolderPath: "   ",
            SolutionName: "TestSolution"
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
        Assert.Contains("Webresource root path is required", exception.Message);
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorNonExistentFolderPathThrowsValidationException()
    {
        // Arrange
        var options = new WebresourceSyncOptions(
            FolderPath: "C:\\NonExistentPath\\Webresources",
            SolutionName: "TestSolution"
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
        Assert.Contains("Webresource root path does not exist", exception.Message);
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorEmptySolutionNameThrowsValidationException()
    {
        // Arrange
        // Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var options = new WebresourceSyncOptions(
            FolderPath: tempDir,
            SolutionName: ""
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
            Assert.Contains("Solution name is required", exception.Message);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorNullSolutionNameThrowsValidationException()
    {
        // Arrange
        // Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var options = new WebresourceSyncOptions(
            FolderPath: tempDir,
            SolutionName: null!
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
            Assert.Contains("Solution name is required", exception.Message);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorWhitespaceSolutionNameThrowsValidationException()
    {
        // Arrange
        // Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var options = new WebresourceSyncOptions(
            FolderPath: tempDir,
            SolutionName: "   "
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
            Assert.Contains("Solution name is required", exception.Message);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorSolutionNameTooLongThrowsValidationException()
    {
        // Arrange
        // Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var options = new WebresourceSyncOptions(
            FolderPath: tempDir,
            SolutionName: new string('a', 66) // 66 characters
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
            Assert.Contains("Solution name cannot exceed 65 characters", exception.Message);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Theory]
    [InlineData("ValidSolution")]
    [InlineData("My_Solution")]
    [InlineData("Solution123")]
    [InlineData("A")]
    [InlineData("AB")]
    public void WebresourceSyncOptionsValidatorValidSolutionNamesPassValidation(string solutionName)
    {
        // Arrange
        // Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var options = new WebresourceSyncOptions(
            FolderPath: tempDir,
            SolutionName: solutionName
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            validator.Validate(ConfigurationScope.WebresourceSync); // Should not throw
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorRelativeFolderPathPassesValidation()
    {
        // Arrange
        // Create a relative test directory
        var relativeDir = "TestWebresources";
        if (Directory.Exists(relativeDir))
            Directory.Delete(relativeDir, true);
        Directory.CreateDirectory(relativeDir);

        var options = new WebresourceSyncOptions(
            FolderPath: relativeDir,
            SolutionName: "TestSolution"
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            validator.Validate(ConfigurationScope.WebresourceSync); // Should not throw
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(relativeDir))
                Directory.Delete(relativeDir, true);
        }
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorDryRunFlagsPassValidation()
    {
        // Arrange
        // Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        var options = new WebresourceSyncOptions(
            FolderPath: tempDir,
            SolutionName: "TestSolution"
        );

        try
        {
            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
            validator.Validate(ConfigurationScope.WebresourceSync); // Should not throw
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WebresourceSyncOptionsValidatorMultipleValidationErrorsThrowsExceptionWithAllErrors()
    {
        // Arrange
        var options = new WebresourceSyncOptions(
            FolderPath: "", // Invalid: empty
            SolutionName: new string('a', 66) // Invalid: too long
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(Options.Create(XrmSyncConfiguration.Empty with { Webresource = WebresourceOptions.Empty with { Sync = options } }));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(() => validator.Validate(ConfigurationScope.WebresourceSync));
        
        // Verify both validation errors are present
        Assert.Contains("Webresource root path is required", exception.Message);
        Assert.Contains("Solution name cannot exceed 65 characters", exception.Message);
    }
}