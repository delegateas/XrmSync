using XrmSync.Model.CustomApi;

namespace XrmSync.Dataverse.Interfaces;

public interface ICustomApiReader
{
	List<CustomApiDefinition> GetCustomApis(Guid solutionId);
}
