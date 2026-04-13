using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using XrmSync.Model;

namespace XrmSync.Options;

internal partial class XrmSyncConfigurationValidator(IOptions<XrmSyncConfiguration> configuration, IOptions<SharedOptions> sharedOptions) : IConfigurationValidator
{
	public void Validate(ConfigurationScope scope)
	{
		if (scope == ConfigurationScope.None)
			return;

		var exceptions = ValidateInternal(scope, configuration.Value, sharedOptions.Value.ProfileName).ToList();
		if (exceptions.Count == 1)
		{
			throw exceptions[0];
		}
		else if (exceptions.Count > 1)
		{
			throw new AggregateException(exceptions);
		}
	}

	private static IEnumerable<Model.Exceptions.OptionsValidationException> ValidateInternal(ConfigurationScope scope, XrmSyncConfiguration configuration, string? profileName)
	{
		// Resolve profile using shared logic
		var profile = configuration.ResolveProfile(profileName);

		if (profile == null)
		{
			// No profiles configured and no specific profile requested, validation passes (CLI mode)
			yield break;
		}

		// Validate solution name at profile level
		var profileErrors = ValidateSolutionName(profile.SolutionName).ToList();
		if (profileErrors.Count != 0)
		{
			yield return new Model.Exceptions.OptionsValidationException($"Profile '{profile.Name}'", profileErrors);
		}

		// Validate each sync item in the profile based on scope
		foreach (var syncItem in profile.Sync)
		{
			List<string> errors = new();

			switch (syncItem)
			{
				case PluginSyncItem pluginSync when scope.HasFlag(ConfigurationScope.PluginSync):
					errors = Validate(pluginSync).ToList();
					if (errors.Count != 0)
					{
						yield return new Model.Exceptions.OptionsValidationException($"Plugin sync in profile '{profile.Name}'", errors);
					}
					break;

				case PluginAnalysisSyncItem pluginAnalysis when scope.HasFlag(ConfigurationScope.PluginAnalysis):
					errors = Validate(pluginAnalysis).ToList();
					if (errors.Count != 0)
					{
						yield return new Model.Exceptions.OptionsValidationException($"Plugin analysis in profile '{profile.Name}'", errors);
					}
					break;

				case WebresourceSyncItem webresource when scope.HasFlag(ConfigurationScope.WebresourceSync):
					errors = Validate(webresource).ToList();
					if (errors.Count != 0)
					{
						yield return new Model.Exceptions.OptionsValidationException($"Webresource sync in profile '{profile.Name}'", errors);
					}
					break;

				case IdentitySyncItem identity when scope.HasFlag(ConfigurationScope.Identity):
					errors = Validate(identity).ToList();
					if (errors.Count != 0)
					{
						yield return new Model.Exceptions.OptionsValidationException($"Identity ({identity.Operation}) in profile '{profile.Name}'", errors);
					}
					break;
			}
		}
	}

	private static IEnumerable<string> Validate(IdentitySyncItem syncItem)
	{
		var errors = new List<string>();
		errors.AddRange(ValidateAssemblyPath(syncItem.AssemblyPath));

		if (syncItem.Operation == IdentityOperation.Ensure)
		{
			errors.AddRange(ValidateGuid(syncItem.ClientId ?? string.Empty, "Client ID"));
			errors.AddRange(ValidateGuid(syncItem.TenantId ?? string.Empty, "Tenant ID"));
		}

		return errors;
	}

	private static IEnumerable<string> Validate(PluginSyncItem syncItem)
	{
		return [
			..ValidateAssemblyPath(syncItem.AssemblyPath)
		];
	}

	private static IEnumerable<string> Validate(PluginAnalysisSyncItem syncItem)
	{
		return [
			..ValidateAssemblyPath(syncItem.AssemblyPath),
			..ValidatePublisherPrefix(syncItem.PublisherPrefix)
		];
	}

	private static IEnumerable<string> Validate(WebresourceSyncItem syncItem)
	{
		return [
			..ValidateFolderPath(syncItem.FolderPath)
		];
	}

	internal static IEnumerable<string> ValidateAssemblyPath(string assemblyPath)
	{
		if (string.IsNullOrWhiteSpace(assemblyPath))
		{
			yield return "Assembly path is required and cannot be empty.";
			yield break;
		}

		var extension = Path.GetExtension(assemblyPath);
		if (!string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
		{
			yield return "Assembly file must have a .dll extension.";
			yield break;
		}

		if (!File.Exists(Path.GetFullPath(assemblyPath)))
		{
			yield return $"Assembly file does not exist: {assemblyPath}";
		}
	}

	internal static IEnumerable<string> ValidateFolderPath(string folderPath)
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

	internal static IEnumerable<string> ValidateSolutionName(string? solutionName)
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
	internal static IEnumerable<string> ValidatePublisherPrefix(string publisherPrefix)
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

	internal static IEnumerable<string> ValidateGuid(string value, string fieldName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			yield return $"{fieldName} is required and cannot be empty.";
		}
		else if (!Guid.TryParse(value, out _))
		{
			yield return $"{fieldName} must be a valid GUID.";
		}
	}

	[GeneratedRegex(@"^[a-z][a-z0-9]{1,7}$")]
	private static partial Regex ValidPublisherPrefix();
}
