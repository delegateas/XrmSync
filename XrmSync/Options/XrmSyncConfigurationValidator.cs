using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using XrmSync.Model;

namespace XrmSync.Options;

internal partial class XrmSyncConfigurationValidator(IOptions<XrmSyncConfiguration> configuration) : IConfigurationValidator
{
    public void Validate(ConfigurationScope scope)
    {
        if (scope == ConfigurationScope.None)
        {
            throw new Model.Exceptions.OptionsValidationException("No configuration scope specified for validation.");
        }

        var exceptions = ValidateInternal(scope, configuration.Value).ToList();
        if (exceptions.Count == 1)
        {
            throw exceptions[0];
        } else if (exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }
    }

    private static IEnumerable<Model.Exceptions.OptionsValidationException> ValidateInternal(ConfigurationScope scope, XrmSyncConfiguration configuration)
    {
        if (scope.HasFlag(ConfigurationScope.PluginSync))
        {
            var errors = Validate(configuration.Plugin.Sync).ToList();

            if (errors.Count != 0)
            {
                yield return new Model.Exceptions.OptionsValidationException("Plugin sync", errors);
            }
        }

        if (scope.HasFlag(ConfigurationScope.PluginAnalysis))
        {
            var errors = Validate(configuration.Plugin.Analysis).ToList();

            if (errors.Count != 0)
            {
                yield return new Model.Exceptions.OptionsValidationException("Plugin analysis", errors);
            }
        }

        if (scope.HasFlag(ConfigurationScope.WebresourceSync))
        {
            var errors = Validate(configuration.Webresource.Sync).ToList();

            if (errors.Count != 0)
            {
                yield return new Model.Exceptions.OptionsValidationException("Webresource sync", errors);
            }
        }
    }

    private static IEnumerable<string> Validate(PluginSyncOptions options)
    {
        return [
            ..ValidateAssemblyPath(options.AssemblyPath),
            ..ValidateSolutionName(options.SolutionName)
        ];
    }

    private static IEnumerable<string> Validate(PluginAnalysisOptions options)
    {
        return [
            ..ValidateAssemblyPath(options.AssemblyPath),
            ..ValidatePublisherPrefix(options.PublisherPrefix)
        ];
    }

    private static IEnumerable<string> Validate(WebresourceSyncOptions options)
    {
        return [
            ..ValidateFolderPath(options.FolderPath),
            ..ValidateSolutionName(options.SolutionName)
        ];
    }

    private static IEnumerable<string> ValidateAssemblyPath(string assemblyPath)
    {
        // Validate AssemblyPath
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            yield return "Assembly path is required and cannot be empty.";
        }
        else if (!File.Exists(Path.GetFullPath(assemblyPath)))
        {
            yield return $"Assembly file does not exist: {assemblyPath}";
        }
        else
        {
            // Validate assembly file extension
            var extension = Path.GetExtension(assemblyPath);
            if (!string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
            {
                yield return "Assembly file must have a .dll extension.";
            }
        }
    }

    private static IEnumerable<string> ValidateFolderPath(string folderPath)
    {
        // Validate FolderPath
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            yield return "Webresource root path is required and cannot be empty.";
        }
        else if (!Directory.Exists(Path.GetFullPath(folderPath)))
        {
            yield return $"Webresource root path does not exist: {folderPath}";
        }
    }

    private static IEnumerable<string> ValidateSolutionName(string? solutionName)
    {
        // Validate SolutionName
        if (string.IsNullOrWhiteSpace(solutionName))
        {
            yield return "Solution name is required and cannot be empty.";
        }
        else if (solutionName.Length > 65)
        {
            yield return "Solution name cannot exceed 65 characters.";
        }
    }
    private static IEnumerable<string> ValidatePublisherPrefix(string publisherPrefix)
    {
        // Validate PublisherPrefix
        if (string.IsNullOrWhiteSpace(publisherPrefix))
        {
            yield return "Publisher prefix is required and cannot be empty.";
        }
        else if (publisherPrefix.Length < 2 || publisherPrefix.Length > 8)
        {
            yield return "Publisher prefix must be between 2 and 8 characters.";
        }
        else if (!ValidPublisherPrefix().IsMatch(publisherPrefix))
        {
            yield return "Publisher prefix must start with a lowercase letter and contain only lowercase letters and numbers.";
        }
    }

    [GeneratedRegex(@"^[a-z][a-z0-9]{1,7}$")]
    private static partial Regex ValidPublisherPrefix();
}
