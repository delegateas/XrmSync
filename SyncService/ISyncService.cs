namespace XrmSync.SyncService;

public interface ISyncService
{
    Task Sync(CancellationToken cancellation);
}