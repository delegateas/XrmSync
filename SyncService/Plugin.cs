//using DG.XrmPluginSync.Dataverse;
//using DG.XrmPluginSync.SyncService.Common;
//using DG.XrmPluginSync.SyncService.Models.Requests;
//using Microsoft.Extensions.Logging;
//using Microsoft.PowerPlatform.Dataverse.Client;
//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Messages;
//using Microsoft.Xrm.Sdk.Query;
//using System.Reflection;

//using StepConfig = System.Tuple<string, int, string, string>;
//using ExtendedStepConfig = System.Tuple<int, int, string, int, string, string>;
//using ImageTuple = System.Tuple<string, string, int, string>;
//using DG.XrmPluginSync.SyncService.AssemblyReader;
//using DG.XrmPluginSync.Model;

//namespace DG.XrmPluginSync.SyncService;

//public class Plugin(ILogger log, CrmDataHelper crmDataHelper, ServiceClient service, Solution solution, Message messageUtility)
//{
//    public List<Entity> GetPluginAssemblies(Guid solutionId)
//    {
//        LinkEntity link = new()
//        {
//            JoinOperator = JoinOperator.Inner,
//            LinkFromAttributeName = "pluginassemblyid",
//            LinkFromEntityName = "pluginassembly",
//            LinkToAttributeName = "objectid",
//            LinkToEntityName = "solutioncomponent"
//        };
//        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));
//        FilterExpression filter = new();
//        QueryExpression query = new("pluginassembly")
//        {
//            ColumnSet = new ColumnSet(allColumns: true)
//        };
//        query.LinkEntities.Add(link);
//        query.Criteria = filter;

//        return crmDataHelper.RetrieveMultiple(query);
//    }

//    public Entity GetPluginAssembly(Guid id)
//    {
//        return service.Retrieve("pluginassembly", id, new ColumnSet(true));
//    }

//    public Entity GetPluginAssembly(string name, string version)
//    {
//        LinkEntity link = new()
//        {
//            JoinOperator = JoinOperator.Inner,
//            LinkFromAttributeName = "pluginassemblyid",
//            LinkFromEntityName = "pluginassembly",
//            LinkToAttributeName = "objectid",
//            LinkToEntityName = "solutioncomponent"
//        };

//        link.Columns.AddColumn("solutionid");

//        FilterExpression filter = new();
//        filter.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, name));
//        filter.AddCondition(new ConditionExpression("version", ConditionOperator.Equal, version));

//        QueryExpression query = new("pluginassembly")
//        {
//            ColumnSet = new ColumnSet(allColumns: true)
//        };
//        query.LinkEntities.Add(link);
//        query.Criteria = filter;

//        return crmDataHelper.RetrieveFirstOrDefault(query);
//    }

//    public Entity GetPluginAssembly(Guid solutionId, string assemblyName)
//    {
//        LinkEntity link = new()
//        {
//            JoinOperator = JoinOperator.Inner,
//            LinkFromAttributeName = "pluginassemblyid",
//            LinkFromEntityName = "pluginassembly",
//            LinkToAttributeName = "objectid",
//            LinkToEntityName = "solutioncomponent"
//        };
//        link.Columns.AddColumn("solutionid");
//        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));

//        FilterExpression filter = new();
//        filter.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, assemblyName));

//        QueryExpression query = new("pluginassembly")
//        {
//            ColumnSet = new ColumnSet(allColumns: true)
//        };
//        query.LinkEntities.Add(link);
//        query.Criteria = filter;
//        return crmDataHelper.RetrieveFirstOrDefault(query);
//    }

//    public PluginAssembly GetPluginAssemblyDTO(Guid solutionId, string assemblyName)
//    {
//        var assemblyEntity = GetPluginAssembly(solutionId, assemblyName);
//        if (assemblyEntity == null) return null;

//        var assemblyDTO = new PluginAssembly
//        {
//            AssemblyId = assemblyEntity.Id
//        };
//        //assemblyDTO.Hash = (string)assemblyEntity.Attributes["sourcehash"]; TODO: does this field even exist?

