using Microsoft.Xrm.Sdk;

namespace XrmSync.Dataverse.Interfaces
{
    public interface IDataverseWriter
    {
        Guid Create(Entity entity, IDictionary<string, object>? parameters);
        void PerformAsBulk<T>(List<T> updates) where T : OrganizationRequest;
        void Update(Entity entity);
        void UpdateMultiple<TEntity>(List<TEntity> entities) where TEntity : Entity;
    }
}