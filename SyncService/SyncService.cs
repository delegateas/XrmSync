using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using DG.XrmPluginSync.Dataverse;
using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.SyncService.Models.Requests;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using DG.XrmPluginSync.SyncService.AssemblyReader;

namespace DG.XrmPluginSync.SyncService;

public class SyncService(ILogger log, Solution solution, IAssemblyReader assemblyReader/*, Plugin plugin, CrmDataHelper crmDataHelper*/)
{
    public async Task SyncPlugins(SyncRequest request)
    {
        request.LogAndValidateRequest();

        log.LogInformation("Comparing plugins registered in CRM versus those found in your local code");

        log.LogInformation("Loading local assembly and its plugins");
        var localAssembly = await assemblyReader.ReadAssemblyAsync(request.AssemblyPath);
        log.LogInformation("Local assembly loaded, identified {0} plugins", localAssembly.PluginTypes.Count);

        log.LogInformation("Validating plugins to be registered");
        //plugin.ValidatePlugins(localAssembly.PluginTypes);
        //log.LogInformation("Plugins validated");

        //log.LogInformation("Retriving registered plugins");
        //var solutionId = solution.GetSolutionId(request.SolutionName);
        //var crmAssembly = plugin.GetPluginAssemblyDTO(solutionId, localAssembly.DllName);

        return;
        
        //if (crmAssembly == null)
        //{
        //    log.LogInformation($"Creating assembly");
        //    crmAssembly = plugin.CreatePluginAssembly(localAssembly, request.SolutionName);
        //}
            
        //if (!crmAssembly.Equals(localAssembly))
        //{
        //    log.LogInformation($"Updating assembly");
        //    plugin.UpdatePluginAssembly(crmAssembly.AssemblyId, localAssembly);
        //}
        
        //// Concat all local and steps and images
        //var localPluginSteps = localAssembly.PluginTypes.SelectMany(x => x.PluginSteps).ToList();
        //var crmPluginSteps = crmAssembly.PluginTypes.SelectMany(x => x.PluginSteps).ToList();
        //var localPluginImages = localPluginSteps.SelectMany(x => x.PluginImages).ToList();
        //var crmPluginImages = crmPluginSteps.SelectMany(x => x.PluginImages).ToList();

        //// Set id´s on steps and images if they exist in crm
        //localPluginSteps.ForEach(x =>
        //{
        //    var crmStep = crmPluginSteps.FirstOrDefault(y => x.Name == y.Name);
        //    x.Id = crmStep == null ? Guid.Empty : crmStep.Id;
        //});
        //localPluginImages.ForEach(x =>
        //{
        //    var crmImage = crmPluginImages.FirstOrDefault(y => x.Name == y.Name);
        //    x.Id = crmImage == null ? Guid.Empty : crmImage.Id;
        //});

        //// Get differences 
        //log.LogInformation("Finding differences");
        //var pluginTypeDifference = DifferenceUtility.GetDifference(localAssembly.PluginTypes, crmAssembly.PluginTypes, new PluginTypeEntity.PluginTypeDTOEqualityComparer<PluginTypeEntity>());
        //var pluginStepsDifference = DifferenceUtility.GetDifference(localPluginSteps, crmPluginSteps, new PluginStepEntity.PluginStepDTOEqualityComparer<PluginStepEntity>());
        //var pluginImagesDifference = DifferenceUtility.GetDifference(localPluginImages, crmPluginImages, new PluginImageEntity.PluginImageDTOEqualityComparer<PluginImageEntity>());
        //log.LogInformation($"pluginTypes  - create: {pluginTypeDifference.Creates.Count()}, update: {pluginTypeDifference.Updates.Count()}, delete: {pluginTypeDifference.Deletes.Count()}");
        //log.LogInformation($"pluginSteps  - create: {pluginStepsDifference.Creates.Count()}, update: {pluginStepsDifference.Updates.Count()}, delete: {pluginStepsDifference.Deletes.Count()}");
        //log.LogInformation($"pluginImages - create: {pluginImagesDifference.Creates.Count()}, update: {pluginImagesDifference.Updates.Count()}, delete: {pluginImagesDifference.Deletes.Count()}");
        
        //// Delete
        //var deleteReqs = GetDeleteRequests(pluginTypeDifference.Deletes, pluginStepsDifference.Deletes, pluginImagesDifference.Deletes);
        //crmDataHelper.PerformAsBulkWithOutput(deleteReqs, log);

        //// Update 
        //var updateReqs = GetUpdateRequests(pluginStepsDifference.Updates, pluginImagesDifference.Updates);
        //crmDataHelper.PerformAsBulkWithOutput(updateReqs, log);

        //// Create - Doing it in the order type -> step -> image. Because there is a necessary relation. 
        //// Creating plugin types and appending them to existing crm plugintypes
        //var createdPluginTypes = plugin.CreatePluginTypes(pluginTypeDifference.Creates, crmAssembly.AssemblyId);
        //crmAssembly.PluginTypes.AddRange(createdPluginTypes);

        //var createdPluginSteps = plugin.CreatePluginSteps(pluginStepsDifference.Creates, crmAssembly.PluginTypes, request.SolutionName);
        //crmPluginSteps.AddRange(createdPluginSteps);

        //var createdPluginImages = plugin.CreatePluginImages(pluginImagesDifference.Creates, crmPluginSteps);

        //log.LogInformation("Plugin synchronization was completed successfully");
    }
    //private List<DeleteRequest> GetDeleteRequests(List<PluginTypeEntity> pluginTypes, List<PluginStepEntity> pluginSteps, List<PluginImageEntity> pluginImages)
    //{
    //    var pluginTypeReqs = pluginTypes
    //        .Select(x => new DeleteRequest
    //        {
    //            Target = new EntityReference("plugintype", x.Id)
    //        });
    //    var pluginStepReqs = pluginSteps
    //        .Select(x => new DeleteRequest
    //        {
    //            Target = new EntityReference("sdkmessageprocessingstep", x.Id)
    //        });
    //    var pluginImageReqs = pluginImages
    //        .Select(x => new DeleteRequest
    //        {
    //            Target = new EntityReference("sdkmessageprocessingstepimage", x.Id)
    //        });

