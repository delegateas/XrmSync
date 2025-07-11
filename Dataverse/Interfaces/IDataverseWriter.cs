using Microsoft.Xrm.Sdk;

namespace XrmSync.Dataverse.Interfaces
{
    public interface IDataverseWriter
    {
        Guid Create(Entity entity, IDictionary<string, object>? parameters);
        List<ExecuteMultipleResponseItem> PerformAsBulk<T>(List<T> updates, Func<T, string> targetSelector) where T : OrganizationRequest;
        void PerformAsBulkWithOutput<T>(List<T> updates, Func<T, string> targetSelector) where T : OrganizationRequest;
        void Update(Entity entity);
        void UpdateMultiple<TEntity>(List<TEntity> entities) where TEntity : Entity;
    }
}