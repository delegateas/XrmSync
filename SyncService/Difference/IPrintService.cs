using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public interface IPrintService
{
    void Print<TEntity, TParent>(Difference<TEntity, TParent> differences, string title, Func<ParentReference<TEntity, TParent>, string> namePicker)
        where TEntity : EntityBase
        where TParent : EntityBase;
    void Print<TEntity>(Difference<TEntity> differences, string title, Func<TEntity, string> namePicker) where TEntity : EntityBase;
}