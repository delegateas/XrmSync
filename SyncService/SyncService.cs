using Microsoft.Extensions.Logging;
using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.SyncService.Models.Requests;
using DG.XrmPluginSync.SyncService.AssemblyReader;
using DG.XrmPluginSync.SyncService.Extensions;
using DG.XrmPluginSync.Dataverse.Interfaces;

namespace DG.XrmPluginSync.SyncService;

public class SyncService(ILogger log, ISolutionReader solution, IAssemblyReader assemblyReader, Plugin plugin)
{
    public async Task SyncPlugins(SyncRequest request)
    {
        request.LogAndValidateRequest();

        if (request.DryRun)
        {
            log.LogInformation("!!! Dry run mode is enabled. No changes will be made to Dataverse !!!");
        }

        log.LogInformation("Comparing plugins registered in CRM versus those found in your local code");

        log.LogInformation("Loading local assembly and its plugins");
        var localAssembly = await assemblyReader.ReadAssemblyAsync(request.AssemblyPath);
        log.LogInformation("Local assembly loaded, identified {0} plugins", localAssembly.PluginTypes.Count);

        log.LogInformation("Validating plugins to be registered");
        plugin.ValidatePlugins(localAssembly.PluginTypes);
        log.LogInformation("Plugins validated");

        log.LogInformation("Retrieving registered plugins from Dataverse");
        var solutionId = solution.GetSolutionId(request.SolutionName);
        var crmAssembly = plugin.GetPluginAssembly(solutionId, localAssembly.Name);
        crmAssembly = UpsertAssembly(request.SolutionName, localAssembly, crmAssembly);

        // Concat all local and steps and images
        var localPluginSteps = localAssembly.PluginTypes.SelectMany(x => x.PluginSteps).ToList();
        var crmPluginSteps = crmAssembly.PluginTypes.SelectMany(x => x.PluginSteps).ToList();
        var localPluginImages = localPluginSteps.SelectMany(x => x.PluginImages).ToList();
        var crmPluginImages = crmPluginSteps.SelectMany(x => x.PluginImages).ToList();

        // Set IDs on steps and images if they exist in crm
        crmPluginSteps.TransferIdsTo(localPluginSteps, x => x.Name);
        crmPluginImages.TransferIdsTo(localPluginImages, x => $"[{x.Name}] {x.PluginStepName}");

        // Get differences 
        var pluginTypeDifference = DifferenceUtility.GetDifference(localAssembly.PluginTypes, crmAssembly.PluginTypes, new PluginTypeEntity.PluginTypeDTOEqualityComparer<PluginTypeEntity>());
        log.Print(pluginTypeDifference, "PluginTypes", x => x.Name);

        var pluginStepsDifference = DifferenceUtility.GetDifference(localPluginSteps, crmPluginSteps, new PluginStepEntity.PluginStepDTOEqualityComparer<PluginStepEntity>());
        log.Print(pluginStepsDifference, "PluginSteps", x => x.Name);
        
        var pluginImagesDifference = DifferenceUtility.GetDifference(localPluginImages, crmPluginImages, new PluginImageEntity.PluginImageDTOEqualityComparer<PluginImageEntity>());
        log.Print(pluginImagesDifference, "PluginImages", x => $"[{x.Name}] {x.PluginStepName}");

        // Delete
        plugin.DeletePlugins(pluginTypeDifference.Deletes, pluginStepsDifference.Deletes, pluginImagesDifference.Deletes);

        // Update 
        plugin.UpdatePlugins(pluginStepsDifference.Updates, pluginImagesDifference.Updates);

        // Create
        plugin.CreatePlugins(crmAssembly, crmPluginSteps, request.SolutionName, pluginTypeDifference.Creates, pluginStepsDifference.Creates, pluginImagesDifference.Creates);

        log.LogInformation("Plugin synchronization was completed successfully");
    }

    private PluginAssembly UpsertAssembly(string solutionName, PluginAssembly localAssembly, PluginAssembly? remoteAssembly)
    {
        if (remoteAssembly == null)
        {
            remoteAssembly = plugin.CreatePluginAssembly(localAssembly, solutionName);
        }
        else if (remoteAssembly.Hash != localAssembly.Hash)
        {
            plugin.UpdatePluginAssembly(remoteAssembly.Id, localAssembly);
        }

        return remoteAssembly;
    }
}
