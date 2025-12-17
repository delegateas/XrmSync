using DG.Tools.XrmMockup;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Factory for creating XrmMockup365 instances.
/// Uses thread-safe lazy initialization for shared settings.
/// </summary>
public static class XrmMockupFactory
{
	private static readonly Lock SettingsLock = new();
	private static XrmMockupSettings? sharedSettings;

	/// <summary>
	/// Creates a new XrmMockup365 instance with shared settings.
	/// Each call returns a fresh instance with its own in-memory database.
	/// </summary>
	public static XrmMockup365 CreateMockup()
	{
		return XrmMockup365.GetInstance(GetSettings());
	}

	private static XrmMockupSettings GetSettings()
	{
		lock (SettingsLock)
		{
			return sharedSettings ??= new XrmMockupSettings
			{
				BasePluginTypes = [],
				CodeActivityInstanceTypes = [],
				EnableProxyTypes = true,
				IncludeAllWorkflows = false,
				MetadataDirectoryPath = GetMetadataPath(),
			};
		}
	}

	private static string GetMetadataPath()
	{
		var currentDir = AppDomain.CurrentDomain.BaseDirectory;

		// Try relative path from bin/Debug/net10.0
		var relativePath = Path.Combine(currentDir, "..", "..", "..", "Metadata");
		if (Directory.Exists(relativePath))
		{
			return relativePath;
		}

		// Fall back to output directory if copied
		var outputPath = Path.Combine(currentDir, "Metadata");
		if (Directory.Exists(outputPath))
		{
			return outputPath;
		}

		throw new DirectoryNotFoundException(
			$"Metadata directory not found. Tried:\n  {relativePath}\n  {outputPath}\n" +
			"Run scripts/Generate-XrmMockupMetadata.ps1 to generate metadata.");
	}
}
