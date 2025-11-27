using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService;

public record CompiledData(List<Step> Steps, List<Image> Images, List<CustomApiDefinition> CustomApis, List<RequestParameter> RequestParameters, List<ResponseProperty> ResponseProperties);
