using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Model;

namespace XrmSync.SyncService;

internal class WebresourceSyncService(IOptions<WebresourceSyncOptions> options, ILogger<WebresourceSyncService> logger) : ISyncService
{
    public Task Sync(CancellationToken cancellation)
    {
        logger.LogInformation("Hello, {0}", options.Value.FolderPath);
        throw new NotImplementedException();
    }
}
