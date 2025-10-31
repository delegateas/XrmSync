using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Entity that defines a custom API</para>
/// <para>Display Name: Custom API</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[EntityLogicalName("customapi")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class CustomAPI : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "customapi";
    public const int EntityTypeCode = 10027;

    public CustomAPI() : base(EntityLogicalName) { }
    public CustomAPI(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("name");

    [AttributeLogicalName("customapiid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("customapiid", value);
        }
    }

    /// <summary>
    /// <para>The type of custom processing step allowed</para>
    /// <para>Display Name: Allowed Custom Processing Step Type</para>
    /// </summary>
    [AttributeLogicalName("allowedcustomprocessingsteptype")]
    [DisplayName("Allowed Custom Processing Step Type")]
    public customapi_allowedcustomprocessingsteptype? AllowedCustomProcessingStepType
    {
        get => this.GetOptionSetValue<customapi_allowedcustomprocessingsteptype>("allowedcustomprocessingsteptype");
        set => this.SetOptionSetValue("allowedcustomprocessingsteptype", value);
    }

    /// <summary>
    /// <para>The binding type of the custom API</para>
    /// <para>Display Name: Binding Type</para>
    /// </summary>
    [AttributeLogicalName("bindingtype")]
    [DisplayName("Binding Type")]
    public customapi_bindingtype? BindingType
    {
        get => this.GetOptionSetValue<customapi_bindingtype>("bindingtype");
        set => this.SetOptionSetValue("bindingtype", value);
    }

    /// <summary>
    /// <para>The logical name of the entity bound to the custom API</para>
    /// <para>Display Name: Bound Entity Logical Name</para>
    /// </summary>
    [AttributeLogicalName("boundentitylogicalname")]
    [DisplayName("Bound Entity Logical Name")]
    [MaxLength(100)]
    public string BoundEntityLogicalName
    {
        get => GetAttributeValue<string>("boundentitylogicalname");
        set => SetAttributeValue("boundentitylogicalname", value);
    }

    /// <summary>
    /// <para>For internal use only.</para>
    /// <para>Display Name: Row id unique</para>
    /// </summary>
    [AttributeLogicalName("componentidunique")]
    [DisplayName("Row id unique")]
    public Guid? ComponentIdUnique
    {
        get => GetAttributeValue<Guid?>("componentidunique");
        set => SetAttributeValue("componentidunique", value);
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
    /// <para>Unique identifier of the user who created the record.</para>
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
    /// <para>Date and time when the record was created.</para>
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
    /// <para>Unique identifier of the delegate user who created the record.</para>
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
    /// <para>Display Name: Custom API</para>
    /// </summary>
    [AttributeLogicalName("customapiid")]
    [DisplayName("Custom API")]
    public Guid CustomAPIId
    {
        get => GetAttributeValue<Guid>("customapiid");
        set => SetId("customapiid", value);
    }

    /// <summary>
    /// <para>Localized description for custom API instances</para>
    /// <para>Display Name: Description</para>
    /// </summary>
    [AttributeLogicalName("description")]
    [DisplayName("Description")]
    [MaxLength(300)]
    public string Description
    {
        get => GetAttributeValue<string>("description");
        set => SetAttributeValue("description", value);
    }

    /// <summary>
    /// <para>Localized display name for custom API instances</para>
    /// <para>Display Name: Display Name</para>
    /// </summary>
    [AttributeLogicalName("displayname")]
    [DisplayName("Display Name")]
    [MaxLength(100)]
    public string DisplayName
    {
        get => GetAttributeValue<string>("displayname");
        set => SetAttributeValue("displayname", value);
    }

    /// <summary>
    /// <para>Name of the privilege that allows execution of the custom API</para>
    /// <para>Display Name: Execute Privilege Name</para>
    /// </summary>
    [AttributeLogicalName("executeprivilegename")]
    [DisplayName("Execute Privilege Name")]
    [MaxLength(100)]
    public string ExecutePrivilegeName
    {
        get => GetAttributeValue<string>("executeprivilegename");
        set => SetAttributeValue("executeprivilegename", value);
    }

    /// <summary>
    /// <para>Unique identifier for fxexpression associated with Custom API.</para>
    /// <para>Display Name: FxExpression</para>
    /// </summary>
    [AttributeLogicalName("fxexpressionid")]
    [DisplayName("FxExpression")]
    public EntityReference? FxExpressionId
    {
        get => GetAttributeValue<EntityReference?>("fxexpressionid");
        set => SetAttributeValue("fxexpressionid", value);
    }

    /// <summary>
    /// <para>Sequence number of the import that created this record.</para>
    /// <para>Display Name: Import Sequence Number</para>
    /// </summary>
    [AttributeLogicalName("importsequencenumber")]
    [DisplayName("Import Sequence Number")]
    [Range(-2147483648, 2147483647)]
    public int? ImportSequenceNumber
    {
        get => GetAttributeValue<int?>("importsequencenumber");
        set => SetAttributeValue("importsequencenumber", value);
    }

    /// <summary>
    /// <para>For internal use only.</para>
    /// <para>Display Name: Is Customizable</para>
    /// </summary>
    [AttributeLogicalName("iscustomizable")]
    [DisplayName("Is Customizable")]
    public BooleanManagedProperty IsCustomizable
    {
        get => GetAttributeValue<BooleanManagedProperty>("iscustomizable");
        set => SetAttributeValue("iscustomizable", value);
    }

    /// <summary>
    /// <para>Indicates if the custom API is a function (GET is supported) or not (POST is supported)</para>
    /// <para>Display Name: Is Function</para>
    /// </summary>
    [AttributeLogicalName("isfunction")]
    [DisplayName("Is Function")]
    public bool? IsFunction
    {
        get => GetAttributeValue<bool?>("isfunction");
        set => SetAttributeValue("isfunction", value);
    }

    /// <summary>
    /// <para>Indicates whether the solution component is part of a managed solution.</para>
    /// <para>Display Name: Is Managed</para>
    /// </summary>
    [AttributeLogicalName("ismanaged")]
    [DisplayName("Is Managed")]
    public bool? IsManaged
    {
        get => GetAttributeValue<bool?>("ismanaged");
        set => SetAttributeValue("ismanaged", value);
    }

    /// <summary>
    /// <para>Indicates if the custom API is private (hidden from metadata and documentation)</para>
    /// <para>Display Name: Is Private</para>
    /// </summary>
    [AttributeLogicalName("isprivate")]
    [DisplayName("Is Private")]
    public bool? IsPrivate
    {
        get => GetAttributeValue<bool?>("isprivate");
        set => SetAttributeValue("isprivate", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user who modified the record.</para>
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
    /// <para>Date and time when the record was modified.</para>
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
    /// <para>Unique identifier of the delegate user who modified the record.</para>
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
    /// <para>The primary name of the custom API</para>
    /// <para>Display Name: Name</para>
    /// </summary>
    [AttributeLogicalName("name")]
    [DisplayName("Name")]
    [MaxLength(100)]
    public string Name
    {
        get => GetAttributeValue<string>("name");
        set => SetAttributeValue("name", value);
    }

    /// <summary>
    /// <para>Date and time that the record was migrated.</para>
    /// <para>Display Name: Record Created On</para>
    /// </summary>
    [AttributeLogicalName("overriddencreatedon")]
    [DisplayName("Record Created On")]
    public DateTime? OverriddenCreatedOn
    {
        get => GetAttributeValue<DateTime?>("overriddencreatedon");
        set => SetAttributeValue("overriddencreatedon", value);
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
    /// <para>Owner Id</para>
    /// <para>Display Name: Owner</para>
    /// </summary>
    [AttributeLogicalName("ownerid")]
    [DisplayName("Owner")]
    public EntityReference? OwnerId
    {
        get => GetAttributeValue<EntityReference?>("ownerid");
        set => SetAttributeValue("ownerid", value);
    }

    /// <summary>
    /// <para>Unique identifier for the business unit that owns the record</para>
    /// <para>Display Name: Owning Business Unit</para>
    /// </summary>
    [AttributeLogicalName("owningbusinessunit")]
    [DisplayName("Owning Business Unit")]
    public EntityReference? OwningBusinessUnit
    {
        get => GetAttributeValue<EntityReference?>("owningbusinessunit");
        set => SetAttributeValue("owningbusinessunit", value);
    }

    /// <summary>
    /// <para>Unique identifier for the team that owns the record.</para>
    /// <para>Display Name: Owning Team</para>
    /// </summary>
    [AttributeLogicalName("owningteam")]
    [DisplayName("Owning Team")]
    public EntityReference? OwningTeam
    {
        get => GetAttributeValue<EntityReference?>("owningteam");
        set => SetAttributeValue("owningteam", value);
    }

    /// <summary>
    /// <para>Unique identifier for the user that owns the record.</para>
    /// <para>Display Name: Owning User</para>
    /// </summary>
    [AttributeLogicalName("owninguser")]
    [DisplayName("Owning User")]
    public EntityReference? OwningUser
    {
        get => GetAttributeValue<EntityReference?>("owninguser");
        set => SetAttributeValue("owninguser", value);
    }

    /// <summary>
    /// <para>Display Name: Plugin Type</para>
    /// </summary>
    [AttributeLogicalName("plugintypeid")]
    [DisplayName("Plugin Type")]
    public EntityReference? PluginTypeId
    {
        get => GetAttributeValue<EntityReference?>("plugintypeid");
        set => SetAttributeValue("plugintypeid", value);
    }

    /// <summary>
    /// <para>Unique identifier for powerfxrule associated with Custom API.</para>
    /// <para>Display Name: PowerFx Rule</para>
    /// </summary>
    [AttributeLogicalName("powerfxruleid")]
    [DisplayName("PowerFx Rule")]
    public EntityReference? PowerfxRuleId
    {
        get => GetAttributeValue<EntityReference?>("powerfxruleid");
        set => SetAttributeValue("powerfxruleid", value);
    }

    /// <summary>
    /// <para>Display Name: Sdk Message</para>
    /// </summary>
    [AttributeLogicalName("sdkmessageid")]
    [DisplayName("Sdk Message")]
    public EntityReference? SdkMessageId
    {
        get => GetAttributeValue<EntityReference?>("sdkmessageid");
        set => SetAttributeValue("sdkmessageid", value);
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
    /// <para>Status of the Custom API</para>
    /// <para>Display Name: Status</para>
    /// </summary>
    [AttributeLogicalName("statecode")]
    [DisplayName("Status")]
    public customapi_statecode? statecode
    {
        get => this.GetOptionSetValue<customapi_statecode>("statecode");
        set => this.SetOptionSetValue("statecode", value);
    }

    /// <summary>
    /// <para>Reason for the status of the Custom API</para>
    /// <para>Display Name: Status Reason</para>
    /// </summary>
    [AttributeLogicalName("statuscode")]
    [DisplayName("Status Reason")]
    public customapi_statuscode? statuscode
    {
        get => this.GetOptionSetValue<customapi_statuscode>("statuscode");
        set => this.SetOptionSetValue("statuscode", value);
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
    /// <para>For internal use only.</para>
    /// <para>Display Name: Time Zone Rule Version Number</para>
    /// </summary>
    [AttributeLogicalName("timezoneruleversionnumber")]
    [DisplayName("Time Zone Rule Version Number")]
    [Range(-1, 2147483647)]
    public int? TimeZoneRuleVersionNumber
    {
        get => GetAttributeValue<int?>("timezoneruleversionnumber");
        set => SetAttributeValue("timezoneruleversionnumber", value);
    }

    /// <summary>
    /// <para>Unique name for the custom API</para>
    /// <para>Display Name: Unique Name</para>
    /// </summary>
    [AttributeLogicalName("uniquename")]
    [DisplayName("Unique Name")]
    [MaxLength(128)]
    public string UniqueName
    {
        get => GetAttributeValue<string>("uniquename");
        set => SetAttributeValue("uniquename", value);
    }

    /// <summary>
    /// <para>Time zone code that was in use when the record was created.</para>
    /// <para>Display Name: UTC Conversion Time Zone Code</para>
    /// </summary>
    [AttributeLogicalName("utcconversiontimezonecode")]
    [DisplayName("UTC Conversion Time Zone Code")]
    [Range(-1, 2147483647)]
    public int? UTCConversionTimeZoneCode
    {
        get => GetAttributeValue<int?>("utcconversiontimezonecode");
        set => SetAttributeValue("utcconversiontimezonecode", value);
    }

    /// <summary>
    /// <para>Version Number</para>
    /// <para>Display Name: Version Number</para>
    /// </summary>
    [AttributeLogicalName("versionnumber")]
    [DisplayName("Version Number")]
    public long? VersionNumber
    {
        get => GetAttributeValue<long?>("versionnumber");
        set => SetAttributeValue("versionnumber", value);
    }

    /// <summary>
    /// <para>Indicates if the custom API is enabled as a workflow action</para>
    /// <para>Display Name: Enabled for Workflow</para>
    /// </summary>
    [AttributeLogicalName("workflowsdkstepenabled")]
    [DisplayName("Enabled for Workflow")]
    public bool? WorkflowSdkStepEnabled
    {
        get => GetAttributeValue<bool?>("workflowsdkstepenabled");
        set => SetAttributeValue("workflowsdkstepenabled", value);
    }

    [AttributeLogicalName("createdby")]
    [RelationshipSchemaName("lk_customapi_createdby")]
    [RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapi_createdby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapi_createdby", null);
        set => SetRelatedEntity("lk_customapi_createdby", null, value);
    }

    [AttributeLogicalName("createdonbehalfby")]
    [RelationshipSchemaName("lk_customapi_createdonbehalfby")]
    [RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapi_createdonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapi_createdonbehalfby", null);
        set => SetRelatedEntity("lk_customapi_createdonbehalfby", null, value);
    }

    [AttributeLogicalName("modifiedby")]
    [RelationshipSchemaName("lk_customapi_modifiedby")]
    [RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapi_modifiedby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapi_modifiedby", null);
        set => SetRelatedEntity("lk_customapi_modifiedby", null, value);
    }

    [AttributeLogicalName("modifiedonbehalfby")]
    [RelationshipSchemaName("lk_customapi_modifiedonbehalfby")]
    [RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapi_modifiedonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapi_modifiedonbehalfby", null);
        set => SetRelatedEntity("lk_customapi_modifiedonbehalfby", null, value);
    }

    [AttributeLogicalName("owninguser")]
    [RelationshipSchemaName("user_customapi")]
    [RelationshipMetadata("ManyToOne", "owninguser", "systemuser", "systemuserid", "Referencing")]
    public SystemUser user_customapi
    {
        get => GetRelatedEntity<SystemUser>("user_customapi", null);
        set => SetRelatedEntity("user_customapi", null, value);
    }

    [AttributeLogicalName("sdkmessageid")]
    [RelationshipSchemaName("sdkmessage_customapi")]
    [RelationshipMetadata("ManyToOne", "sdkmessageid", "sdkmessage", "sdkmessageid", "Referencing")]
    public SdkMessage sdkmessage_customapi
    {
        get => GetRelatedEntity<SdkMessage>("sdkmessage_customapi", null);
        set => SetRelatedEntity("sdkmessage_customapi", null, value);
    }

    [AttributeLogicalName("plugintypeid")]
    [RelationshipSchemaName("plugintype_customapi")]
    [RelationshipMetadata("ManyToOne", "plugintypeid", "plugintype", "plugintypeid", "Referencing")]
    public PluginType plugintype_customapi
    {
        get => GetRelatedEntity<PluginType>("plugintype_customapi", null);
        set => SetRelatedEntity("plugintype_customapi", null, value);
    }

    [RelationshipSchemaName("customapi_customapirequestparameter")]
    [RelationshipMetadata("OneToMany", "customapiid", "customapirequestparameter", "customapiid", "Referenced")]
    public IEnumerable<CustomAPIRequestParameter> customapi_customapirequestparameter
    {
        get => GetRelatedEntities<CustomAPIRequestParameter>("customapi_customapirequestparameter", null);
        set => SetRelatedEntities("customapi_customapirequestparameter", null, value);
    }

    [RelationshipSchemaName("customapi_customapiresponseproperty")]
    [RelationshipMetadata("OneToMany", "customapiid", "customapiresponseproperty", "customapiid", "Referenced")]
    public IEnumerable<CustomAPIResponseProperty> customapi_customapiresponseproperty
    {
        get => GetRelatedEntities<CustomAPIResponseProperty>("customapi_customapiresponseproperty", null);
        set => SetRelatedEntities("customapi_customapiresponseproperty", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the CustomAPI entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<CustomAPI, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the CustomAPI with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of CustomAPI to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved CustomAPI</returns>
    public static CustomAPI Retrieve(IOrganizationService service, Guid id, params Expression<Func<CustomAPI, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }
}