//        var pluginTypes = GetPluginTypes(assemblyDTO.AssemblyId);
//        var pluginTypeDtos = pluginTypes
//            .Select(type =>
//            {
//                var pluginSteps = GetPluginSteps(solutionId, type.Id);
//                var pluginStepDtos = pluginSteps.Select(step =>
//                {
//                    var pluginImages = GetPluginImages(step.Id)
//                    .Select(image => new PluginImageEntity
//                    {
//                        PluginStepName = (string)step.Attributes["name"],
//                        Name = (string)image.Attributes["name"],
//                        EntityAlias = (string)image.Attributes["entityalias"],
//                        ImageType = ((OptionSetValue)image.Attributes["imagetype"]).Value,
//                        Attributes = (string)image.Attributes["attributes"],
//                        Id = image.Id
//                    }).ToList();

//                    return new PluginStepEntity
//                    {
//                        ExecutionStage = ((OptionSetValue)step.Attributes["stage"]).Value,
//                        Deployment = ((OptionSetValue)step.Attributes["supporteddeployment"]).Value,
//                        ExecutionMode = ((OptionSetValue)step.Attributes["mode"]).Value,
//                        ExecutionOrder = (int)step.Attributes["rank"],
//                        FilteredAttributes = (string)step.Attributes["filteringattributes"],
//                        UserContext = ((EntityReference)step.Attributes["impersonatinguserid"]).Id,
//                        PluginTypeName = (string)type.Attributes["name"],
//                        Name = (string)step.Attributes["name"],
//                        PluginImages = pluginImages,

//                        Id = step.Id
//                    };
//                }).ToList();
//                return new PluginTypeEntity
//                {
//                    Name = (string)type.Attributes["name"],
//                    PluginSteps = pluginStepDtos,
//                    Id = type.Id
//                };
//            }).ToList();

//        assemblyDTO.PluginTypes = pluginTypeDtos;

//        return assemblyDTO;
//    }
//    public void UpdatePluginAssembly(string path, string version, Entity assembly)
//    {
//        Entity newAssembly = new("pluginassembly", assembly.Id);
//        newAssembly.Attributes.Add("content", FileUtility.GetBase64StringFromFile(path));
//        newAssembly.Attributes.Add("version", version);
//        newAssembly.Attributes.Add("description", Common.LoggerFactory.GetSyncDescription());
//        service.Update(newAssembly);
//        log.LogInformation($"{newAssembly.LogicalName}: {assembly.Attributes["name"]} was updated with version {version}");
//    }
//    public void UpdatePluginAssembly(Guid assemblyId, PluginAssembly localAssembly)
//    {
//        var entity = new Entity("pluginassembly", assemblyId);
//        entity.Attributes.Add("name", localAssembly.DllName);
//        entity.Attributes.Add("content", FileUtility.GetBase64StringFromFile(localAssembly.DllPath));
//        entity.Attributes.Add("sourcehash", localAssembly.Hash);
//        entity.Attributes.Add("isolationmode", new OptionSetValue(2));
//        entity.Attributes.Add("version", localAssembly.AssemblyVersion.ToString());
//        entity.Attributes.Add("description", Common.LoggerFactory.GetSyncDescription());

//        service.Update(entity);
//    }
//    public Entity CreatePluginAssembly(string solutionName, string name, string path, string version)
//    {
//        var newAssembly = new Entity("pluginassembly");
//        newAssembly.Attributes.Add("name", name);
//        //newAssembly.Attributes.Add("sourcehash", "DEBUG")
//        newAssembly.Attributes.Add("content", FileUtility.GetBase64StringFromFile(path));
//        newAssembly.Attributes.Add("isolationmode", new OptionSetValue(2)); // 2 is sandbox
//        newAssembly.Attributes.Add("version", version);
//        newAssembly.Attributes.Add("description", Common.LoggerFactory.GetSyncDescription());

//        var parameters = new ParameterCollection();
//        parameters.Add("SolutionUniqueName", solutionName);

//        var req = new CreateRequest
//        {
//            Target = newAssembly
//        };
//        req.Parameters.AddRange(parameters);

