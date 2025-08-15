using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;

namespace XrmSync.Dataverse.Interfaces;

public interface ICustomApiWriter
{
    List<CustomApiDefinition> CreateCustomApis(List<CustomApiDefinition> customApis, List<PluginType> pluginTypes, string description);
    List<RequestParameter> CreateRequestParameters(List<RequestParameter> requestParameters, List<CustomApiDefinition> customApis);
    List<ResponseProperty> CreateResponseProperties(List<ResponseProperty> responseProperties, List<CustomApiDefinition> customApis);
    List<CustomApiDefinition> UpdateCustomApis(List<CustomApiDefinition> customApis, List<PluginType> pluginTypes, string description);
    List<RequestParameter> UpdateRequestParameters(List<RequestParameter> requestParameters);
    List<ResponseProperty> UpdateResponseProperties(List<ResponseProperty> responseProperties);
    void DeleteCustomApiDefinitions(IEnumerable<CustomApiDefinition> customApis);
    void DeleteCustomApiRequestParameters(IEnumerable<RequestParameter> requestParameters);
    void DeleteCustomApiResponseProperties(IEnumerable<ResponseProperty> responseProperties);
}
