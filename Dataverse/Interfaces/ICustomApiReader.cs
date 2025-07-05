using DG.XrmPluginSync.Model.CustomApi;
using Microsoft.Xrm.Sdk;

namespace DG.XrmPluginSync.Dataverse.Interfaces;

public interface ICustomApiReader
{
    List<ApiDefinition> GetCustomApis(Guid solutionId);
}