//        newAssembly.Id = ((CreateResponse)service.Execute(req)).id;
//        return newAssembly;
//    }
//    public PluginAssembly CreatePluginAssembly(PluginAssembly localAssembly, string solutionName)
//    {
//        var entity = new Entity("pluginassembly");
//        entity.Attributes.Add("name", localAssembly.DllName);
//        entity.Attributes.Add("content", FileUtility.GetBase64StringFromFile(localAssembly.DllPath));
//        entity.Attributes.Add("sourcehash", localAssembly.Hash);
//        entity.Attributes.Add("isolationmode", new OptionSetValue(2));
//        entity.Attributes.Add("version", localAssembly.AssemblyVersion.ToString());
//        entity.Attributes.Add("description", Common.LoggerFactory.GetSyncDescription());

//        var parameters = new ParameterCollection();
//        parameters.Add("SolutionUniqueName", solutionName);

//        var req = new CreateRequest
//        {
//            Target = entity
//        };
//        req.Parameters.AddRange(parameters);
//        var assemblyId = ((CreateResponse)service.Execute(req)).id;

//        return new PluginAssembly
//        {
//            AssemblyId = assemblyId,
//            DllName = localAssembly.DllName,
//            Hash = localAssembly.Hash,
//        };
//    }
//    public List<PluginTypeEntity> CreatePluginTypes(List<PluginTypeEntity> pluginTypes, Guid assemblyId)
//    {
//        return pluginTypes.Select(x =>
//        {
//            var entity = new Entity("plugintype");
//            entity.Attributes.Add("name", x.Name);
//            entity.Attributes.Add("typename", x.Name);
//            entity.Attributes.Add("friendlyname", Guid.NewGuid().ToString());
//            entity.Attributes.Add("pluginassemblyid", new EntityReference("pluginassembly", assemblyId));
//            entity.Attributes.Add("description", Common.LoggerFactory.GetSyncDescription());

//            x.Id = service.Create(entity);
//            return x;
//        }).ToList();
//    }
//    public List<PluginStepEntity> CreatePluginSteps(List<PluginStepEntity> pluginSteps, List<PluginTypeEntity> pluginTypes, string solutionName)
//    {
//        return pluginSteps.Select(step =>
//        {
//            var pluginType = pluginTypes.First(type => type.Name == step.PluginTypeName);
//            var message = messageUtility.GetMessage(step.EventOperation);
//            var messageFilter = messageUtility.GetMessageFilter(step.LogicalName, message.Id);

//            var entity = new Entity("sdkmessageprocessingstep");
//            entity.Attributes.Add("name", step.Name);
//            entity.Attributes.Add("asyncautodelete", false);
//            entity.Attributes.Add("rank", step.ExecutionOrder);
//            entity.Attributes.Add("mode", new OptionSetValue(step.ExecutionMode));
//            entity.Attributes.Add("plugintypeid", new EntityReference("plugintype", pluginType.Id));
//            entity.Attributes.Add("sdkmessageid", new EntityReference("sdkmessage", message.Id));
//            entity.Attributes.Add("stage", new OptionSetValue(step.ExecutionStage));
//            entity.Attributes.Add("filteringattributes", step.FilteredAttributes);
//            entity.Attributes.Add("supporteddeployment", new OptionSetValue(step.Deployment));
//            entity.Attributes.Add("description", Common.LoggerFactory.GetSyncDescription());
//            entity.Attributes.Add("impersonatinguserid", step.UserContext == Guid.Empty ? null : new EntityReference("systemuser", step.UserContext));
//            entity.Attributes.Add("sdkmessagefilterid", string.IsNullOrEmpty(step.LogicalName) ? null : new EntityReference("sdkmessagefilter", messageFilter.Id));

//            var parameters = new ParameterCollection();
//            parameters.Add("SolutionUniqueName", solutionName);

//            var req = new CreateRequest
//            {
//                Target = entity
//            };
//            req.Parameters.AddRange(parameters);
//            step.Id = ((CreateResponse)service.Execute(req)).id;

//            return step;
//        }).ToList();
//    }
//    public List<PluginImageEntity> CreatePluginImages(List<PluginImageEntity> pluginImages, List<PluginStepEntity> pluginSteps)
//    {
//        return pluginImages.Select(image =>
//        {
//            var pluginStep = pluginSteps.First(step => step.Name == image.PluginStepName);
//            var messagePropertyName = Message.GetMessagePropertyName(image.EventOperation);

