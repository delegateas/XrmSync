using XrmSync.Model;
using XrmSync.Model.CustomApi;

namespace XrmSync.Dataverse.Interfaces;

public interface ICustomApiWriter
{
    ICollection<CustomApiDefinition> CreateCustomApis(ICollection<CustomApiDefinition> customApis, string description);
    ICollection<ParentReference<RequestParameter, CustomApiDefinition>> CreateRequestParameters(ICollection<ParentReference<RequestParameter, CustomApiDefinition>> requestParameters);
    ICollection<ParentReference<ResponseProperty, CustomApiDefinition>> CreateResponseProperties(ICollection<ParentReference<ResponseProperty, CustomApiDefinition>> responseProperties);

    void UpdateCustomApis(ICollection<CustomApiDefinition> customApis, string description);
    void UpdateRequestParameters(ICollection<RequestParameter> requestParameters);
    void UpdateResponseProperties(ICollection<ResponseProperty> responseProperties);

    void DeleteCustomApiDefinitions(IEnumerable<CustomApiDefinition> customApis);
    void DeleteCustomApiRequestParameters(IEnumerable<RequestParameter> requestParameters);
    void DeleteCustomApiResponseProperties(IEnumerable<ResponseProperty> responseProperties);
}
