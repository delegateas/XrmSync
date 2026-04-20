using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Contains data to represent an Azure Active Directory Application used to connect to secure web-hosted resources.</para>
/// <para>Display Name: Managed Identity</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[EntityLogicalName("managedidentity")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class ManagedIdentity : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "managedidentity";
    public const int EntityTypeCode = 10032;

    public ManagedIdentity() : base(EntityLogicalName) { }
    public ManagedIdentity(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("name");

    [AttributeLogicalName("managedidentityid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("managedidentityid", value);
        }
    }

    /// <summary>
    /// <para>Application Id</para>
    /// <para>Display Name: ApplicationId</para>
    /// </summary>
    [AttributeLogicalName("applicationid")]
    [DisplayName("ApplicationId")]
    public Guid? ApplicationId
    {
        get => GetAttributeValue<Guid?>("applicationid");
        set => SetAttributeValue("applicationid", value);
    }

    /// <summary>
    /// <para>Contains a secret for the Azure Active Directory application. Once set, it cannot be read except by Dataverse.</para>
    /// <para>Display Name: Client Secret</para>
    /// </summary>
    [AttributeLogicalName("clientsecret")]
    [DisplayName("Client Secret")]
    [MaxLength(100)]
    public string? ClientSecret
    {
        get => GetAttributeValue<string?>("clientsecret");
        set => SetAttributeValue("clientsecret", value);
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
    /// <para>Where the Managed Identity will get the credentials to use.</para>
    /// <para>Display Name: Credential Source</para>
    /// </summary>
    [AttributeLogicalName("credentialsource")]
    [DisplayName("Credential Source")]
    public credentialsource? CredentialSource
    {
        get => this.GetOptionSetValue<credentialsource>("credentialsource");
        set => this.SetOptionSetValue("credentialsource", value);
    }

    /// <summary>
    /// <para>Determines Identity type for Managed Identity</para>
    /// <para>Display Name: Identity Type</para>
    /// </summary>
    [AttributeLogicalName("identitytype")]
    [DisplayName("Identity Type")]
    public identitytype? IdentityType
    {
        get => this.GetOptionSetValue<identitytype>("identitytype");
        set => this.SetOptionSetValue("identitytype", value);
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
    /// <para>Unique identifier for keyvaultreference which contains the secret.</para>
    /// <para>Display Name: KeyVaultReferenceId</para>
    /// </summary>
    [AttributeLogicalName("keyvaultreferenceid")]
    [DisplayName("KeyVaultReferenceId")]
    public EntityReference? KeyVaultReferenceId
    {
        get => GetAttributeValue<EntityReference?>("keyvaultreferenceid");
        set => SetAttributeValue("keyvaultreferenceid", value);
    }

    /// <summary>
    /// <para>Display Name: ManagedIdentity Id</para>
    /// </summary>
    [AttributeLogicalName("managedidentityid")]
    [DisplayName("ManagedIdentity Id")]
    public Guid? ManagedIdentityId
    {
        get => GetAttributeValue<Guid?>("managedidentityid");
        set => SetId("managedidentityid", value);
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
    /// <para>The name assigned to this Managed Identity.</para>
    /// <para>Display Name: Name</para>
    /// </summary>
    [AttributeLogicalName("name")]
    [DisplayName("Name")]
    [MaxLength(100)]
    public string? Name
    {
        get => GetAttributeValue<string?>("name");
        set => SetAttributeValue("name", value);
    }

    /// <summary>
    /// <para>ObjectId</para>
    /// <para>Display Name: ObjectId</para>
    /// </summary>
    [AttributeLogicalName("objectid")]
    [DisplayName("ObjectId")]
    public Guid? ObjectId
    {
        get => GetAttributeValue<Guid?>("objectid");
        set => SetAttributeValue("objectid", value);
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
    /// <para>Status of the Managed Identity</para>
    /// <para>Display Name: Status</para>
    /// </summary>
    [AttributeLogicalName("statecode")]
    [DisplayName("Status")]
    public managedidentity_statecode? statecode
    {
        get => this.GetOptionSetValue<managedidentity_statecode>("statecode");
        set => this.SetOptionSetValue("statecode", value);
    }

    /// <summary>
    /// <para>Reason for the status of the Managed Identity</para>
    /// <para>Display Name: Status Reason</para>
    /// </summary>
    [AttributeLogicalName("statuscode")]
    [DisplayName("Status Reason")]
    public managedidentity_statuscode? statuscode
    {
        get => this.GetOptionSetValue<managedidentity_statuscode>("statuscode");
        set => this.SetOptionSetValue("statuscode", value);
    }

    /// <summary>
    /// <para>Where the Scope of the SubjectName for Managed Identity will be determined.</para>
    /// <para>Display Name: Subject Scope</para>
    /// </summary>
    [AttributeLogicalName("subjectscope")]
    [DisplayName("Subject Scope")]
    public subjectscope? SubjectScope
    {
        get => this.GetOptionSetValue<subjectscope>("subjectscope");
        set => this.SetOptionSetValue("subjectscope", value);
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
    /// <para>The Id of the Azure Active Directory Tenant that the Application is part of.</para>
    /// <para>Display Name: TenantId</para>
    /// </summary>
    [AttributeLogicalName("tenantid")]
    [DisplayName("TenantId")]
    public Guid? TenantId
    {
        get => GetAttributeValue<Guid?>("tenantid");
        set => SetAttributeValue("tenantid", value);
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
    /// <para>Version indicating the format of the FIC subject.</para>
    /// <para>Display Name: version</para>
    /// </summary>
    [AttributeLogicalName("version")]
    [DisplayName("version")]
    [Range(0, 2147483647)]
    public int? Version
    {
        get => GetAttributeValue<int?>("version");
        set => SetAttributeValue("version", value);
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
    [RelationshipSchemaName("lk_managedidentity_createdby")]
    [RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_managedidentity_createdby
    {
        get => GetRelatedEntity<SystemUser>("lk_managedidentity_createdby", null);
        set => SetRelatedEntity("lk_managedidentity_createdby", null, value);
    }

    [AttributeLogicalName("createdonbehalfby")]
    [RelationshipSchemaName("lk_managedidentity_createdonbehalfby")]
    [RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_managedidentity_createdonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_managedidentity_createdonbehalfby", null);
        set => SetRelatedEntity("lk_managedidentity_createdonbehalfby", null, value);
    }

    [AttributeLogicalName("modifiedby")]
    [RelationshipSchemaName("lk_managedidentity_modifiedby")]
    [RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_managedidentity_modifiedby
    {
        get => GetRelatedEntity<SystemUser>("lk_managedidentity_modifiedby", null);
        set => SetRelatedEntity("lk_managedidentity_modifiedby", null, value);
    }

    [AttributeLogicalName("modifiedonbehalfby")]
    [RelationshipSchemaName("lk_managedidentity_modifiedonbehalfby")]
    [RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_managedidentity_modifiedonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_managedidentity_modifiedonbehalfby", null);
        set => SetRelatedEntity("lk_managedidentity_modifiedonbehalfby", null, value);
    }

    [RelationshipSchemaName("managedidentity_PluginAssembly")]
    [RelationshipMetadata("OneToMany", "managedidentityid", "pluginassembly", "managedidentityid", "Referenced")]
    public IEnumerable<PluginAssembly> managedidentity_PluginAssembly
    {
        get => GetRelatedEntities<PluginAssembly>("managedidentity_PluginAssembly", null);
        set => SetRelatedEntities("managedidentity_PluginAssembly", null, value);
    }

    [AttributeLogicalName("owninguser")]
    [RelationshipSchemaName("user_managedidentity")]
    [RelationshipMetadata("ManyToOne", "owninguser", "systemuser", "systemuserid", "Referencing")]
    public SystemUser user_managedidentity
    {
        get => GetRelatedEntity<SystemUser>("user_managedidentity", null);
        set => SetRelatedEntity("user_managedidentity", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the ManagedIdentity entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<ManagedIdentity, object?>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the ManagedIdentity with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of ManagedIdentity to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved ManagedIdentity</returns>
    public static ManagedIdentity Retrieve(IOrganizationService service, Guid id, params Expression<Func<ManagedIdentity, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }
}