//            var entity = new Entity("sdkmessageprocessingstepimage");
//            entity.Attributes.Add("name", image.Name);
//            entity.Attributes.Add("entityalias", image.EntityAlias);
//            entity.Attributes.Add("imagetype", new OptionSetValue(image.ImageType));
//            entity.Attributes.Add("attributes", image.Attributes);
//            entity.Attributes.Add("messagepropertyname", messagePropertyName);
//            entity.Attributes.Add("sdkmessageprocessingstepid", new EntityReference("sdkmessageprocessingstep", pluginStep.Id));

//            image.Id = service.Create(entity);
//            return image;
//        }).ToList();
//    }
//    public List<Entity> GetPluginTypes(Guid assemblyId)
//    {
//        FilterExpression filter = new();
//        filter.AddCondition(new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assemblyId));
//        QueryExpression query = new("plugintype")
//        {
//            ColumnSet = new ColumnSet(allColumns: true),
//            Criteria = filter
//        };
//        return crmDataHelper.RetrieveMultiple(query);
//    }
//    public List<Entity> GetPluginSteps(Guid solutionId)
//    {
//        LinkEntity link = new()
//        {
//            JoinOperator = JoinOperator.Inner,
//            LinkFromAttributeName = "sdkmessageprocessingstepid",
//            LinkFromEntityName = "sdkmessageprocessingstep",
//            LinkToAttributeName = "objectid",
//            LinkToEntityName = "solutioncomponent"
//        };
//        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));

//        FilterExpression filter = new();
//        QueryExpression query = new("sdkmessageprocessingstep")
//        {
//            ColumnSet = new ColumnSet(allColumns: true)
//        };
//        query.LinkEntities.Add(link);
//        query.Criteria = filter;

//        return crmDataHelper.RetrieveMultiple(query);
//    }
//    public List<Entity> GetPluginSteps(Guid solutionId, Guid pluginTypeId)
//    {
//        LinkEntity link = new()
//        {
//            JoinOperator = JoinOperator.Inner,
//            LinkFromAttributeName = "sdkmessageprocessingstepid",
//            LinkFromEntityName = "sdkmessageprocessingstep",
//            LinkToAttributeName = "objectid",
//            LinkToEntityName = "solutioncomponent"
//        };
//        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));

//        FilterExpression filter = new();
//        filter.AddCondition(new ConditionExpression("plugintypeid", ConditionOperator.Equal, pluginTypeId));
//        QueryExpression query = new("sdkmessageprocessingstep")
//        {
//            ColumnSet = new ColumnSet(allColumns: true)
//        };
//        query.LinkEntities.Add(link);
//        query.Criteria = filter;

//        return crmDataHelper.RetrieveMultiple(query);
//    }
//    public List<Entity> GetPluginImages(Guid stepId)
//    {
//        FilterExpression filter = new();
//        filter.AddCondition(new ConditionExpression("sdkmessageprocessingstepid", ConditionOperator.Equal, stepId));
//        QueryExpression query = new("sdkmessageprocessingstepimage")
//        {
//            ColumnSet = new ColumnSet(allColumns: true),
//            Criteria = filter
//        };
//        return crmDataHelper.RetrieveMultiple(query);
//    }
//    public void ActivateOrDeactivatePluginSteps(ActivateOrDeactivatePluginsRequest request)
//    {
//        log.LogAndValidateRequest(request);

//        Guid solutionId;
//        if (!string.IsNullOrEmpty(request.SolutionName))
//        {
//            solutionId = solution.GetSolutionId(request.SolutionName);
//        }
//        else
//        {
//            var solutionInformation = SolutionFileUtility.GetSolutionInformationFromFile(request.SolutionPath);
//            solutionId = solution.GetSolutionId(solutionInformation.SolutionName);
//        }
        
//        var pluginSteps = GetPluginSteps(solutionId);

//        var updateStateReqs = pluginSteps.Select(x =>
//        {
//            var newPluginStep = new Entity(x.LogicalName, x.Id);
//            // Plugin: stateCode = 1 and statusCode = 2 (inactive), 
//            //         stateCode = 0 and statusCode = 1 (active) 
//            // Remark: statusCode = -1, will default the statuscode for the given statecode
//            newPluginStep.Attributes.Add("statecode", new OptionSetValue(request.Activate ? 0 : 1));
//            newPluginStep.Attributes.Add("statuscode", new OptionSetValue(-1));

