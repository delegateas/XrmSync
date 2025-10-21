using XrmSync.Model;

namespace XrmSync.SyncService.Difference;

public record PrintHeaderOptions(string? Message, bool PrintConnection)
{
    public static PrintHeaderOptions Default => new(null, true);
}

public interface IPrintService
{
    void Print<TEntity, TParent>(Difference<TEntity, TParent> differences, string title, Func<ParentReference<TEntity, TParent>, string> namePicker)
        where TEntity : EntityBase
        where TParent : EntityBase;
    void Print<TEntity>(Difference<TEntity> differences, string title, Func<TEntity, string> namePicker) where TEntity : EntityBase;

    void PrintHeader(PrintHeaderOptions printHeaderOptions);
}
