using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Difference;

public record Differences(
    Difference<PluginType> Types,
    Difference<Step> PluginSteps,
    Difference<Image> PluginImages,
    Difference<CustomApiDefinition> CustomApis,
    Difference<RequestParameter> RequestParameters,
    Difference<ResponseProperty> ResponseProperties
);
