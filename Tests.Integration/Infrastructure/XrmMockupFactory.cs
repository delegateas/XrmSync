using DG.Tools.XrmMockup;
using Microsoft.Xrm.Sdk.Metadata;
using System.Reflection;

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
		var mockup = XrmMockup365.GetInstance(GetSettings());
		PatchEntityNameAttributes(mockup);
		return mockup;
	}

	/// <summary>
	/// XrmMockup models EntityName attributes as int-backed picklists, but Dataverse (and the
	/// early-bound context XrmSync uses) exposes them as the entity logical name string. Without
	/// this patch XrmMockup silently discards the string value on write, so any read or query
	/// against <c>sdkmessagefilter.primaryobjecttypecode</c> comes back empty.
	/// </summary>
	private static void PatchEntityNameAttributes(XrmMockupBase mockup)
	{
		foreach (var entityMetadata in GetEntityMetadata(mockup).Values)
		{
			var attributes = entityMetadata.Attributes;
			if (attributes is null || !Array.Exists(attributes, a => a is EntityNameAttributeMetadata))
			{
				continue;
			}

			AttributesProperty.SetValue(entityMetadata, Array.ConvertAll(attributes,
				a => a is EntityNameAttributeMetadata ? AsStringAttribute(a) : a));
		}
	}

	private static AttributeMetadata AsStringAttribute(AttributeMetadata source)
	{
		var replacement = new StringAttributeMetadata
		{
			LogicalName = source.LogicalName,
			SchemaName = source.SchemaName,
			MaxLength = 256,
		};

		// IsValidFor* have internal setters; the platform sets primaryobjecttypecode itself, but
		// tests have to seed it directly.
		SetInternal(replacement, nameof(AttributeMetadata.IsValidForCreate), true);
		SetInternal(replacement, nameof(AttributeMetadata.IsValidForUpdate), true);
		SetInternal(replacement, nameof(AttributeMetadata.IsValidForRead), true);

		return replacement;
	}

	private static void SetInternal(AttributeMetadata target, string propertyName, bool value) =>
		typeof(AttributeMetadata).GetProperty(propertyName)!.SetValue(target, value);

	private static Dictionary<string, EntityMetadata> GetEntityMetadata(XrmMockupBase mockup)
	{
		var skeleton = MetadataProperty.GetValue(mockup)!;
		return (Dictionary<string, EntityMetadata>)skeleton.GetType().GetField("EntityMetadata")!.GetValue(skeleton)!;
	}

	private static readonly PropertyInfo MetadataProperty =
		typeof(XrmMockupBase).GetProperty("Metadata", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;

	private static readonly PropertyInfo AttributesProperty =
		typeof(EntityMetadata).GetProperty(nameof(EntityMetadata.Attributes))!;

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