//            return new UpdateRequest
//            {
//                Target = newPluginStep
//            };
//        }).ToList();

//        crmDataHelper.PerformAsBulkWithOutput(updateStateReqs, log);
//    }
//    public void ValidatePlugins(List<PluginTypeEntity> pluginTypes)
//    {
//        List<Exception> exceptions = new();
//        var pluginSteps = pluginTypes.SelectMany(x => x.PluginSteps);
//        var preOperationAsyncPlugins = pluginSteps
//            .Where(x =>
//            x.ExecutionMode == (int)ExecutionMode.Asynchronous &&
//            x.ExecutionStage != (int)ExecutionStage.Post)
//            .ToList();
//        exceptions.AddRange(preOperationAsyncPlugins.Select(x => new Exception($"Plugin {x.Name}: Pre execution stages does not support asynchronous execution mode")));

//        var preOperationWithPostImagesPlugins = pluginSteps
//            .Where(x =>
//            {
//                var postImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PostImage);

//                return
//                (x.ExecutionStage == (int)ExecutionStage.Pre ||
//                 x.ExecutionStage == (int)ExecutionStage.PreValidation) && postImages.Any();
//            });
//        exceptions.AddRange(preOperationWithPostImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Pre execution stages does not support post-images")));

//        var associateDisassociateWithFilterPlugins = pluginSteps
//            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
//            .Where(x => x.FilteredAttributes != null);
//        exceptions.AddRange(associateDisassociateWithFilterPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events can't have filtered attributes")));

//        var associateDisassociateWithImagesPlugins = pluginSteps
//            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
//            .Where(x => x.PluginImages.Any());
//        exceptions.AddRange(associateDisassociateWithImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events can't have images")));

//        var associateDisassociateNotAllEntitiesPlugins = pluginSteps
//            .Where(x => x.EventOperation == "Associate" || x.EventOperation == "Disassociate")
//            .Where(x => x.LogicalName != "");
//        exceptions.AddRange(associateDisassociateNotAllEntitiesPlugins.Select(x => new Exception($"Plugin {x.Name}: Associate/Disassociate events must target all entities")));

//        var createWithPreImagesPlugins = pluginSteps
//            .Where(x =>
//            {
//                var preImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PreImage);
//                return x.EventOperation == "Create" && preImages.Any();
//            });
//        exceptions.AddRange(createWithPreImagesPlugins.Select(x => new Exception($"Plugin {x.Name}: Create events does not support pre-images")));

//        var deleteWithPostImagesPLugins = pluginSteps
//            .Where(x =>
//            {
//                var postImages = x.PluginImages.Where(image => image.ImageType == (int)ImageType.PostImage);
//                return x.EventOperation == "Delete" && postImages.Any();
//            });
//        exceptions.AddRange(deleteWithPostImagesPLugins.Select(x => new Exception($"Plugin {x.Name}: Delete events does not support post-images")));

//        var userContextDoesNotExistPlugins = pluginSteps
//            .Where(x => x.UserContext != Guid.Empty)
//            .Where(x =>
//            {
//                var query = new QueryExpression("systemuser");
//                var filter = new FilterExpression();
//                filter.AddCondition(new ConditionExpression("systemuserid", ConditionOperator.Equal, x.UserContext));
//                query.Criteria = filter;
//                query.ColumnSet = new ColumnSet(null);

//                var user = crmDataHelper.RetrieveFirstOrDefault(query);
//                return user == null;
//            });
//        exceptions.AddRange(userContextDoesNotExistPlugins.Select(x => new Exception($"Plugin {x.Name}: Defined user context is not in the system")));

//        if (exceptions.Count == 1) throw exceptions.First();
//        else if (exceptions.Count > 1) throw new AggregateException("Some plugins can't be validated", exceptions);
//    }

//    //public PluginAssembly GetAssemblyDTO(string dllPath)
//    //{
//    //    var dllFullPath = Path.GetFullPath(dllPath);
//    //    var assemblyWriteTime = File.GetLastWriteTimeUtc(dllFullPath);

