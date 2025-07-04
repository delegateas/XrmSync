using Microsoft.Xrm.Sdk;

namespace DG.XrmPluginSync.Dataverse.Interfaces;

public interface ICustomApiReader
{
    List<Entity> GetCustomApiRequestParameters(Guid customApiId);
    List<Entity> GetCustomApiResponseProperties(Guid customApiId);
    List<Entity> GetCustomApis(Guid solutionId);
}