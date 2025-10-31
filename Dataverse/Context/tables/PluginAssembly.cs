using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Assembly that contains one or more plug-in types.</para>
/// <para>Display Name: Plug-in Assembly</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[EntityLogicalName("pluginassembly")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class PluginAssembly : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "pluginassembly";
    public const int EntityTypeCode = 4605;

    public PluginAssembly() : base(EntityLogicalName) { }
    public PluginAssembly(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("name");

    [AttributeLogicalName("pluginassemblyid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("pluginassemblyid", value);
        }
    }

    /// <summary>
    /// <para>Specifies mode of authentication with web sources like WebApp</para>
    /// <para>Display Name: Specifies mode of authentication with web sources</para>
    /// </summary>
    [AttributeLogicalName("authtype")]
    [DisplayName("Specifies mode of authentication with web sources")]
    public pluginassembly_authtype? AuthType
    {
        get => this.GetOptionSetValue<pluginassembly_authtype>("authtype");
        set => this.SetOptionSetValue("authtype", value);
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
    /// <para>Bytes of the assembly, in Base64 format.</para>
    /// <para>Display Name: content</para>
    /// </summary>
    [AttributeLogicalName("content")]
    [DisplayName("content")]
    [MaxLength(1073741823)]
    public string Content
    {
        get => GetAttributeValue<string>("content");
        set => SetAttributeValue("content", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user who created the plug-in assembly.</para>
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
    /// <para>Date and time when the plug-in assembly was created.</para>
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
    /// <para>Unique identifier of the delegate user who created the pluginassembly.</para>
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
    /// <para>Culture code for the plug-in assembly.</para>
    /// <para>Display Name: Culture</para>
    /// </summary>
    [AttributeLogicalName("culture")]
    [DisplayName("Culture")]
    [MaxLength(32)]
    public string Culture
    {
        get => GetAttributeValue<string>("culture");
        set => SetAttributeValue("culture", value);
    }

    /// <summary>
    /// <para>Customization Level.</para>
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
    /// <para>Description of the plug-in assembly.</para>
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
    /// <para>Information about how the plugin assembly is to be isolated at execution time; None / Sandboxed.</para>
    /// <para>Display Name: Isolation Mode</para>
    /// </summary>
    [AttributeLogicalName("isolationmode")]
    [DisplayName("Isolation Mode")]
    public pluginassembly_isolationmode? IsolationMode
    {
        get => this.GetOptionSetValue<pluginassembly_isolationmode>("isolationmode");
        set => this.SetOptionSetValue("isolationmode", value);
    }

    /// <summary>
    /// <para>Display Name: ispasswordset</para>
    /// </summary>
    [AttributeLogicalName("ispasswordset")]
    [DisplayName("ispasswordset")]
    public bool? IsPasswordSet
    {
        get => GetAttributeValue<bool?>("ispasswordset");
        set => SetAttributeValue("ispasswordset", value);
    }

    /// <summary>
    /// <para>Major of the assembly version.</para>
    /// <para>Display Name: major</para>
    /// </summary>
    [AttributeLogicalName("major")]
    [DisplayName("major")]
    [Range(0, 65534)]
    public int? Major
    {
        get => GetAttributeValue<int?>("major");
        set => SetAttributeValue("major", value);
    }

    /// <summary>
    /// <para>Unique identifier for managedidentity associated with pluginassembly.</para>
    /// <para>Display Name: ManagedIdentityId</para>
    /// </summary>
    [AttributeLogicalName("managedidentityid")]
    [DisplayName("ManagedIdentityId")]
    public EntityReference? ManagedIdentityId
    {
        get => GetAttributeValue<EntityReference?>("managedidentityid");
        set => SetAttributeValue("managedidentityid", value);
    }

    /// <summary>
    /// <para>Minor of the assembly version.</para>
    /// <para>Display Name: minor</para>
    /// </summary>
    [AttributeLogicalName("minor")]
    [DisplayName("minor")]
    [Range(0, 65534)]
    public int? Minor
    {
        get => GetAttributeValue<int?>("minor");
        set => SetAttributeValue("minor", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user who last modified the plug-in assembly.</para>
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
    /// <para>Date and time when the plug-in assembly was last modified.</para>
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
    /// <para>Unique identifier of the delegate user who last modified the pluginassembly.</para>
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
    /// <para>Name of the plug-in assembly.</para>
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
    /// <para>Unique identifier of the organization with which the plug-in assembly is associated.</para>
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
    /// <para>Unique identifier for Plugin Package associated with Plug-in Assembly.</para>
    /// <para>Display Name: Package</para>
    /// </summary>
    [AttributeLogicalName("packageid")]
    [DisplayName("Package")]
    public EntityReference? PackageId
    {
        get => GetAttributeValue<EntityReference?>("packageid");
        set => SetAttributeValue("packageid", value);
    }

    /// <summary>
    /// <para>User Password</para>
    /// <para>Display Name: User Password</para>
    /// </summary>
    [AttributeLogicalName("password")]
    [DisplayName("User Password")]
    [MaxLength(256)]
    public string Password
    {
        get => GetAttributeValue<string>("password");
        set => SetAttributeValue("password", value);
    }

    /// <summary>
    /// <para>File name of the plug-in assembly. Used when the source type is set to 1.</para>
    /// <para>Display Name: Path</para>
    /// </summary>
    [AttributeLogicalName("path")]
    [DisplayName("Path")]
    [MaxLength(256)]
    public string Path
    {
        get => GetAttributeValue<string>("path");
        set => SetAttributeValue("path", value);
    }

    /// <summary>
    /// <para>Display Name: pluginassemblyid</para>
    /// </summary>
    [AttributeLogicalName("pluginassemblyid")]
    [DisplayName("pluginassemblyid")]
    public Guid PluginAssemblyId
    {
        get => GetAttributeValue<Guid>("pluginassemblyid");
        set => SetId("pluginassemblyid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the plug-in assembly.</para>
    /// <para>Display Name: pluginassemblyidunique</para>
    /// </summary>
    [AttributeLogicalName("pluginassemblyidunique")]
    [DisplayName("pluginassemblyidunique")]
    public Guid? PluginAssemblyIdUnique
    {
        get => GetAttributeValue<Guid?>("pluginassemblyidunique");
        set => SetAttributeValue("pluginassemblyidunique", value);
    }

    /// <summary>
    /// <para>Public key token of the assembly. This value can be obtained from the assembly by using reflection.</para>
    /// <para>Display Name: Public Key Token</para>
    /// </summary>
    [AttributeLogicalName("publickeytoken")]
    [DisplayName("Public Key Token")]
    [MaxLength(32)]
    public string PublicKeyToken
    {
        get => GetAttributeValue<string>("publickeytoken");
        set => SetAttributeValue("publickeytoken", value);
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
    /// <para>Hash of the source of the assembly.</para>
    /// <para>Display Name: sourcehash</para>
    /// </summary>
    [AttributeLogicalName("sourcehash")]
    [DisplayName("sourcehash")]
    [MaxLength(256)]
    public string SourceHash
    {
        get => GetAttributeValue<string>("sourcehash");
        set => SetAttributeValue("sourcehash", value);
    }

    /// <summary>
    /// <para>Location of the assembly, for example 0=database, 1=on-disk.</para>
    /// <para>Display Name: Source Type</para>
    /// </summary>
    [AttributeLogicalName("sourcetype")]
    [DisplayName("Source Type")]
    public pluginassembly_sourcetype? SourceType
    {
        get => this.GetOptionSetValue<pluginassembly_sourcetype>("sourcetype");
        set => this.SetOptionSetValue("sourcetype", value);
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
    /// <para>Web Url</para>
    /// <para>Display Name: Web Url</para>
    /// </summary>
    [AttributeLogicalName("url")]
    [DisplayName("Web Url")]
    [MaxLength(2000)]
    public string Url
    {
        get => GetAttributeValue<string>("url");
        set => SetAttributeValue("url", value);
    }

    /// <summary>
    /// <para>User Name</para>
    /// <para>Display Name: User Name</para>
    /// </summary>
    [AttributeLogicalName("username")]
    [DisplayName("User Name")]
    [MaxLength(256)]
    public string UserName
    {
        get => GetAttributeValue<string>("username");
        set => SetAttributeValue("username", value);
    }

    /// <summary>
    /// <para>Version number of the assembly. The value can be obtained from the assembly through reflection.</para>
    /// <para>Display Name: Version</para>
    /// </summary>
    [AttributeLogicalName("version")]
    [DisplayName("Version")]
    [MaxLength(48)]
    public string Version
    {
        get => GetAttributeValue<string>("version");
        set => SetAttributeValue("version", value);
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

    [AttributeLogicalName("createdonbehalfby")]
    [RelationshipSchemaName("lk_pluginassembly_createdonbehalfby")]
    [RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_pluginassembly_createdonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_pluginassembly_createdonbehalfby", null);
        set => SetRelatedEntity("lk_pluginassembly_createdonbehalfby", null, value);
    }

    [AttributeLogicalName("modifiedonbehalfby")]
    [RelationshipSchemaName("lk_pluginassembly_modifiedonbehalfby")]
    [RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_pluginassembly_modifiedonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_pluginassembly_modifiedonbehalfby", null);
        set => SetRelatedEntity("lk_pluginassembly_modifiedonbehalfby", null, value);
    }

    [AttributeLogicalName("modifiedby")]
    [RelationshipSchemaName("modifiedby_pluginassembly")]
    [RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser modifiedby_pluginassembly
    {
        get => GetRelatedEntity<SystemUser>("modifiedby_pluginassembly", null);
        set => SetRelatedEntity("modifiedby_pluginassembly", null, value);
    }

    [AttributeLogicalName("createdby")]
    [RelationshipSchemaName("createdby_pluginassembly")]
    [RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser createdby_pluginassembly
    {
        get => GetRelatedEntity<SystemUser>("createdby_pluginassembly", null);
        set => SetRelatedEntity("createdby_pluginassembly", null, value);
    }

    [RelationshipSchemaName("pluginassembly_plugintype")]
    [RelationshipMetadata("OneToMany", "pluginassemblyid", "plugintype", "pluginassemblyid", "Referenced")]
    public IEnumerable<PluginType> pluginassembly_plugintype
    {
        get => GetRelatedEntities<PluginType>("pluginassembly_plugintype", null);
        set => SetRelatedEntities("pluginassembly_plugintype", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the PluginAssembly entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<PluginAssembly, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the PluginAssembly with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of PluginAssembly to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved PluginAssembly</returns>
    public static PluginAssembly Retrieve(IOrganizationService service, Guid id, params Expression<Func<PluginAssembly, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }
}