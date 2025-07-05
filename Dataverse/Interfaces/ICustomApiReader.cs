using DG.XrmSync.Model.CustomApi;
using Microsoft.Xrm.Sdk;

namespace DG.XrmSync.Dataverse.Interfaces;

public interface ICustomApiReader
{
    List<ApiDefinition> GetCustomApis(Guid solutionId);
}