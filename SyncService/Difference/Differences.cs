using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Difference;

public record Differences(
    Difference<PluginDefinition> Types,
    Difference<Step, PluginDefinition> PluginSteps,
    Difference<Image, Step> PluginImages,
    Difference<CustomApiDefinition> CustomApis,
    Difference<RequestParameter, CustomApiDefinition> RequestParameters,
    Difference<ResponseProperty, CustomApiDefinition> ResponseProperties
);
