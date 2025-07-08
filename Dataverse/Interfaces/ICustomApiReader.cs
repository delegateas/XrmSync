using XrmSync.Model.CustomApi;

namespace XrmSync.Dataverse.Interfaces;

public interface ICustomApiReader
{
    List<ApiDefinition> GetCustomApis(Guid solutionId);
}