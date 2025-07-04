using DG.XrmPluginSync.Model.CustomApi;
using DG.XrmPluginSync.Model.Plugin;

namespace DG.XrmPluginSync.SyncService;

public record CompiledData(List<PluginType> Types, List<Step> Steps, List<Image> Images, List<ApiDefinition> CustomApis, List<RequestParameter> RequestParameters, List<ResponseProperty> ResponseProperties);