    //    return pluginImageReqs.Concat(pluginStepReqs).Concat(pluginTypeReqs).ToList();
    //}
    //private List<UpdateRequest> GetUpdateRequests(List<PluginStepEntity> pluginSteps, List<PluginImageEntity> pluginImages)
    //{
    //    var pluginStepReqs = pluginSteps
    //        .Select(x =>
    //        {
    //            var entity = new Entity("sdkmessageprocessingstep", x.Id);
    //            entity.Attributes.Add("stage", new OptionSetValue(x.ExecutionStage));
    //            entity.Attributes.Add("filteringattributes", x.FilteredAttributes);
    //            entity.Attributes.Add("supporteddeployment", new OptionSetValue(x.Deployment));
    //            entity.Attributes.Add("mode", new OptionSetValue(x.ExecutionMode));
    //            entity.Attributes.Add("rank", x.ExecutionOrder);
    //            entity.Attributes.Add("description", Common.LoggerFactory.GetSyncDescription());
    //            entity.Attributes.Add("impersonatinguserid", x.UserContext == Guid.Empty ? null : new EntityReference("systemuser", x.Id));

    //            return new UpdateRequest
    //            {
    //                Target = entity
    //            };
    //        });
    //    var pluginImageReqs = pluginImages
    //        .Select(x =>
    //        {
    //            var entity = new Entity("sdkmessageprocessingstepimage", x.Id);
    //            entity.Attributes.Add("name", x.Name);
    //            entity.Attributes.Add("entityalias", x.EntityAlias);
    //            entity.Attributes.Add("imagetype", new OptionSetValue(x.ImageType));
    //            entity.Attributes.Add("attributes", x.ImageType);

    //            return new UpdateRequest
    //            {
    //                Target = entity
    //            };
    //        });

    //    return pluginImageReqs.Concat(pluginStepReqs).ToList();
    //}
}
