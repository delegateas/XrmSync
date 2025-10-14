using Microsoft.Extensions.Options;
using XrmSync.Model;

namespace XrmSync.Options;

internal class XrmSyncConfigurationValidator(IOptions<XrmSyncConfiguration> configuration) : IConfigurationValidator
{
    public void Validate(ConfigurationScope scope)
    {
        if (scope == ConfigurationScope.None)
        {
            throw new Model.Exceptions.OptionsValidationException("No configuration scope specified for validation.");
        }

        if (scope.HasFlag(ConfigurationScope.PluginSync))
        {
            if (configuration.Value.Plugin?.Sync is null)
            {
                throw new Model.Exceptions.OptionsValidationException("Plugin sync options are required but not provided.");
            }

            Validate(configuration.Value.Plugin.Sync);
        }

        if (scope.HasFlag(ConfigurationScope.PluginAnalysis))
        {
            if (configuration.Value.Plugin?.Analysis is null)
            {
                throw new Model.Exceptions.OptionsValidationException("Plugin analysis options are required but not provided.");
            }

            Validate(configuration.Value.Plugin.Analysis);
        }
    }

    private static void Validate(PluginSyncOptions options)
    {
        var errors = new List<string>();

        // Validate AssemblyPath
        if (string.IsNullOrWhiteSpace(options.AssemblyPath))
        {
            errors.Add("Assembly path is required and cannot be empty.");
        }
        else if (!Path.IsPathRooted(options.AssemblyPath) && !File.Exists(options.AssemblyPath))
        {
            // For relative paths, check if file exists
            if (!File.Exists(Path.GetFullPath(options.AssemblyPath)))
            {
                errors.Add($"Assembly file does not exist: {options.AssemblyPath}");
            }
        }
        else if (Path.IsPathRooted(options.AssemblyPath) && !File.Exists(options.AssemblyPath))
        {
            // For absolute paths, check directly
            errors.Add($"Assembly file does not exist: {options.AssemblyPath}");
        }

        // Validate assembly file extension
        if (!string.IsNullOrWhiteSpace(options.AssemblyPath))
        {
            var extension = Path.GetExtension(options.AssemblyPath);
            if (!string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Assembly file must have a .dll extension.");
            }
        }

        // Validate SolutionName
        if (string.IsNullOrWhiteSpace(options.SolutionName))
        {
            errors.Add("Solution name is required and cannot be empty.");
        }
        else if (options.SolutionName.Length > 65)
        {
            errors.Add("Solution name cannot exceed 65 characters.");
        }

        // Validate LogLevel (enum validation is automatically handled by the type system)

        if (errors.Count > 0)
        {
            throw new Model.Exceptions.OptionsValidationException($"Sync options validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(e => $"- {e}"))}");
        }
    }

    private static void Validate(PluginAnalysisOptions options)
    {
        var errors = new List<string>();

        // Validate AssemblyPath
        if (string.IsNullOrWhiteSpace(options.AssemblyPath))
        {
            errors.Add("Assembly path is required and cannot be empty.");
        }
        else if (!Path.IsPathRooted(options.AssemblyPath) && !File.Exists(options.AssemblyPath))
        {
            // For relative paths, check if file exists
            if (!File.Exists(Path.GetFullPath(options.AssemblyPath)))
            {
                errors.Add($"Assembly file does not exist: {options.AssemblyPath}");
            }
        }
        else if (Path.IsPathRooted(options.AssemblyPath) && !File.Exists(options.AssemblyPath))
        {
            // For absolute paths, check directly
            errors.Add($"Assembly file does not exist: {options.AssemblyPath}");
        }

        // Validate assembly file extension
        if (!string.IsNullOrWhiteSpace(options.AssemblyPath))
        {
            var extension = Path.GetExtension(options.AssemblyPath);
            if (!string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Assembly file must have a .dll extension.");
            }
        }

        // Validate PublisherPrefix
        if (string.IsNullOrWhiteSpace(options.PublisherPrefix))
        {
            errors.Add("Publisher prefix is required and cannot be empty.");
        }
        else if (options.PublisherPrefix.Length < 2 || options.PublisherPrefix.Length > 8)
        {
            errors.Add("Publisher prefix must be between 2 and 8 characters.");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(options.PublisherPrefix, @"^[a-z][a-z0-9]*$"))
        {
            errors.Add("Publisher prefix must start with a lowercase letter and contain only lowercase letters and numbers.");
        }

        if (errors.Count > 0)
        {
            throw new Model.Exceptions.OptionsValidationException($"Analysis options validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select(e => $"- {e}"))}");
        }
    }
}
