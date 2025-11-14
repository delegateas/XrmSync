using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Filter that defines which SDK messages are valid for each type of entity.</para>
/// <para>Display Name: Sdk Message Filter</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("sdkmessagefilter")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class SdkMessageFilter : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "sdkmessagefilter";
    public const int EntityTypeCode = 4607;

    public SdkMessageFilter() : base(EntityLogicalName) { }
    public SdkMessageFilter(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("name");

    [AttributeLogicalName("sdkmessagefilterid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("sdkmessagefilterid", value);
        }
    }

    /// <summary>
    /// <para>Identifies where a method will be exposed. 0 - Server, 1 - Client, 2 - both.</para>
    /// <para>Display Name: Availability</para>
    /// </summary>
    [AttributeLogicalName("availability")]
    [DisplayName("Availability")]
    [Range(-2147483648, 2147483647)]
    public int? Availability
    {
        get => GetAttributeValue<int?>("availability");
        set => SetAttributeValue("availability", value);
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
    /// <para>Unique identifier of the user who created the SDK message filter.</para>
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
    /// <para>Date and time when the SDK message filter was created.</para>
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
    /// <para>Unique identifier of the delegate user who created the sdkmessagefilter.</para>
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
    /// <para>Customization level of the SDK message filter.</para>
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
    /// <para>Version in which the component is introduced.</para>
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
    /// <para>Indicates whether a custom SDK message processing step is allowed.</para>
    /// <para>Display Name: Custom Processing Step Allowed</para>
    /// </summary>
    [AttributeLogicalName("iscustomprocessingstepallowed")]
    [DisplayName("Custom Processing Step Allowed")]
    public bool? IsCustomProcessingStepAllowed
    {
        get => GetAttributeValue<bool?>("iscustomprocessingstepallowed");
        set => SetAttributeValue("iscustomprocessingstepallowed", value);
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
    /// <para>Indicates whether the filter should be visible.</para>
    /// <para>Display Name: isvisible</para>
    /// </summary>
    [AttributeLogicalName("isvisible")]
    [DisplayName("isvisible")]
    public bool? IsVisible
    {
        get => GetAttributeValue<bool?>("isvisible");
        set => SetAttributeValue("isvisible", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user who last modified the SDK message filter.</para>
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
    /// <para>Date and time when the SDK message filter was last modified.</para>
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
    /// <para>Unique identifier of the delegate user who last modified the sdkmessagefilter.</para>
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
    /// <para>Name of the SDK message filter.</para>
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
    /// <para>Unique identifier of the organization with which the SDK message filter is associated.</para>
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
    /// <para>Type of entity with which the SDK message filter is primarily associated.</para>
    /// <para>Display Name: Primary Object Type Code</para>
    /// </summary>
    [AttributeLogicalName("primaryobjecttypecode")]
    [DisplayName("Primary Object Type Code")]
    [MaxLength()]
    public string PrimaryObjectTypeCode
    {
        get => GetAttributeValue<string>("primaryobjecttypecode");
        set => SetAttributeValue("primaryobjecttypecode", value);
    }

    /// <summary>
    /// <para>For internal use only.</para>
    /// <para>Display Name: restrictionlevel</para>
    /// </summary>
    [AttributeLogicalName("restrictionlevel")]
    [DisplayName("restrictionlevel")]
    [Range(0, 255)]
    public int? RestrictionLevel
    {
        get => GetAttributeValue<int?>("restrictionlevel");
        set => SetAttributeValue("restrictionlevel", value);
    }

    /// <summary>
    /// <para>Display Name: sdkmessagefilterid</para>
    /// </summary>
    [AttributeLogicalName("sdkmessagefilterid")]
    [DisplayName("sdkmessagefilterid")]
    public Guid SdkMessageFilterId
    {
        get => GetAttributeValue<Guid>("sdkmessagefilterid");
        set => SetId("sdkmessagefilterid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the SDK message filter.</para>
    /// <para>Display Name: sdkmessagefilteridunique</para>
    /// </summary>
    [AttributeLogicalName("sdkmessagefilteridunique")]
    [DisplayName("sdkmessagefilteridunique")]
    public Guid? SdkMessageFilterIdUnique
    {
        get => GetAttributeValue<Guid?>("sdkmessagefilteridunique");
        set => SetAttributeValue("sdkmessagefilteridunique", value);
    }

    /// <summary>
    /// <para>Unique identifier of the related SDK message.</para>
    /// <para>Display Name: SDK Message ID</para>
    /// </summary>
    [AttributeLogicalName("sdkmessageid")]
    [DisplayName("SDK Message ID")]
    public EntityReference? SdkMessageId
    {
        get => GetAttributeValue<EntityReference?>("sdkmessageid");
        set => SetAttributeValue("sdkmessageid", value);
    }

    /// <summary>
    /// <para>Type of entity with which the SDK message filter is secondarily associated.</para>
    /// <para>Display Name: Secondary Object Type Code</para>
    /// </summary>
    [AttributeLogicalName("secondaryobjecttypecode")]
    [DisplayName("Secondary Object Type Code")]
    [MaxLength()]
    public string SecondaryObjectTypeCode
    {
        get => GetAttributeValue<string>("secondaryobjecttypecode");
        set => SetAttributeValue("secondaryobjecttypecode", value);
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
    /// <para>Display Name: versionnumber</para>
    /// </summary>
    [AttributeLogicalName("versionnumber")]
    [DisplayName("versionnumber")]
    public long? VersionNumber
    {
        get => GetAttributeValue<long?>("versionnumber");
        set => SetAttributeValue("versionnumber", value);
    }

    /// <summary>
    /// <para>Whether or not the SDK message can be called from a workflow.</para>
    /// <para>Display Name: WorkflowSdkStepEnabled</para>
    /// </summary>
    [AttributeLogicalName("workflowsdkstepenabled")]
    [DisplayName("WorkflowSdkStepEnabled")]
    public bool? WorkflowSdkStepEnabled
    {
        get => GetAttributeValue<bool?>("workflowsdkstepenabled");
        set => SetAttributeValue("workflowsdkstepenabled", value);
    }

    [AttributeLogicalName("modifiedonbehalfby")]
    [RelationshipSchemaName("lk_sdkmessagefilter_modifiedonbehalfby")]
    [RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_sdkmessagefilter_modifiedonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_sdkmessagefilter_modifiedonbehalfby", null);
        set => SetRelatedEntity("lk_sdkmessagefilter_modifiedonbehalfby", null, value);
    }

    [AttributeLogicalName("createdby")]
    [RelationshipSchemaName("createdby_sdkmessagefilter")]
    [RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser createdby_sdkmessagefilter
    {
        get => GetRelatedEntity<SystemUser>("createdby_sdkmessagefilter", null);
        set => SetRelatedEntity("createdby_sdkmessagefilter", null, value);
    }

    [AttributeLogicalName("modifiedby")]
    [RelationshipSchemaName("modifiedby_sdkmessagefilter")]
    [RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser modifiedby_sdkmessagefilter
    {
        get => GetRelatedEntity<SystemUser>("modifiedby_sdkmessagefilter", null);
        set => SetRelatedEntity("modifiedby_sdkmessagefilter", null, value);
    }

    [AttributeLogicalName("createdonbehalfby")]
    [RelationshipSchemaName("lk_sdkmessagefilter_createdonbehalfby")]
    [RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_sdkmessagefilter_createdonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_sdkmessagefilter_createdonbehalfby", null);
        set => SetRelatedEntity("lk_sdkmessagefilter_createdonbehalfby", null, value);
    }

    [AttributeLogicalName("sdkmessageid")]
    [RelationshipSchemaName("sdkmessageid_sdkmessagefilter")]
    [RelationshipMetadata("ManyToOne", "sdkmessageid", "sdkmessage", "sdkmessageid", "Referencing")]
    public SdkMessage sdkmessageid_sdkmessagefilter
    {
        get => GetRelatedEntity<SdkMessage>("sdkmessageid_sdkmessagefilter", null);
        set => SetRelatedEntity("sdkmessageid_sdkmessagefilter", null, value);
    }

    [RelationshipSchemaName("sdkmessagefilterid_sdkmessageprocessingstep")]
    [RelationshipMetadata("OneToMany", "sdkmessagefilterid", "sdkmessageprocessingstep", "sdkmessagefilterid", "Referenced")]
    public IEnumerable<SdkMessageProcessingStep> sdkmessagefilterid_sdkmessageprocessingstep
    {
        get => GetRelatedEntities<SdkMessageProcessingStep>("sdkmessagefilterid_sdkmessageprocessingstep", null);
        set => SetRelatedEntities("sdkmessagefilterid_sdkmessageprocessingstep", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the SdkMessageFilter entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<SdkMessageFilter, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the SdkMessageFilter with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of SdkMessageFilter to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved SdkMessageFilter</returns>
    public static SdkMessageFilter Retrieve(IOrganizationService service, Guid id, params Expression<Func<SdkMessageFilter, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }
}