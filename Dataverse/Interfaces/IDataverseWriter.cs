using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace XrmSync.Dataverse.Interfaces
{
	public interface IDataverseWriter
	{
		Guid Create(Entity entity, IDictionary<string, object>? parameters);
		void Update(Entity entity);
		void UpdateMultiple<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity;
		void DeleteMultiple<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity;
		void DeleteMultiple(IEnumerable<DeleteRequest> deleteRequests);
	}
}
