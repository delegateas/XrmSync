using DG.XrmSync.Model.CustomApi;
using DG.XrmSync.Model.Plugin;

namespace DG.XrmSync.SyncService;

public record CompiledData(List<PluginType> Types, List<Step> Steps, List<Image> Images, List<ApiDefinition> CustomApis, List<RequestParameter> RequestParameters, List<ResponseProperty> ResponseProperties);
