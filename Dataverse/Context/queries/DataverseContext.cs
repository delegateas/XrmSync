using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
public class DataverseContext : OrganizationServiceContext
{
    public DataverseContext(IOrganizationService service)
        : base(service)
    {
    }

    public IQueryable<ActivityParty> ActivityPartySet
    {
        get { return CreateQuery<ActivityParty>(); }
    }

    public IQueryable<CustomAPI> CustomAPISet
    {
        get { return CreateQuery<CustomAPI>(); }
    }

    public IQueryable<CustomAPIRequestParameter> CustomAPIRequestParameterSet
    {
        get { return CreateQuery<CustomAPIRequestParameter>(); }
    }

    public IQueryable<CustomAPIResponseProperty> CustomAPIResponsePropertySet
    {
        get { return CreateQuery<CustomAPIResponseProperty>(); }
    }

    public IQueryable<Dependency> DependencySet
    {
        get { return CreateQuery<Dependency>(); }
    }

    public IQueryable<DependencyNode> DependencyNodeSet
    {
        get { return CreateQuery<DependencyNode>(); }
    }

    public IQueryable<PluginAssembly> PluginAssemblySet
    {
        get { return CreateQuery<PluginAssembly>(); }
    }

    public IQueryable<PluginType> PluginTypeSet
    {
        get { return CreateQuery<PluginType>(); }
    }

    public IQueryable<Publisher> PublisherSet
    {
        get { return CreateQuery<Publisher>(); }
    }

    public IQueryable<SdkMessage> SdkMessageSet
    {
        get { return CreateQuery<SdkMessage>(); }
    }

    public IQueryable<SdkMessageFilter> SdkMessageFilterSet
    {
        get { return CreateQuery<SdkMessageFilter>(); }
    }

    public IQueryable<SdkMessageProcessingStep> SdkMessageProcessingStepSet
    {
        get { return CreateQuery<SdkMessageProcessingStep>(); }
    }

    public IQueryable<SdkMessageProcessingStepImage> SdkMessageProcessingStepImageSet
    {
        get { return CreateQuery<SdkMessageProcessingStepImage>(); }
    }

    public IQueryable<Solution> SolutionSet
    {
        get { return CreateQuery<Solution>(); }
    }

    public IQueryable<SolutionComponent> SolutionComponentSet
    {
        get { return CreateQuery<SolutionComponent>(); }
    }

    public IQueryable<SystemUser> SystemUserSet
    {
        get { return CreateQuery<SystemUser>(); }
    }

    public IQueryable<WebResource> WebResourceSet
    {
        get { return CreateQuery<WebResource>(); }
    }
}