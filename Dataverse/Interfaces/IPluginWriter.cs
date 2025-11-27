using XrmSync.Model;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse.Interfaces;

public interface IPluginWriter
{
	ICollection<PluginDefinition> CreatePluginTypes(ICollection<PluginDefinition> pluginTypes, Guid assemblyId, string description);
	void DeletePluginTypes(IEnumerable<PluginDefinition> pluginTypes);

	ICollection<ParentReference<Step, PluginDefinition>> CreatePluginSteps(ICollection<ParentReference<Step, PluginDefinition>> pluginSteps, string description);
	void UpdatePluginSteps(IEnumerable<Step> pluginSteps, string description);
	void DeletePluginSteps(IEnumerable<Step> pluginSteps);

	ICollection<ParentReference<Image, Step>> CreatePluginImages(ICollection<ParentReference<Image, Step>> pluginImages);
	void UpdatePluginImages(IEnumerable<ParentReference<Image, Step>> pluginImages);
	void DeletePluginImages(IEnumerable<Image> pluginImages);

}
