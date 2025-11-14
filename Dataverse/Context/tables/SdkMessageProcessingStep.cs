using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Stage in the execution pipeline that a plug-in is to execute.</para>
/// <para>Display Name: Sdk Message Processing Step</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("sdkmessageprocessingstep")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class SdkMessageProcessingStep : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "sdkmessageprocessingstep";
    public const int EntityTypeCode = 4608;

    public SdkMessageProcessingStep() : base(EntityLogicalName) { }
    public SdkMessageProcessingStep(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("name");

    [AttributeLogicalName("sdkmessageprocessingstepid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("sdkmessageprocessingstepid", value);
        }
    }

    /// <summary>
    /// <para>Indicates whether the asynchronous system job is automatically deleted on completion.</para>
    /// <para>Display Name: Asynchronous Automatic Delete</para>
    /// </summary>
    [AttributeLogicalName("asyncautodelete")]
    [DisplayName("Asynchronous Automatic Delete")]
    public bool? AsyncAutoDelete
    {
        get => GetAttributeValue<bool?>("asyncautodelete");
        set => SetAttributeValue("asyncautodelete", value);
    }

    /// <summary>
    /// <para>Display Name: canbebypassed</para>
    /// </summary>
    [AttributeLogicalName("canbebypassed")]
    [DisplayName("canbebypassed")]
    public bool? CanBeBypassed
    {
        get => GetAttributeValue<bool?>("canbebypassed");
        set => SetAttributeValue("canbebypassed", value);
    }

    /// <summary>
    /// <para>Identifies whether a SDK Message Processing Step type will be ReadOnly or Read Write. false - ReadWrite, true - ReadOnly</para>
    /// <para>Display Name: Intent</para>
    /// </summary>
    [AttributeLogicalName("canusereadonlyconnection")]
    [DisplayName("Intent")]
    public bool? CanUseReadOnlyConnection
    {
        get => GetAttributeValue<bool?>("canusereadonlyconnection");
        set => SetAttributeValue("canusereadonlyconnection", value);
    }

    /// <summary>
    /// <para>For internal use only.</para>
    /// <para>Display Name: Category</para>
    /// </summary>
    [AttributeLogicalName("category")]
    [DisplayName("Category")]
    [MaxLength(100)]
    public string Category
    {
        get => GetAttributeValue<string>("category");
        set => SetAttributeValue("category", value);
    }

    /// <summary>
    /// <para>For internal use only.</para>
    /// <para>Display Name: Component State</para>
    /// </summary>
    [AttributeLogicalName("componentstate")]
    [DisplayName("Component State")]
    public componentstate? ComponentState
    {
        get => this.GetOptionSetValue<componentstate>("componentstate");
        set => this.SetOptionSetValue("componentstate", value);
    }

    /// <summary>
    /// <para>Step-specific configuration for the plug-in type. Passed to the plug-in constructor at run time.</para>
    /// <para>Display Name: Configuration</para>
    /// </summary>
    [AttributeLogicalName("configuration")]
    [DisplayName("Configuration")]
    [MaxLength(1073741823)]
    public string Configuration
    {
        get => GetAttributeValue<string>("configuration");
        set => SetAttributeValue("configuration", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user who created the SDK message processing step.</para>
    /// <para>Display Name: Created By</para>
    /// </summary>
    [AttributeLogicalName("createdby")]
    [DisplayName("Created By")]
    public EntityReference? CreatedBy
    {
        get => GetAttributeValue<EntityReference?>("createdby");
        set => SetAttributeValue("createdby", value);
    }

    /// <summary>
    /// <para>Date and time when the SDK message processing step was created.</para>
    /// <para>Display Name: Created On</para>
    /// </summary>
    [AttributeLogicalName("createdon")]
    [DisplayName("Created On")]
    public DateTime? CreatedOn
    {
        get => GetAttributeValue<DateTime?>("createdon");
        set => SetAttributeValue("createdon", value);
    }

    /// <summary>
    /// <para>Unique identifier of the delegate user who created the sdkmessageprocessingstep.</para>
    /// <para>Display Name: Created By (Delegate)</para>
    /// </summary>
    [AttributeLogicalName("createdonbehalfby")]
    [DisplayName("Created By (Delegate)")]
    public EntityReference? CreatedOnBehalfBy
    {
        get => GetAttributeValue<EntityReference?>("createdonbehalfby");
        set => SetAttributeValue("createdonbehalfby", value);
    }

    /// <summary>
    /// <para>Customization level of the SDK message processing step.</para>
    /// <para>Display Name: customizationlevel</para>
    /// </summary>
    [AttributeLogicalName("customizationlevel")]
    [DisplayName("customizationlevel")]
    [Range(-255, 255)]
    public int? CustomizationLevel
    {
        get => GetAttributeValue<int?>("customizationlevel");
        set => SetAttributeValue("customizationlevel", value);
    }

    /// <summary>
    /// <para>Description of the SDK message processing step.</para>
    /// <para>Display Name: Description</para>
    /// </summary>
    [AttributeLogicalName("description")]
    [DisplayName("Description")]
    [MaxLength(256)]
    public string Description
    {
        get => GetAttributeValue<string>("description");
        set => SetAttributeValue("description", value);
    }

    /// <summary>
    /// <para>EnablePluginProfiler</para>
    /// <para>Display Name: EnablePluginProfiler</para>
    /// </summary>
    [AttributeLogicalName("enablepluginprofiler")]
    [DisplayName("EnablePluginProfiler")]
    public bool? EnablePluginProfiler
    {
        get => GetAttributeValue<bool?>("enablepluginprofiler");
        set => SetAttributeValue("enablepluginprofiler", value);
    }

    /// <summary>
    /// <para>Configuration for sending pipeline events to the Event Expander service.</para>
    /// <para>Display Name: EventExpander</para>
    /// </summary>
    [AttributeLogicalName("eventexpander")]
    [DisplayName("EventExpander")]
    [MaxLength(1073741823)]
    public string EventExpander
    {
        get => GetAttributeValue<string>("eventexpander");
        set => SetAttributeValue("eventexpander", value);
    }

    /// <summary>
    /// <para>Unique identifier of the associated event handler.</para>
    /// <para>Display Name: Event Handler</para>
    /// </summary>
    [AttributeLogicalName("eventhandler")]
    [DisplayName("Event Handler")]
    public EntityReference? EventHandler
    {
        get => GetAttributeValue<EntityReference?>("eventhandler");
        set => SetAttributeValue("eventhandler", value);
    }

    /// <summary>
    /// <para>Comma-separated list of attributes. If at least one of these attributes is modified, the plug-in should execute.</para>
    /// <para>Display Name: Filtering Attributes</para>
    /// </summary>
    [AttributeLogicalName("filteringattributes")]
    [DisplayName("Filtering Attributes")]
    [MaxLength(100000)]
    public string FilteringAttributes
    {
        get => GetAttributeValue<string>("filteringattributes");
        set => SetAttributeValue("filteringattributes", value);
    }

    /// <summary>
    /// <para>Unique identifier for fxexpression associated with SdkMessageProcessingStep.</para>
    /// <para>Display Name: fxexpressionid</para>
    /// </summary>
    [AttributeLogicalName("fxexpressionid")]
    [DisplayName("fxexpressionid")]
    public EntityReference? FxExpressionId
    {
        get => GetAttributeValue<EntityReference?>("fxexpressionid");
        set => SetAttributeValue("fxexpressionid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user to impersonate context when step is executed.</para>
    /// <para>Display Name: Impersonating User</para>
    /// </summary>
    [AttributeLogicalName("impersonatinguserid")]
    [DisplayName("Impersonating User")]
    public EntityReference? ImpersonatingUserId
    {
        get => GetAttributeValue<EntityReference?>("impersonatinguserid");
        set => SetAttributeValue("impersonatinguserid", value);
    }

    /// <summary>
    /// <para>Version in which the form is introduced.</para>
    /// <para>Display Name: Introduced Version</para>
    /// </summary>
    [AttributeLogicalName("introducedversion")]
    [DisplayName("Introduced Version")]
    [MaxLength(48)]
    public string IntroducedVersion
    {
        get => GetAttributeValue<string>("introducedversion");
        set => SetAttributeValue("introducedversion", value);
    }

    /// <summary>
    /// <para>Identifies if a plug-in should be executed from a parent pipeline, a child pipeline, or both.</para>
    /// <para>Display Name: Invocation Source</para>
    /// </summary>
    [AttributeLogicalName("invocationsource")]
    [DisplayName("Invocation Source")]
    public sdkmessageprocessingstep_invocationsource? InvocationSource
    {
        get => this.GetOptionSetValue<sdkmessageprocessingstep_invocationsource>("invocationsource");
        set => this.SetOptionSetValue("invocationsource", value);
    }

    /// <summary>
    /// <para>Information that specifies whether this component can be customized.</para>
    /// <para>Display Name: Customizable</para>
    /// </summary>
    [AttributeLogicalName("iscustomizable")]
    [DisplayName("Customizable")]
    public BooleanManagedProperty IsCustomizable
    {
        get => GetAttributeValue<BooleanManagedProperty>("iscustomizable");
        set => SetAttributeValue("iscustomizable", value);
    }

    /// <summary>
    /// <para>Information that specifies whether this component should be hidden.</para>
    /// <para>Display Name: Hidden</para>
    /// </summary>
    [AttributeLogicalName("ishidden")]
    [DisplayName("Hidden")]
    public BooleanManagedProperty IsHidden
    {
        get => GetAttributeValue<BooleanManagedProperty>("ishidden");
        set => SetAttributeValue("ishidden", value);
    }

    /// <summary>
    /// <para>Information that specifies whether this component is managed.</para>
    /// <para>Display Name: State</para>
    /// </summary>
    [AttributeLogicalName("ismanaged")]
    [DisplayName("State")]
    public bool? IsManaged
    {
        get => GetAttributeValue<bool?>("ismanaged");
        set => SetAttributeValue("ismanaged", value);
    }

    /// <summary>
    /// <para>Run-time mode of execution, for example, synchronous or asynchronous.</para>
    /// <para>Display Name: Execution Mode</para>
    /// </summary>
    [AttributeLogicalName("mode")]
    [DisplayName("Execution Mode")]
    public sdkmessageprocessingstep_mode? Mode
    {
        get => this.GetOptionSetValue<sdkmessageprocessingstep_mode>("mode");
        set => this.SetOptionSetValue("mode", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user who last modified the SDK message processing step.</para>
    /// <para>Display Name: Modified By</para>
    /// </summary>
    [AttributeLogicalName("modifiedby")]
    [DisplayName("Modified By")]
    public EntityReference? ModifiedBy
    {
        get => GetAttributeValue<EntityReference?>("modifiedby");
        set => SetAttributeValue("modifiedby", value);
    }

    /// <summary>
    /// <para>Date and time when the SDK message processing step was last modified.</para>
    /// <para>Display Name: Modified On</para>
    /// </summary>
    [AttributeLogicalName("modifiedon")]
    [DisplayName("Modified On")]
    public DateTime? ModifiedOn
    {
        get => GetAttributeValue<DateTime?>("modifiedon");
        set => SetAttributeValue("modifiedon", value);
    }

    /// <summary>
    /// <para>Unique identifier of the delegate user who last modified the sdkmessageprocessingstep.</para>
    /// <para>Display Name: Modified By (Delegate)</para>
    /// </summary>
    [AttributeLogicalName("modifiedonbehalfby")]
    [DisplayName("Modified By (Delegate)")]
    public EntityReference? ModifiedOnBehalfBy
    {
        get => GetAttributeValue<EntityReference?>("modifiedonbehalfby");
        set => SetAttributeValue("modifiedonbehalfby", value);
    }

    /// <summary>
    /// <para>Name of SdkMessage processing step.</para>
    /// <para>Display Name: Name</para>
    /// </summary>
    [AttributeLogicalName("name")]
    [DisplayName("Name")]
    [MaxLength(256)]
    public string Name
    {
        get => GetAttributeValue<string>("name");
        set => SetAttributeValue("name", value);
    }

    /// <summary>
    /// <para>Unique identifier of the organization with which the SDK message processing step is associated.</para>
    /// <para>Display Name: organizationid</para>
    /// </summary>
    [AttributeLogicalName("organizationid")]
    [DisplayName("organizationid")]
    public EntityReference? OrganizationId
    {
        get => GetAttributeValue<EntityReference?>("organizationid");
        set => SetAttributeValue("organizationid", value);
    }

    /// <summary>
    /// <para>For internal use only.</para>
    /// <para>Display Name: Record Overwrite Time</para>
    /// </summary>
    [AttributeLogicalName("overwritetime")]
    [DisplayName("Record Overwrite Time")]
    public DateTime? OverwriteTime
    {
        get => GetAttributeValue<DateTime?>("overwritetime");
        set => SetAttributeValue("overwritetime", value);
    }

    /// <summary>
    /// <para>Unique identifier of the plug-in type associated with the step.</para>
    /// <para>Display Name: Plug-In Type</para>
    /// </summary>
    [AttributeLogicalName("plugintypeid")]
    [DisplayName("Plug-In Type")]
    public EntityReference? PluginTypeId
    {
        get => GetAttributeValue<EntityReference?>("plugintypeid");
        set => SetAttributeValue("plugintypeid", value);
    }

    /// <summary>
    /// <para>Unique identifier for powerfxrule associated with SdkMessageProcessingStep.</para>
    /// <para>Display Name: powerfxruleid</para>
    /// </summary>
    [AttributeLogicalName("powerfxruleid")]
    [DisplayName("powerfxruleid")]
    public EntityReference? PowerfxRuleId
    {
        get => GetAttributeValue<EntityReference?>("powerfxruleid");
        set => SetAttributeValue("powerfxruleid", value);
    }

    /// <summary>
    /// <para>Processing order within the stage.</para>
    /// <para>Display Name: Execution Order</para>
    /// </summary>
    [AttributeLogicalName("rank")]
    [DisplayName("Execution Order")]
    [Range(-2147483648, 2147483647)]
    public int? Rank
    {
        get => GetAttributeValue<int?>("rank");
        set => SetAttributeValue("rank", value);
    }

    /// <summary>
    /// <para>For internal use only. Holds miscellaneous properties related to runtime integration.</para>
    /// <para>Display Name: Runtime Integration Properties</para>
    /// </summary>
    [AttributeLogicalName("runtimeintegrationproperties")]
    [DisplayName("Runtime Integration Properties")]
    [MaxLength(512)]
    public string RuntimeIntegrationProperties
    {
        get => GetAttributeValue<string>("runtimeintegrationproperties");
        set => SetAttributeValue("runtimeintegrationproperties", value);
    }

    /// <summary>
    /// <para>Unique identifier of the SDK message filter.</para>
    /// <para>Display Name: SdkMessage Filter</para>
    /// </summary>
    [AttributeLogicalName("sdkmessagefilterid")]
    [DisplayName("SdkMessage Filter")]
    public EntityReference? SdkMessageFilterId
    {
        get => GetAttributeValue<EntityReference?>("sdkmessagefilterid");
        set => SetAttributeValue("sdkmessagefilterid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the SDK message.</para>
    /// <para>Display Name: SDK Message</para>
    /// </summary>
    [AttributeLogicalName("sdkmessageid")]
    [DisplayName("SDK Message")]
    public EntityReference? SdkMessageId
    {
        get => GetAttributeValue<EntityReference?>("sdkmessageid");
        set => SetAttributeValue("sdkmessageid", value);
    }

    /// <summary>
    /// <para>Display Name: sdkmessageprocessingstepid</para>
    /// </summary>
    [AttributeLogicalName("sdkmessageprocessingstepid")]
    [DisplayName("sdkmessageprocessingstepid")]
    public Guid SdkMessageProcessingStepId
    {
        get => GetAttributeValue<Guid>("sdkmessageprocessingstepid");
        set => SetId("sdkmessageprocessingstepid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the SDK message processing step.</para>
    /// <para>Display Name: sdkmessageprocessingstepidunique</para>
    /// </summary>
    [AttributeLogicalName("sdkmessageprocessingstepidunique")]
    [DisplayName("sdkmessageprocessingstepidunique")]
    public Guid? SdkMessageProcessingStepIdUnique
    {
        get => GetAttributeValue<Guid?>("sdkmessageprocessingstepidunique");
        set => SetAttributeValue("sdkmessageprocessingstepidunique", value);
    }

    /// <summary>
    /// <para>Unique identifier of the Sdk message processing step secure configuration.</para>
    /// <para>Display Name: SDK Message Processing Step Secure Configuration</para>
    /// </summary>
    [AttributeLogicalName("sdkmessageprocessingstepsecureconfigid")]
    [DisplayName("SDK Message Processing Step Secure Configuration")]
    public EntityReference? SdkMessageProcessingStepSecureConfigId
    {
        get => GetAttributeValue<EntityReference?>("sdkmessageprocessingstepsecureconfigid");
        set => SetAttributeValue("sdkmessageprocessingstepsecureconfigid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the associated solution.</para>
    /// <para>Display Name: Solution</para>
    /// </summary>
    [AttributeLogicalName("solutionid")]
    [DisplayName("Solution")]
    public Guid? SolutionId
    {
        get => GetAttributeValue<Guid?>("solutionid");
        set => SetAttributeValue("solutionid", value);
    }

    /// <summary>
    /// <para>Stage in the execution pipeline that the SDK message processing step is in.</para>
    /// <para>Display Name: Execution Stage</para>
    /// </summary>
    [AttributeLogicalName("stage")]
    [DisplayName("Execution Stage")]
    public sdkmessageprocessingstep_stage? Stage
    {
        get => this.GetOptionSetValue<sdkmessageprocessingstep_stage>("stage");
        set => this.SetOptionSetValue("stage", value);
    }

    /// <summary>
    /// <para>Status of the SDK message processing step.</para>
    /// <para>Display Name: Status</para>
    /// </summary>
    [AttributeLogicalName("statecode")]
    [DisplayName("Status")]
    public sdkmessageprocessingstep_statecode? StateCode
    {
        get => this.GetOptionSetValue<sdkmessageprocessingstep_statecode>("statecode");
        set => this.SetOptionSetValue("statecode", value);
    }

    /// <summary>
    /// <para>Reason for the status of the SDK message processing step.</para>
    /// <para>Display Name: Status Reason</para>
    /// </summary>
    [AttributeLogicalName("statuscode")]
    [DisplayName("Status Reason")]
    public sdkmessageprocessingstep_statuscode? StatusCode
    {
        get => this.GetOptionSetValue<sdkmessageprocessingstep_statuscode>("statuscode");
        set => this.SetOptionSetValue("statuscode", value);
    }

    /// <summary>
    /// <para>Deployment that the SDK message processing step should be executed on; server, client, or both.</para>
    /// <para>Display Name: Deployment</para>
    /// </summary>
    [AttributeLogicalName("supporteddeployment")]
    [DisplayName("Deployment")]
    public sdkmessageprocessingstep_supporteddeployment? SupportedDeployment
    {
        get => this.GetOptionSetValue<sdkmessageprocessingstep_supporteddeployment>("supporteddeployment");
        set => this.SetOptionSetValue("supporteddeployment", value);
    }

    /// <summary>
    /// <para>For internal use only.</para>
    /// <para>Display Name: Solution</para>
    /// </summary>
    [AttributeLogicalName("supportingsolutionid")]
    [DisplayName("Solution")]
    public Guid? SupportingSolutionId
    {
        get => GetAttributeValue<Guid?>("supportingsolutionid");
        set => SetAttributeValue("supportingsolutionid", value);
    }

    /// <summary>
    /// <para>Number that identifies a specific revision of the SDK message processing step.</para>
    /// <para>Display Name: versionnumber</para>
    /// </summary>
    [AttributeLogicalName("versionnumber")]
    [DisplayName("versionnumber")]
    public long? VersionNumber
    {
        get => GetAttributeValue<long?>("versionnumber");
        set => SetAttributeValue("versionnumber", value);
    }

    [AttributeLogicalName("impersonatinguserid")]
    [RelationshipSchemaName("impersonatinguserid_sdkmessageprocessingstep")]
    [RelationshipMetadata("ManyToOne", "impersonatinguserid", "systemuser", "systemuserid", "Referencing")]
    public SystemUser impersonatinguserid_sdkmessageprocessingstep
    {
        get => GetRelatedEntity<SystemUser>("impersonatinguserid_sdkmessageprocessingstep", null);
        set => SetRelatedEntity("impersonatinguserid_sdkmessageprocessingstep", null, value);
    }

    [AttributeLogicalName("sdkmessagefilterid")]
    [RelationshipSchemaName("sdkmessagefilterid_sdkmessageprocessingstep")]
    [RelationshipMetadata("ManyToOne", "sdkmessagefilterid", "sdkmessagefilter", "sdkmessagefilterid", "Referencing")]
    public SdkMessageFilter sdkmessagefilterid_sdkmessageprocessingstep
    {
        get => GetRelatedEntity<SdkMessageFilter>("sdkmessagefilterid_sdkmessageprocessingstep", null);
        set => SetRelatedEntity("sdkmessagefilterid_sdkmessageprocessingstep", null, value);
    }

    [AttributeLogicalName("eventhandler")]
    [RelationshipSchemaName("plugintype_sdkmessageprocessingstep")]
    [RelationshipMetadata("ManyToOne", "eventhandler", "plugintype", "plugintypeid", "Referencing")]
    public PluginType plugintype_sdkmessageprocessingstep
    {
        get => GetRelatedEntity<PluginType>("plugintype_sdkmessageprocessingstep", null);
        set => SetRelatedEntity("plugintype_sdkmessageprocessingstep", null, value);
    }

    [AttributeLogicalName("plugintypeid")]
    [RelationshipSchemaName("plugintypeid_sdkmessageprocessingstep")]
    [RelationshipMetadata("ManyToOne", "plugintypeid", "plugintype", "plugintypeid", "Referencing")]
    public PluginType plugintypeid_sdkmessageprocessingstep
    {
        get => GetRelatedEntity<PluginType>("plugintypeid_sdkmessageprocessingstep", null);
        set => SetRelatedEntity("plugintypeid_sdkmessageprocessingstep", null, value);
    }

    [AttributeLogicalName("createdby")]
    [RelationshipSchemaName("createdby_sdkmessageprocessingstep")]
    [RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser createdby_sdkmessageprocessingstep
    {
        get => GetRelatedEntity<SystemUser>("createdby_sdkmessageprocessingstep", null);
        set => SetRelatedEntity("createdby_sdkmessageprocessingstep", null, value);
    }

    [AttributeLogicalName("modifiedonbehalfby")]
    [RelationshipSchemaName("lk_sdkmessageprocessingstep_modifiedonbehalfby")]
    [RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_sdkmessageprocessingstep_modifiedonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_sdkmessageprocessingstep_modifiedonbehalfby", null);
        set => SetRelatedEntity("lk_sdkmessageprocessingstep_modifiedonbehalfby", null, value);
    }

    [AttributeLogicalName("sdkmessageid")]
    [RelationshipSchemaName("sdkmessageid_sdkmessageprocessingstep")]
    [RelationshipMetadata("ManyToOne", "sdkmessageid", "sdkmessage", "sdkmessageid", "Referencing")]
    public SdkMessage sdkmessageid_sdkmessageprocessingstep
    {
        get => GetRelatedEntity<SdkMessage>("sdkmessageid_sdkmessageprocessingstep", null);
        set => SetRelatedEntity("sdkmessageid_sdkmessageprocessingstep", null, value);
    }

    [AttributeLogicalName("modifiedby")]
    [RelationshipSchemaName("modifiedby_sdkmessageprocessingstep")]
    [RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser modifiedby_sdkmessageprocessingstep
    {
        get => GetRelatedEntity<SystemUser>("modifiedby_sdkmessageprocessingstep", null);
        set => SetRelatedEntity("modifiedby_sdkmessageprocessingstep", null, value);
    }

    [AttributeLogicalName("createdonbehalfby")]
    [RelationshipSchemaName("lk_sdkmessageprocessingstep_createdonbehalfby")]
    [RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_sdkmessageprocessingstep_createdonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_sdkmessageprocessingstep_createdonbehalfby", null);
        set => SetRelatedEntity("lk_sdkmessageprocessingstep_createdonbehalfby", null, value);
    }

    [RelationshipSchemaName("sdkmessageprocessingstepid_sdkmessageprocessingstepimage")]
    [RelationshipMetadata("OneToMany", "sdkmessageprocessingstepid", "sdkmessageprocessingstepimage", "sdkmessageprocessingstepid", "Referenced")]
    public IEnumerable<SdkMessageProcessingStepImage> sdkmessageprocessingstepid_sdkmessageprocessingstepimage
    {
        get => GetRelatedEntities<SdkMessageProcessingStepImage>("sdkmessageprocessingstepid_sdkmessageprocessingstepimage", null);
        set => SetRelatedEntities("sdkmessageprocessingstepid_sdkmessageprocessingstepimage", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the SdkMessageProcessingStep entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<SdkMessageProcessingStep, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the SdkMessageProcessingStep with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of SdkMessageProcessingStep to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved SdkMessageProcessingStep</returns>
    public static SdkMessageProcessingStep Retrieve(IOrganizationService service, Guid id, params Expression<Func<SdkMessageProcessingStep, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }
}