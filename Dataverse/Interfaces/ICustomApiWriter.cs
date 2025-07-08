using XrmSync.Model.CustomApi;

namespace XrmSync.Dataverse.Interfaces;

public interface ICustomApiWriter
{
    List<ApiDefinition> CreateCustomApis(List<ApiDefinition> customApis, string solutionName, string description);
    List<RequestParameter> CreateRequestParameters(List<RequestParameter> requestParameters, List<ApiDefinition> customApis);
    List<ResponseProperty> CreateResponseProperties(List<ResponseProperty> responseProperties, List<ApiDefinition> customApis);
    List<ApiDefinition> UpdateCustomApis(List<ApiDefinition> customApis, string description);
    List<RequestParameter> UpdateRequestParameters(List<RequestParameter> requestParameters);
    List<ResponseProperty> UpdateResponseProperties(List<ResponseProperty> responseProperties);
}
