using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Entity that defines a request parameter for a custom API</para>
/// <para>Display Name: Custom API Request Parameter</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("customapirequestparameter")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class CustomAPIRequestParameter : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "customapirequestparameter";
    public const int EntityTypeCode = 10037;

    public CustomAPIRequestParameter() : base(EntityLogicalName) { }
    public CustomAPIRequestParameter(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("name");

    [AttributeLogicalName("customapirequestparameterid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("customapirequestparameterid", value);
        }
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
    /// <para>Unique identifier for the custom API that owns this custom API request parameter</para>
    /// <para>Display Name: Custom API</para>
    /// </summary>
    [AttributeLogicalName("customapiid")]
    [DisplayName("Custom API")]
    public EntityReference? CustomAPIId
    {
        get => GetAttributeValue<EntityReference?>("customapiid");
        set => SetAttributeValue("customapiid", value);
    }

    /// <summary>
    /// <para>Display Name: Custom API Request Parameter</para>
    /// </summary>
    [AttributeLogicalName("customapirequestparameterid")]
    [DisplayName("Custom API Request Parameter")]
    public Guid CustomAPIRequestParameterId
    {
        get => GetAttributeValue<Guid>("customapirequestparameterid");
        set => SetId("customapirequestparameterid", value);
    }

    /// <summary>
    /// <para>Localized description for custom API request parameter instances</para>
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
    /// <para>Localized display name for custom API request parameter instances</para>
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
    /// <para>Indicates if the custom API request parameter is optional</para>
    /// <para>Display Name: Is Optional</para>
    /// </summary>
    [AttributeLogicalName("isoptional")]
    [DisplayName("Is Optional")]
    public bool? IsOptional
    {
        get => GetAttributeValue<bool?>("isoptional");
        set => SetAttributeValue("isoptional", value);
    }

    /// <summary>
    /// <para>The logical name of the entity bound to the custom API request parameter</para>
    /// <para>Display Name: Bound Entity Logical Name</para>
    /// </summary>
    [AttributeLogicalName("logicalentityname")]
    [DisplayName("Bound Entity Logical Name")]
    [MaxLength(100)]
    public string LogicalEntityName
    {
        get => GetAttributeValue<string>("logicalentityname");
        set => SetAttributeValue("logicalentityname", value);
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
    /// <para>The primary name of the custom API request parameter</para>
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
    /// <para>Status of the Custom API Request Parameter</para>
    /// <para>Display Name: Status</para>
    /// </summary>
    [AttributeLogicalName("statecode")]
    [DisplayName("Status")]
    public customapirequestparameter_statecode? statecode
    {
        get => this.GetOptionSetValue<customapirequestparameter_statecode>("statecode");
        set => this.SetOptionSetValue("statecode", value);
    }

    /// <summary>
    /// <para>Reason for the status of the Custom API Request Parameter</para>
    /// <para>Display Name: Status Reason</para>
    /// </summary>
    [AttributeLogicalName("statuscode")]
    [DisplayName("Status Reason")]
    public customapirequestparameter_statuscode? statuscode
    {
        get => this.GetOptionSetValue<customapirequestparameter_statuscode>("statuscode");
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
    /// <para>The data type of the custom API request parameter</para>
    /// <para>Display Name: Type</para>
    /// </summary>
    [AttributeLogicalName("type")]
    [DisplayName("Type")]
    public customapifieldtype? Type
    {
        get => this.GetOptionSetValue<customapifieldtype>("type");
        set => this.SetOptionSetValue("type", value);
    }

    /// <summary>
    /// <para>Unique name for the custom API request parameter</para>
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

    [AttributeLogicalName("createdby")]
    [RelationshipSchemaName("lk_customapirequestparameter_createdby")]
    [RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapirequestparameter_createdby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapirequestparameter_createdby", null);
        set => SetRelatedEntity("lk_customapirequestparameter_createdby", null, value);
    }

    [AttributeLogicalName("createdonbehalfby")]
    [RelationshipSchemaName("lk_customapirequestparameter_createdonbehalfby")]
    [RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapirequestparameter_createdonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapirequestparameter_createdonbehalfby", null);
        set => SetRelatedEntity("lk_customapirequestparameter_createdonbehalfby", null, value);
    }

    [AttributeLogicalName("modifiedby")]
    [RelationshipSchemaName("lk_customapirequestparameter_modifiedby")]
    [RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapirequestparameter_modifiedby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapirequestparameter_modifiedby", null);
        set => SetRelatedEntity("lk_customapirequestparameter_modifiedby", null, value);
    }

    [AttributeLogicalName("modifiedonbehalfby")]
    [RelationshipSchemaName("lk_customapirequestparameter_modifiedonbehalfby")]
    [RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_customapirequestparameter_modifiedonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_customapirequestparameter_modifiedonbehalfby", null);
        set => SetRelatedEntity("lk_customapirequestparameter_modifiedonbehalfby", null, value);
    }

    [AttributeLogicalName("customapiid")]
    [RelationshipSchemaName("customapi_customapirequestparameter")]
    [RelationshipMetadata("ManyToOne", "customapiid", "customapi", "customapiid", "Referencing")]
    public CustomAPI customapi_customapirequestparameter
    {
        get => GetRelatedEntity<CustomAPI>("customapi_customapirequestparameter", null);
        set => SetRelatedEntity("customapi_customapirequestparameter", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the CustomAPIRequestParameter entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<CustomAPIRequestParameter, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the CustomAPIRequestParameter with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of CustomAPIRequestParameter to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved CustomAPIRequestParameter</returns>
    public static CustomAPIRequestParameter Retrieve(IOrganizationService service, Guid id, params Expression<Func<CustomAPIRequestParameter, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }

    /// <summary>
    /// Retrieves the CustomAPIRequestParameter using the Custom API Request Parameter Export Key alternate key.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="ComponentState">ComponentState key value</param>
    /// <param name="CustomAPIId">CustomAPIId key value</param>
    /// <param name="OverwriteTime">OverwriteTime key value</param>
    /// <param name="UniqueName">UniqueName key value</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved CustomAPIRequestParameter</returns>
    public static CustomAPIRequestParameter Retrieve_CustomAPIRequestParameterExportKey(IOrganizationService service, componentstate ComponentState, Guid CustomAPIId, DateTime OverwriteTime, string UniqueName, params Expression<Func<CustomAPIRequestParameter, object>>[] columns)
    {
        var keyedEntityReference = new EntityReference(EntityLogicalName, new KeyAttributeCollection
        {
            ["componentstate"] = ComponentState,
            ["customapiid"] = CustomAPIId,
            ["overwritetime"] = OverwriteTime,
            ["uniquename"] = UniqueName,
        });

        return service.Retrieve(keyedEntityReference, columns);
    }
}