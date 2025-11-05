using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using XrmSync.Commands;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Options;

internal class ConfigValidationOutput(
    IConfiguration configuration,
    IOptions<XrmSyncConfiguration> configOptions,
    IOptions<SharedOptions> sharedOptions) : IConfigValidationOutput
{
    public Task OutputValidationResult(CancellationToken cancellationToken = default)
    {
        var configName = sharedOptions.Value.ConfigName;
        var configSource = GetConfigurationSource();

        Console.WriteLine($"Configuration: '{configName}' (from {configSource})");
        Console.WriteLine();

        var config = configOptions.Value;
        var allValid = true;

        // Validate and display Plugin Sync Configuration
        allValid &= OutputSectionValidation(
            "Plugin Sync Configuration",
            ConfigurationScope.PluginSync,
            () =>
            {
                Console.WriteLine($"  Assembly Path: {config.Plugin.Sync.AssemblyPath}");
                Console.WriteLine($"  Solution Name: {config.Plugin.Sync.SolutionName}");
            });

        // Validate and display Plugin Analysis Configuration
        allValid &= OutputSectionValidation(
            "Plugin Analysis Configuration",
            ConfigurationScope.PluginAnalysis,
            () =>
            {
                Console.WriteLine($"  Assembly Path: {config.Plugin.Analysis.AssemblyPath}");
                Console.WriteLine($"  Publisher Prefix: {config.Plugin.Analysis.PublisherPrefix}");
                Console.WriteLine($"  Pretty Print: {config.Plugin.Analysis.PrettyPrint}");
            });

        // Validate and display Webresource Sync Configuration
        allValid &= OutputSectionValidation(
            "Webresource Sync Configuration",
            ConfigurationScope.WebresourceSync,
            () =>
            {
                Console.WriteLine($"  Folder Path: {config.Webresource.Sync.FolderPath}");
                Console.WriteLine($"  Solution Name: {config.Webresource.Sync.SolutionName}");
            });

        // Display Logger Configuration (always valid)
        Console.WriteLine("✓ Logger Configuration");
        Console.WriteLine($"  Log Level: {config.Logger.LogLevel}");
        Console.WriteLine($"  CI Mode: {config.Logger.CiMode}");
        Console.WriteLine();

        // Display Execution Configuration (always valid)
        Console.WriteLine("✓ Execution Configuration");
        Console.WriteLine($"  Dry Run: {config.Execution.DryRun}");
        Console.WriteLine();

        // Display available commands based on configuration
        var availableCommands = GetAvailableCommands();
        if (availableCommands.Count != 0)
        {
            Console.WriteLine($"Available Commands: {string.Join(", ", availableCommands)}");
            Console.WriteLine();
        }

        // Final validation status
        if (allValid)
        {
            Console.WriteLine("Validation: PASSED");
        }
        else
        {
            Console.WriteLine("Validation: FAILED - See errors above");
        }

        return Task.CompletedTask;
    }

    public Task OutputConfigList(CancellationToken cancellationToken = default)
    {
        var xrmSyncSection = configuration.GetSection(XrmSyncConfigurationBuilder.SectionName.XrmSync);

        if (!xrmSyncSection.Exists())
        {
            Console.WriteLine("No XrmSync configurations found in appsettings.json");
            return Task.CompletedTask;
        }

        var configNames = xrmSyncSection.GetChildren()
            .Select(c => c.Key)
            .ToList();

        if (configNames.Count == 0)
        {
            Console.WriteLine("No XrmSync configurations found in appsettings.json");
            return Task.CompletedTask;
        }

        Console.WriteLine($"Available configurations (from {GetConfigurationSource()}):");
        Console.WriteLine();

        foreach (var name in configNames)
        {
            Console.WriteLine($"  - {name}");

            // Try to get a brief status for this config
            var (isValid, summary) = GetConfigBriefStatus(name);
            var statusSymbol = isValid ? "✓" : "✗";
            Console.WriteLine($"    {statusSymbol} {summary}");
            Console.WriteLine();
        }

        return Task.CompletedTask;
    }

    private bool OutputSectionValidation(string sectionName, ConfigurationScope scope, Action displayValues)
    {
        try
        {
            // Create a temporary validator to check this section
            var validator = new XrmSyncConfigurationValidator(configOptions);
            validator.Validate(scope);

            // If we get here, validation passed
            Console.WriteLine($"✓ {sectionName}");
            displayValues();
            Console.WriteLine();
            return true;
        }
        catch (Model.Exceptions.OptionsValidationException ex)
        {
            Console.WriteLine($"✗ {sectionName}");
            displayValues();
            Console.WriteLine($"  {ex.Message}");
            Console.WriteLine();
            return false;
        }
        catch (AggregateException ex)
        {
            Console.WriteLine($"✗ {sectionName}");
            displayValues();
            Console.WriteLine($"  Errors:");
            foreach (var innerEx in ex.InnerExceptions.OfType<Model.Exceptions.OptionsValidationException>())
            {
                Console.WriteLine($"  {innerEx.Message}");
            }
            Console.WriteLine();
            return false;
        }
    }

    private (bool isValid, string summary) GetConfigBriefStatus(string configName)
    {
        try
        {
            // Create a temporary configuration builder for this config
            var tempSharedOptions = Microsoft.Extensions.Options.Options.Create(new SharedOptions(false, null, configName));
            var tempBuilder = new XrmSyncConfigurationBuilder(configuration, tempSharedOptions);
            var tempConfig = tempBuilder.Build();
            var tempConfigOptions = Microsoft.Extensions.Options.Options.Create(tempConfig);
            var tempValidator = new XrmSyncConfigurationValidator(tempConfigOptions);

            var sections = new List<string>();
            var hasErrors = false;

            // Check each section
            try
            {
                tempValidator.Validate(ConfigurationScope.PluginSync);
                sections.Add("plugins");
            }
            catch { hasErrors = true; }

            try
            {
                tempValidator.Validate(ConfigurationScope.PluginAnalysis);
                sections.Add("analyze");
            }
            catch { hasErrors = true; }

            try
            {
                tempValidator.Validate(ConfigurationScope.WebresourceSync);
                sections.Add("webresources");
            }
            catch { hasErrors = true; }

            if (sections.Count == 0)
            {
                return (false, "No valid configurations");
            }

            var summary = $"Configured: {string.Join(", ", sections)}";
            if (hasErrors)
            {
                summary += " (some sections have errors)";
            }

            return (!hasErrors, summary);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    private List<string> GetAvailableCommands()
    {
        var commands = new List<string>();
        var validator = new XrmSyncConfigurationValidator(configOptions);

        // Check if plugin sync is configured
        try
        {
            validator.Validate(ConfigurationScope.PluginSync);
            commands.Add("plugins");
        }
        catch { /* Not configured or invalid */ }

        // Check if plugin analysis is configured
        try
        {
            validator.Validate(ConfigurationScope.PluginAnalysis);
            commands.Add("analyze");
        }
        catch { /* Not configured or invalid */ }

        // Check if webresource sync is configured
        try
        {
            validator.Validate(ConfigurationScope.WebresourceSync);
            commands.Add("webresources");
        }
        catch { /* Not configured or invalid */ }

        return commands;
    }

    private static string GetConfigurationSource()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var baseFile = $"{ConfigReader.CONFIG_FILE_BASE}.json";
        var envFile = $"{ConfigReader.CONFIG_FILE_BASE}.{environment}.json";

        var basePath = Directory.GetCurrentDirectory();
        var baseExists = File.Exists(Path.Combine(basePath, baseFile));
        var envExists = File.Exists(Path.Combine(basePath, envFile));

        if (envExists && baseExists)
        {
            return $"{baseFile}, {envFile}";
        }
        else if (envExists)
        {
            return envFile;
        }
        else if (baseExists)
        {
            return baseFile;
        }
        else
        {
            return "no configuration file found";
        }
    }
}
