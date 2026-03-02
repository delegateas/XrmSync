namespace XrmSync.Model.Webresource;

public static class WebresourceTypeMap
{
	public static readonly Dictionary<string, WebresourceType> ExtensionToType = new()
	{
		{ ".html", WebresourceType.HTML },
		{ ".htm", WebresourceType.HTML },
		{ ".css", WebresourceType.CSS },
		{ ".js", WebresourceType.JS },
		{ ".xml", WebresourceType.XML },
		{ ".xaml", WebresourceType.XML },
		{ ".xsd", WebresourceType.XML },
		{ ".xsl", WebresourceType.XSL },
		{ ".xslt", WebresourceType.XSL },
		{ ".png", WebresourceType.PNG },
		{ ".jpg", WebresourceType.JPG },
		{ ".jpeg", WebresourceType.JPG },
		{ ".gif", WebresourceType.GIF },
		{ ".xap", WebresourceType.XAP },
		{ ".ico", WebresourceType.ICO },
		{ ".svg", WebresourceType.SVG },
		{ ".resx", WebresourceType.RSX }
	};

	/// <summary>
	/// Resolves file extension strings (e.g. "js", ".css") to their corresponding WebresourceType values.
	/// Returns an empty set if no extensions are provided (meaning "all types").
	/// </summary>
	public static HashSet<WebresourceType> ResolveTypes(IEnumerable<string>? extensions)
	{
		if (extensions?.Any() != true)
			return [];

		return [.. extensions
			.Select(ext => ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}")
			.Where(ExtensionToType.ContainsKey)
			.Select(ext => ExtensionToType[ext])];
	}
}
