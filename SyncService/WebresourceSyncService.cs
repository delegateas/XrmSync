using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Analyzer.Reader;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Webresource;
using XrmSync.SyncService.Difference;
using XrmSync.SyncService.Exceptions;
using XrmSync.SyncService.Validation;

namespace XrmSync.SyncService;

internal class WebresourceSyncService(
    IOptions<WebresourceSyncOptions> config,
    ILogger<WebresourceSyncService> log,
    ILocalReader localReader,
    ISolutionReader solutionReader,
    IWebresourceReader webresourceReader,
    IWebresourceWriter webresourceWriter,
    IValidator<WebresourceDefinition> webresourceValidator,
    IPrintService printService
    ) : ISyncService
{
    private readonly WebresourceSyncOptions options = config.Value;

    public Task Sync(CancellationToken cancellation)
    {
        printService.PrintHeader(PrintHeaderOptions.Default with { Message = "Comparing webresources registered in Dataverse versus those found in your local code" });

        var (local, remote) = ReadData();

        // Map remote to local by name, and set IDs
        MapIds(local, remote);

        // Get operations
        var toCreate = ToCreate(local, remote);
        var toDelete = ToDelete(local, remote);
        var toUpdate = ToUpdate(local, remote);

        // Validate webresources to be created and deleted
        ValidateWebresourcesOrThrow(toCreate);
        ValidateWebresourcesOrThrow(toDelete);

        webresourceWriter.Create(toCreate);
        webresourceWriter.Delete(toDelete);
        webresourceWriter.Update(toUpdate);

        log.LogInformation("Webresource synchronization was completed successfully");

        return Task.CompletedTask;
    }

    private (List<WebresourceDefinition> local, List<WebresourceDefinition> remote) ReadData()
    {
        log.LogInformation("Reading solution information for solution \"{solutionName}\"", options.SolutionName);
        var (solutionId, solutionPrefix) = solutionReader.RetrieveSolution(options.SolutionName);

        var prefix = $"{solutionPrefix}_{options.SolutionName}";

        var local = localReader.ReadWebResourceFolder(options.FolderPath, prefix);
        log.LogInformation("Identified {count} webresources in local folder", local.Count);

        var remote = webresourceReader.GetWebresources(solutionId);
        log.LogInformation("Identified {count} webresources registered in CRM", remote.Count);

        return (local, remote);
    }

    private static void MapIds(List<WebresourceDefinition> local, List<WebresourceDefinition> remote)
    {
        var joined = local.Join(
            remote,
            l => l.Name,
            r => r.Name,
            (l, r) => new { Local = l, Remote = r },
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var item in joined)
        {
            item.Local.Id = item.Remote.Id;
        }
    }

    private List<WebresourceDefinition> ToCreate(List<WebresourceDefinition> local, List<WebresourceDefinition> remote)
    {
        var toCreate = local
            .Where(l => l.Id == default)
            .ToList();
        log.LogInformation("Identified {count} webresources to create in CRM", toCreate.Count);

        foreach (var item in toCreate)
        {
            log.LogDebug("  - {name}", item.Name);
        }

        return toCreate;
    }

    private List<WebresourceDefinition> ToDelete(List<WebresourceDefinition> local, List<WebresourceDefinition> remote)
    {
        var toDelete = remote.ExceptBy(local.Select(d => d.Id), d => d.Id).ToList();

        log.LogInformation("Identified {count} webresources to delete from CRM", toDelete.Count);
        foreach (var item in toDelete)
        {
            log.LogDebug("  - {name}", item.Name);
        }
        return toDelete;
    }

    private List<WebresourceDefinition> ToUpdate(List<WebresourceDefinition> local, List<WebresourceDefinition> remote)
    {
        var toUpdate = local
            .Where(l => l.Id != default)
            .Join(
                remote,
                l => l.Id,
                r => r.Id,
                (l, r) => new { Local = l, Remote = r }
            )
            .Select(item => (
                item.Local,
                contentDiffers: !item.Local.Content.Equals(item.Remote.Content, StringComparison.OrdinalIgnoreCase),
                nameDiffers: !item.Local.DisplayName.Equals(item.Remote.DisplayName, StringComparison.OrdinalIgnoreCase)
            ))
            .Where(item => item.contentDiffers || item.nameDiffers)
            .ToList();

        log.LogInformation("Identified {count} webresources to update in CRM", toUpdate.Count);
        foreach (var item in toUpdate)
        {
            log.LogDebug("  - {name}{contentDiffers}{nameDiffer}",
                item.Local.Name,
                item.contentDiffers ? " (content differs)" : "",
                item.nameDiffers ? " (name differs)" : ""
            );
        }

        return [.. toUpdate.Select(u => u.Local)];
    }

    private void ValidateWebresourcesOrThrow(List<WebresourceDefinition> webresources)
    {
        if (webresources.Count == 0)
        {
            return;
        }

        try
        {
            webresourceValidator.ValidateOrThrow(webresources);
        }
        catch (ValidationException ex)
        {
            log.LogError("Validation failed for the local webresources:");
            log.LogError(" - {Message}", ex.Message);
            throw new XrmSyncException("Validation failed for the local webresources", ex);
        }
        catch (AggregateException ex)
        {
            log.LogError("Validation failed for the local webresources:");
            foreach (var inner in ex.InnerExceptions)
            {
                log.LogError(" - {Message}", inner.Message);
            }
            throw new XrmSyncException("Validation failed for the local webresources", ex);
        }
    }
}
