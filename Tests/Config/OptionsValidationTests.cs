using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

public class OptionsValidationTests
{
    private static SharedOptions CreateSharedOptions(string profileName = "default") =>
        new SharedOptions(profileName);

    [Fact]
    public void PluginSyncValidator_ValidOptions_PassesValidation()
    {
        // Arrange - Create a test DLL file
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", "TestSolution", new List<SyncItem>
                    {
                        new PluginSyncItem(dllPath)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            validator.Validate(ConfigurationScope.PluginSync); // Should not throw
        }
        finally
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Fact]
    public void PluginSyncValidator_EmptyAssemblyPath_ThrowsValidationException()
    {
        // Arrange
        var config = new XrmSyncConfiguration(
            DryRun: false,
            LogLevel: LogLevel.Information,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new PluginSyncItem("")
                })
            }
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(
            Options.Create(config),
            Options.Create(CreateSharedOptions()));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
            () => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly path is required", exception.Message);
    }

    [Fact]
    public void PluginSyncValidator_NonExistentAssemblyFile_ThrowsValidationException()
    {
        // Arrange
        var config = new XrmSyncConfiguration(
            DryRun: false,
            LogLevel: LogLevel.Information,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new PluginSyncItem("nonexistent.dll")
                })
            }
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(
            Options.Create(config),
            Options.Create(CreateSharedOptions()));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
            () => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly file does not exist", exception.Message);
    }

    [Fact]
    public void PluginSyncValidator_WrongFileExtension_ThrowsValidationException()
    {
        // Arrange
        var config = new XrmSyncConfiguration(
            DryRun: false,
            LogLevel: LogLevel.Information,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new PluginSyncItem("testhost.exe")
                })
            }
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(
            Options.Create(config),
            Options.Create(CreateSharedOptions()));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
            () => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Assembly file must have a .dll extension", exception.Message);
    }

    [Fact]
    public void ProfileValidator_SolutionNameTooLong_ThrowsValidationException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", new string('a', 66), new List<SyncItem>
                    {
                        new PluginSyncItem(dllPath)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
                () => validator.Validate(ConfigurationScope.PluginSync));
            Assert.Contains("Solution name cannot exceed 65 characters", exception.Message);
        }
        finally
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Fact]
    public void PluginAnalysisValidator_ValidOptions_PassesValidation()
    {
        // Arrange - Create a test DLL file
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", "TestSolution", new List<SyncItem>
                    {
                        new PluginAnalysisSyncItem(dllPath, "contoso", true)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            validator.Validate(ConfigurationScope.PluginAnalysis); // Should not throw
        }
        finally
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Fact]
    public void PluginAnalysisValidator_EmptyPublisherPrefix_ThrowsValidationException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", "TestSolution", new List<SyncItem>
                    {
                        new PluginAnalysisSyncItem(dllPath, "", true)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
                () => validator.Validate(ConfigurationScope.PluginAnalysis));
            Assert.Contains("Publisher prefix is required", exception.Message);
        }
        finally
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Fact]
    public void PluginAnalysisValidator_InvalidPublisherPrefixTooShort_ThrowsValidationException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", "TestSolution", new List<SyncItem>
                    {
                        new PluginAnalysisSyncItem(dllPath, "a", true)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
                () => validator.Validate(ConfigurationScope.PluginAnalysis));
            Assert.Contains("Publisher prefix must be between 2 and 8 characters", exception.Message);
        }
        finally
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Theory]
    [InlineData("contoso")]
    [InlineData("ms")]
    [InlineData("contoso1")]
    [InlineData("abc123")]
    public void PluginAnalysisValidator_ValidPublisherPrefixes_PassValidation(string publisherPrefix)
    {
        // Arrange - Create a test DLL file
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", "TestSolution", new List<SyncItem>
                    {
                        new PluginAnalysisSyncItem(dllPath, publisherPrefix, true)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            validator.Validate(ConfigurationScope.PluginAnalysis); // Should not throw
        }
        finally
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Theory]
    [InlineData("Contoso")] // Starts with uppercase
    [InlineData("1contoso")] // Starts with number
    [InlineData("con-toso")] // Contains hyphen
    public void PluginAnalysisValidator_InvalidPublisherPrefixFormats_ThrowValidationException(string publisherPrefix)
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".dll"));
        var dllPath = Path.ChangeExtension(tempFile, ".dll");
        File.WriteAllText(dllPath, "test content");

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", "TestSolution", new List<SyncItem>
                    {
                        new PluginAnalysisSyncItem(dllPath, publisherPrefix, true)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
                () => validator.Validate(ConfigurationScope.PluginAnalysis));
            Assert.Contains("Publisher prefix must start with a lowercase letter", exception.Message);
        }
        finally
        {
            if (File.Exists(dllPath))
                File.Delete(dllPath);
        }
    }

    [Fact]
    public void WebresourceValidator_ValidOptions_PassesValidation()
    {
        // Arrange - Create a test directory
        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        try
        {
            var config = new XrmSyncConfiguration(
                DryRun: false,
                LogLevel: LogLevel.Information,
                CiMode: false,
                Profiles: new List<ProfileConfiguration>
                {
                    new("default", "TestSolution", new List<SyncItem>
                    {
                        new WebresourceSyncItem(tempDir)
                    })
                }
            );

            // Act & Assert
            var validator = new XrmSyncConfigurationValidator(
                Options.Create(config),
                Options.Create(CreateSharedOptions()));
            validator.Validate(ConfigurationScope.WebresourceSync); // Should not throw
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void WebresourceValidator_EmptyFolderPath_ThrowsValidationException()
    {
        // Arrange
        var config = new XrmSyncConfiguration(
            DryRun: false,
            LogLevel: LogLevel.Information,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new WebresourceSyncItem("")
                })
            }
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(
            Options.Create(config),
            Options.Create(CreateSharedOptions()));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
            () => validator.Validate(ConfigurationScope.WebresourceSync));
        Assert.Contains("Webresource root path is required", exception.Message);
    }

    [Fact]
    public void WebresourceValidator_NonExistentFolderPath_ThrowsValidationException()
    {
        // Arrange
        var config = new XrmSyncConfiguration(
            DryRun: false,
            LogLevel: LogLevel.Information,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new WebresourceSyncItem("C:\\NonExistentPath\\Webresources")
                })
            }
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(
            Options.Create(config),
            Options.Create(CreateSharedOptions()));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
            () => validator.Validate(ConfigurationScope.WebresourceSync));
        Assert.Contains("Webresource root path does not exist", exception.Message);
    }

    [Fact]
    public void Validator_ProfileNotFound_ThrowsValidationException()
    {
        // Arrange
        var config = new XrmSyncConfiguration(
            DryRun: false,
            LogLevel: LogLevel.Information,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>()
        );

        // Act & Assert
        var validator = new XrmSyncConfigurationValidator(
            Options.Create(config),
            Options.Create(CreateSharedOptions("nonexistent")));
        var exception = Assert.Throws<XrmSync.Model.Exceptions.OptionsValidationException>(
            () => validator.Validate(ConfigurationScope.PluginSync));
        Assert.Contains("Profile 'nonexistent' not found", exception.Message);
    }
}
