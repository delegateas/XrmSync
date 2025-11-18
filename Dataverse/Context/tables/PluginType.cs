using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Type that inherits from the IPlugin interface and is contained within a plug-in assembly.</para>
/// <para>Display Name: Plug-in Type</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("plugintype")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class PluginType : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "plugintype";
    public const int EntityTypeCode = 4602;

    public PluginType() : base(EntityLogicalName) { }
    public PluginType(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("name");

    [AttributeLogicalName("plugintypeid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("plugintypeid", value);
        }
    }

    /// <summary>
    /// <para>Full path name of the plug-in assembly.</para>
    /// <para>Display Name: Assembly Name</para>
    /// </summary>
    [AttributeLogicalName("assemblyname")]
    [DisplayName("Assembly Name")]
    [MaxLength(100)]
    public string AssemblyName
    {
        get => GetAttributeValue<string>("assemblyname");
        set => SetAttributeValue("assemblyname", value);
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
    /// <para>Unique identifier of the user who created the plug-in type.</para>
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
    /// <para>Date and time when the plug-in type was created.</para>
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
    /// <para>Unique identifier of the delegate user who created the plugintype.</para>
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
    /// <para>Customization level of the plug-in type.</para>
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
    /// <para>Serialized Custom Activity Type information, including required arguments. For more information, see SandboxCustomActivityInfo.</para>
    /// <para>Display Name: Custom Workflow Activity Info</para>
    /// </summary>
    [AttributeLogicalName("customworkflowactivityinfo")]
    [DisplayName("Custom Workflow Activity Info")]
    [MaxLength(1048576)]
    public string CustomWorkflowActivityInfo
    {
        get => GetAttributeValue<string>("customworkflowactivityinfo");
        set => SetAttributeValue("customworkflowactivityinfo", value);
    }

    /// <summary>
    /// <para>Description of the plug-in type.</para>
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
    /// <para>User friendly name for the plug-in.</para>
    /// <para>Display Name: Display Name</para>
    /// </summary>
    [AttributeLogicalName("friendlyname")]
    [DisplayName("Display Name")]
    [MaxLength(256)]
    public string FriendlyName
    {
        get => GetAttributeValue<string>("friendlyname");
        set => SetAttributeValue("friendlyname", value);
    }

    /// <summary>
    /// <para>Display Name: ismanaged</para>
    /// </summary>
    [AttributeLogicalName("ismanaged")]
    [DisplayName("ismanaged")]
    public bool? IsManaged
    {
        get => GetAttributeValue<bool?>("ismanaged");
        set => SetAttributeValue("ismanaged", value);
    }

    /// <summary>
    /// <para>Indicates if the plug-in is a custom activity for workflows.</para>
    /// <para>Display Name: Is Workflow Activity</para>
    /// </summary>
    [AttributeLogicalName("isworkflowactivity")]
    [DisplayName("Is Workflow Activity")]
    public bool? IsWorkflowActivity
    {
        get => GetAttributeValue<bool?>("isworkflowactivity");
        set => SetAttributeValue("isworkflowactivity", value);
    }

    /// <summary>
    /// <para>Major of the version number of the assembly for the plug-in type.</para>
    /// <para>Display Name: Version major</para>
    /// </summary>
    [AttributeLogicalName("major")]
    [DisplayName("Version major")]
    [Range(0, 65534)]
    public int? Major
    {
        get => GetAttributeValue<int?>("major");
        set => SetAttributeValue("major", value);
    }

    /// <summary>
    /// <para>Minor of the version number of the assembly for the plug-in type.</para>
    /// <para>Display Name: Version minor</para>
    /// </summary>
    [AttributeLogicalName("minor")]
    [DisplayName("Version minor")]
    [Range(0, 65534)]
    public int? Minor
    {
        get => GetAttributeValue<int?>("minor");
        set => SetAttributeValue("minor", value);
    }

    /// <summary>
    /// <para>Unique identifier of the user who last modified the plug-in type.</para>
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
    /// <para>Date and time when the plug-in type was last modified.</para>
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
    /// <para>Unique identifier of the delegate user who last modified the plugintype.</para>
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
    /// <para>Name of the plug-in type.</para>
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
    /// <para>Unique identifier of the organization with which the plug-in type is associated.</para>
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
    /// <para>Unique identifier of the plug-in assembly that contains this plug-in type.</para>
    /// <para>Display Name: Plugin Assembly</para>
    /// </summary>
    [AttributeLogicalName("pluginassemblyid")]
    [DisplayName("Plugin Assembly")]
    public EntityReference? PluginAssemblyId
    {
        get => GetAttributeValue<EntityReference?>("pluginassemblyid");
        set => SetAttributeValue("pluginassemblyid", value);
    }

    /// <summary>
    /// <para>Uniquely identifies the plug-in type associated with a plugin package when exporting a solution.</para>
    /// <para>Display Name: Plugin Type export key</para>
    /// </summary>
    [AttributeLogicalName("plugintypeexportkey")]
    [DisplayName("Plugin Type export key")]
    [MaxLength(256)]
    public string PluginTypeExportKey
    {
        get => GetAttributeValue<string>("plugintypeexportkey");
        set => SetAttributeValue("plugintypeexportkey", value);
    }

    /// <summary>
    /// <para>Display Name: Plug-in Type</para>
    /// </summary>
    [AttributeLogicalName("plugintypeid")]
    [DisplayName("Plug-in Type")]
    public Guid PluginTypeId
    {
        get => GetAttributeValue<Guid>("plugintypeid");
        set => SetId("plugintypeid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the plug-in type.</para>
    /// <para>Display Name: plugintypeidunique</para>
    /// </summary>
    [AttributeLogicalName("plugintypeidunique")]
    [DisplayName("plugintypeidunique")]
    public Guid? PluginTypeIdUnique
    {
        get => GetAttributeValue<Guid?>("plugintypeidunique");
        set => SetAttributeValue("plugintypeidunique", value);
    }

    /// <summary>
    /// <para>Public key token of the assembly for the plug-in type.</para>
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
    /// <para>Fully qualified type name of the plug-in type.</para>
    /// <para>Display Name: Type Name</para>
    /// </summary>
    [AttributeLogicalName("typename")]
    [DisplayName("Type Name")]
    [MaxLength(256)]
    public string TypeName
    {
        get => GetAttributeValue<string>("typename");
        set => SetAttributeValue("typename", value);
    }

    /// <summary>
    /// <para>Version number of the assembly for the plug-in type.</para>
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

    /// <summary>
    /// <para>Group name of workflow custom activity.</para>
    /// <para>Display Name: Workflow Activity Group Name</para>
    /// </summary>
    [AttributeLogicalName("workflowactivitygroupname")]
    [DisplayName("Workflow Activity Group Name")]
    [MaxLength(256)]
    public string WorkflowActivityGroupName
    {
        get => GetAttributeValue<string>("workflowactivitygroupname");
        set => SetAttributeValue("workflowactivitygroupname", value);
    }

    [AttributeLogicalName("createdonbehalfby")]
    [RelationshipSchemaName("lk_plugintype_createdonbehalfby")]
    [RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_plugintype_createdonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_plugintype_createdonbehalfby", null);
        set => SetRelatedEntity("lk_plugintype_createdonbehalfby", null, value);
    }

    [AttributeLogicalName("modifiedonbehalfby")]
    [RelationshipSchemaName("lk_plugintype_modifiedonbehalfby")]
    [RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser lk_plugintype_modifiedonbehalfby
    {
        get => GetRelatedEntity<SystemUser>("lk_plugintype_modifiedonbehalfby", null);
        set => SetRelatedEntity("lk_plugintype_modifiedonbehalfby", null, value);
    }

    [AttributeLogicalName("pluginassemblyid")]
    [RelationshipSchemaName("pluginassembly_plugintype")]
    [RelationshipMetadata("ManyToOne", "pluginassemblyid", "pluginassembly", "pluginassemblyid", "Referencing")]
    public PluginAssembly pluginassembly_plugintype
    {
        get => GetRelatedEntity<PluginAssembly>("pluginassembly_plugintype", null);
        set => SetRelatedEntity("pluginassembly_plugintype", null, value);
    }

    [AttributeLogicalName("createdby")]
    [RelationshipSchemaName("createdby_plugintype")]
    [RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser createdby_plugintype
    {
        get => GetRelatedEntity<SystemUser>("createdby_plugintype", null);
        set => SetRelatedEntity("createdby_plugintype", null, value);
    }

    [AttributeLogicalName("modifiedby")]
    [RelationshipSchemaName("modifiedby_plugintype")]
    [RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
    public SystemUser modifiedby_plugintype
    {
        get => GetRelatedEntity<SystemUser>("modifiedby_plugintype", null);
        set => SetRelatedEntity("modifiedby_plugintype", null, value);
    }

    [RelationshipSchemaName("plugintype_sdkmessageprocessingstep")]
    [RelationshipMetadata("OneToMany", "plugintypeid", "sdkmessageprocessingstep", "eventhandler", "Referenced")]
    public IEnumerable<SdkMessageProcessingStep> plugintype_sdkmessageprocessingstep
    {
        get => GetRelatedEntities<SdkMessageProcessingStep>("plugintype_sdkmessageprocessingstep", null);
        set => SetRelatedEntities("plugintype_sdkmessageprocessingstep", null, value);
    }

    [RelationshipSchemaName("plugintypeid_sdkmessageprocessingstep")]
    [RelationshipMetadata("OneToMany", "plugintypeid", "sdkmessageprocessingstep", "plugintypeid", "Referenced")]
    public IEnumerable<SdkMessageProcessingStep> plugintypeid_sdkmessageprocessingstep
    {
        get => GetRelatedEntities<SdkMessageProcessingStep>("plugintypeid_sdkmessageprocessingstep", null);
        set => SetRelatedEntities("plugintypeid_sdkmessageprocessingstep", null, value);
    }

    [RelationshipSchemaName("plugintype_customapi")]
    [RelationshipMetadata("OneToMany", "plugintypeid", "customapi", "plugintypeid", "Referenced")]
    public IEnumerable<CustomAPI> plugintype_customapi
    {
        get => GetRelatedEntities<CustomAPI>("plugintype_customapi", null);
        set => SetRelatedEntities("plugintype_customapi", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the PluginType entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<PluginType, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the PluginType with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of PluginType to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved PluginType</returns>
    public static PluginType Retrieve(IOrganizationService service, Guid id, params Expression<Func<PluginType, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }

    /// <summary>
    /// Retrieves the PluginType using the Plugin Type Entity Key1 alternate key.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="ComponentState">ComponentState key value</param>
    /// <param name="OverwriteTime">OverwriteTime key value</param>
    /// <param name="PluginTypeExportKey">PluginTypeExportKey key value</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved PluginType</returns>
    public static PluginType Retrieve_PluginTypeEntityKey(IOrganizationService service, componentstate ComponentState, DateTime OverwriteTime, string PluginTypeExportKey, params Expression<Func<PluginType, object>>[] columns)
    {
        var keyedEntityReference = new EntityReference(EntityLogicalName, new KeyAttributeCollection
        {
            ["componentstate"] = ComponentState,
            ["overwritetime"] = OverwriteTime,
            ["plugintypeexportkey"] = PluginTypeExportKey,
        });

        return service.Retrieve(keyedEntityReference, columns);
    }
}