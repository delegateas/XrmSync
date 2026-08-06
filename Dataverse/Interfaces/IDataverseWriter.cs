using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace XrmSync.Dataverse.Interfaces
{
	public interface IDataverseWriter
	{
		Guid Create(Entity entity, IDictionary<string, object>? parameters);
		void Update(Entity entity);
		void Delete(Entity entity);
		void UpdateMultiple<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity;
		void DeleteMultiple<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity;
		void DeleteMultiple(IEnumerable<DeleteRequest> deleteRequests);

		/// <summary>
		/// Publishes customizations. <paramref name="parameterXml"/> is the PublishXml importexportxml payload.
		/// Always a single request — the bulk path only handles Create/Update/Delete.
		/// </summary>
		void PublishXml(string parameterXml);
	}
}
