using Microsoft.Xrm.Sdk;

namespace DG.XrmPluginSync.Dataverse.Interfaces
{
    public interface IDataverseWriter
    {
        Guid Create(Entity entity);
        List<ExecuteMultipleResponseItem> PerformAsBulk<T>(List<T> updates) where T : OrganizationRequest;
        void PerformAsBulkWithOutput<T>(List<T> updates) where T : OrganizationRequest;
        void Update(Entity entity);
    }
}