//    //    var dllTempPath = FileUtility.CopyFileToTempPath(dllPath, ".dll");
//    //    var dllname = Path.GetFileNameWithoutExtension(dllPath);
//    //    var hash = CryptographyUtility.Sha1Checksum(File.ReadAllBytes(dllTempPath));

//    //    using (var assemblyReader = new AssemblyReader.AssemblyReader())
//    //    {
//    //        var assembly = assemblyReader.ReadAssembly(dllTempPath);
//    //        var pluginTypes = GetPluginTypesFromAssembly(assembly);

//    //        return new PluginAssembly
//    //        {
//    //            AssemblyVersion = assembly.GetName().Version,
//    //            DllName = dllname,
//    //            DllPath = dllFullPath,
//    //            Hash = hash,
//    //            PluginTypes = pluginTypes,
//    //        };
//    //    }
//    //}

//    private List<PluginTypeEntity> GetPluginTypesFromAssembly(Assembly assembly)
//    {
//        var types = GetLoadableTypes(assembly);
//        var pluginType = types.First(x => x.Name == "Plugin");
//        var plugins = types.Where(x => x.IsSubclassOf(pluginType));
//        var validPlugins = plugins.Where(x => !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null);
//        var invalidPlugins = plugins.Where(x => !(!x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null));
//        foreach (var plugin in invalidPlugins)
//        {
//            if (plugin.IsAbstract)
//                log.LogInformation($"The plugin '{plugin.Name}' is an abstract type and is therefore not valid. The plugin will not be synchronized");
//            if (plugin.GetConstructor(Type.EmptyTypes) == null)
//                log.LogInformation($"The plugin '{plugin.Name}' does not contain an empty contructor and is therefore not valid. The plugin will not be synchronized");
//        }

//        var pluginTypes = validPlugins
//        .SelectMany(x =>
//        {
//            var instance = Activator.CreateInstance(x);
//            var methodInfo = x.GetMethod(@"PluginProcessingStepConfigs");
//            var pluginTuples = (IEnumerable<Tuple<StepConfig, ExtendedStepConfig, IEnumerable<ImageTuple>>>)methodInfo.Invoke(instance, null);
//            return pluginTuples
//                .Select(tuple =>
//                {
//                    var (className, stage, eventOp, logicalName) = tuple.Item1;
//                    var (deployment, mode, notUsedStepname, executionOrder, filteredAttr, userId) = tuple.Item2;
//                    List<ImageTuple> imageTuples = tuple.Item3.ToList();

//                    var entity = string.IsNullOrEmpty(logicalName) ? "any Entity" : logicalName;
//                    var stepName = $"{className}: {Enum.GetName(typeof(ExecutionMode), mode)} {Enum.GetName(typeof(ExecutionStage), stage)} {eventOp} of {entity}";

//                    var images = imageTuples
//                        .Select(image =>
//                        {
//                            var (iName, iAlias, iType, iAttr) = image;

//                            return new PluginImageEntity
//                            {
//                                Id = Guid.Empty,
//                                PluginStepName = stepName,
//                                Name = iName,
//                                EntityAlias = iAlias,
//                                ImageType = iType,
//                                Attributes = iAttr,
//                                EventOperation = eventOp,
//                            };
//                        }).ToList();

//                    var step = new PluginStepEntity
//                    {
//                        Id = Guid.Empty,
//                        ExecutionStage = stage,
//                        Deployment = deployment,
//                        ExecutionMode = mode,
//                        ExecutionOrder = executionOrder,
//                        FilteredAttributes = filteredAttr,
//                        UserContext = new Guid(userId),
//                        PluginTypeName = className,
//                        Name = stepName,
//                        PluginImages = images,

//                        EventOperation = eventOp,
//                        LogicalName = logicalName,
//                    };

//                    return new PluginTypeEntity
//                    {
//                        Id = Guid.Empty,
//                        Name = className,
//                        PluginSteps = [step],
//                    };
//                });
//        }).ToList();

//        return pluginTypes;
//    }

//    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
//    {
//        try
//        {
//            return assembly.GetTypes();
//        }
//        catch (ReflectionTypeLoadException e)
//        {
//            return e.Types.Where(t => t != null);
//        }
//    }
//}
