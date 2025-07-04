using Microsoft.Xrm.Sdk;

namespace DG.XrmPluginSync.Dataverse.Interfaces
{
    public interface IDataverseWriter
    {
        Guid Create(Entity entity);
        Guid Create(Entity entity, ParameterCollection parameters);
        List<ExecuteMultipleResponseItem> PerformAsBulk<T>(List<T> updates, Func<T, string> targetSelector) where T : OrganizationRequest;
        void PerformAsBulkWithOutput<T>(List<T> updates, Func<T, string> targetSelector) where T : OrganizationRequest;
        void Update(Entity entity);
    }
}