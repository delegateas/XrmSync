using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Data equivalent to files used in Web development. Web resources provide client-side components that are used to provide custom user interface elements.</para>
/// <para>Display Name: Web Resource</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("webresource")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class WebResource : ExtendedEntity
#pragma warning restore CS8981
{
	public const string EntityLogicalName = "webresource";
	public const int EntityTypeCode = 9333;

	public WebResource() : base(EntityLogicalName) { }
	public WebResource(Guid id) : base(EntityLogicalName, id) { }

	private string DebuggerDisplay => GetDebuggerDisplay("name");

	[AttributeLogicalName("webresourceid")]
	public override Guid Id
	{
		get
		{
			return base.Id;
		}
		set
		{
			SetId("webresourceid", value);
		}
	}

	/// <summary>
	/// <para>Information that specifies whether this component can be deleted.</para>
	/// <para>Display Name: Can Be Deleted</para>
	/// </summary>
	[AttributeLogicalName("canbedeleted")]
	[DisplayName("Can Be Deleted")]
	public BooleanManagedProperty CanBeDeleted
	{
		get => GetAttributeValue<BooleanManagedProperty>("canbedeleted");
		set => SetAttributeValue("canbedeleted", value);
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
	/// <para>Bytes of the web resource, in Base64 format.</para>
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
	/// <para>Reference to the content file on Azure.</para>
	/// <para>Display Name: ContentFileRef</para>
	/// </summary>
	[AttributeLogicalName("contentfileref")]
	[DisplayName("ContentFileRef")]
	public byte[] ContentFileRef
	{
		get => GetAttributeValue<byte[]>("contentfileref");
		set => SetAttributeValue("contentfileref", value);
	}

	/// <summary>
	/// <para>Json representation of the content of the resource.</para>
	/// <para>Display Name: contentjson</para>
	/// </summary>
	[AttributeLogicalName("contentjson")]
	[DisplayName("contentjson")]
	[MaxLength(1073741823)]
	public string ContentJson
	{
		get => GetAttributeValue<string>("contentjson");
		set => SetAttributeValue("contentjson", value);
	}

	/// <summary>
	/// <para>Reference to the Json content file on Azure.</para>
	/// <para>Display Name: ContentJsonFileRef</para>
	/// </summary>
	[AttributeLogicalName("contentjsonfileref")]
	[DisplayName("ContentJsonFileRef")]
	public byte[] ContentJsonFileRef
	{
		get => GetAttributeValue<byte[]>("contentjsonfileref");
		set => SetAttributeValue("contentjsonfileref", value);
	}

	/// <summary>
	/// <para>Unique identifier of the user who created the web resource.</para>
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
	/// <para>Date and time when the web resource was created.</para>
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
	/// <para>Unique identifier of the delegate user who created the web resource.</para>
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
	/// <para>For internal use only.</para>
	/// <para>Display Name: DependencyXML</para>
	/// </summary>
	[AttributeLogicalName("dependencyxml")]
	[DisplayName("DependencyXML")]
	[MaxLength(5000)]
	public string DependencyXml
	{
		get => GetAttributeValue<string>("dependencyxml");
		set => SetAttributeValue("dependencyxml", value);
	}

	/// <summary>
	/// <para>Description of the web resource.</para>
	/// <para>Display Name: Description</para>
	/// </summary>
	[AttributeLogicalName("description")]
	[DisplayName("Description")]
	[MaxLength(2000)]
	public string Description
	{
		get => GetAttributeValue<string>("description");
		set => SetAttributeValue("description", value);
	}

	/// <summary>
	/// <para>Display name of the web resource.</para>
	/// <para>Display Name: Display Name</para>
	/// </summary>
	[AttributeLogicalName("displayname")]
	[DisplayName("Display Name")]
	[MaxLength(200)]
	public string DisplayName
	{
		get => GetAttributeValue<string>("displayname");
		set => SetAttributeValue("displayname", value);
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
	/// <para>Information that specifies whether this web resource is available for mobile client in offline mode.</para>
	/// <para>Display Name: Is Available For Mobile Offline</para>
	/// </summary>
	[AttributeLogicalName("isavailableformobileoffline")]
	[DisplayName("Is Available For Mobile Offline")]
	public bool? IsAvailableForMobileOffline
	{
		get => GetAttributeValue<bool?>("isavailableformobileoffline");
		set => SetAttributeValue("isavailableformobileoffline", value);
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
	/// <para>Information that specifies whether this web resource is enabled for mobile client.</para>
	/// <para>Display Name: Is Enabled For Mobile Client</para>
	/// </summary>
	[AttributeLogicalName("isenabledformobileclient")]
	[DisplayName("Is Enabled For Mobile Client")]
	public bool? IsEnabledForMobileClient
	{
		get => GetAttributeValue<bool?>("isenabledformobileclient");
		set => SetAttributeValue("isenabledformobileclient", value);
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
	/// <para>Language of the web resource.</para>
	/// <para>Display Name: Language</para>
	/// </summary>
	[AttributeLogicalName("languagecode")]
	[DisplayName("Language")]
	[Range(0, 2147483647)]
	public int? LanguageCode
	{
		get => GetAttributeValue<int?>("languagecode");
		set => SetAttributeValue("languagecode", value);
	}

	/// <summary>
	/// <para>Unique identifier of the user who last modified the web resource.</para>
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
	/// <para>Date and time when the web resource was last modified.</para>
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
	/// <para>Unique identifier of the delegate user who modified the web resource.</para>
	/// <para>Display Name: Created By (Delegate)</para>
	/// </summary>
	[AttributeLogicalName("modifiedonbehalfby")]
	[DisplayName("Created By (Delegate)")]
	public EntityReference? ModifiedOnBehalfBy
	{
		get => GetAttributeValue<EntityReference?>("modifiedonbehalfby");
		set => SetAttributeValue("modifiedonbehalfby", value);
	}

	/// <summary>
	/// <para>Name of the web resource.</para>
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
	/// <para>Unique identifier of the organization associated with the web resource.</para>
	/// <para>Display Name: Organization</para>
	/// </summary>
	[AttributeLogicalName("organizationid")]
	[DisplayName("Organization")]
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
	/// <para>Silverlight runtime version number required by a silverlight web resource.</para>
	/// <para>Display Name: Silverlight Version</para>
	/// </summary>
	[AttributeLogicalName("silverlightversion")]
	[DisplayName("Silverlight Version")]
	[MaxLength(20)]
	public string SilverlightVersion
	{
		get => GetAttributeValue<string>("silverlightversion");
		set => SetAttributeValue("silverlightversion", value);
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
	/// <para>Display Name: Web Resource Identifier</para>
	/// </summary>
	[AttributeLogicalName("webresourceid")]
	[DisplayName("Web Resource Identifier")]
	public Guid WebResourceId
	{
		get => GetAttributeValue<Guid>("webresourceid");
		set => SetId("webresourceid", value);
	}

	/// <summary>
	/// <para>For internal use only.</para>
	/// <para>Display Name: webresourceidunique</para>
	/// </summary>
	[AttributeLogicalName("webresourceidunique")]
	[DisplayName("webresourceidunique")]
	public Guid? WebResourceIdUnique
	{
		get => GetAttributeValue<Guid?>("webresourceidunique");
		set => SetAttributeValue("webresourceidunique", value);
	}

	/// <summary>
	/// <para>Drop-down list for selecting the type of the web resource.</para>
	/// <para>Display Name: Type</para>
	/// </summary>
	[AttributeLogicalName("webresourcetype")]
	[DisplayName("Type")]
	public webresource_webresourcetype? WebResourceType
	{
		get => this.GetOptionSetValue<webresource_webresourcetype>("webresourcetype");
		set => this.SetOptionSetValue("webresourcetype", value);
	}

	[AttributeLogicalName("modifiedby")]
	[RelationshipSchemaName("webresource_modifiedby")]
	[RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser webresource_modifiedby
	{
		get => GetRelatedEntity<SystemUser>("webresource_modifiedby", null);
		set => SetRelatedEntity("webresource_modifiedby", null, value);
	}

	[AttributeLogicalName("createdby")]
	[RelationshipSchemaName("webresource_createdby")]
	[RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser webresource_createdby
	{
		get => GetRelatedEntity<SystemUser>("webresource_createdby", null);
		set => SetRelatedEntity("webresource_createdby", null, value);
	}

	[AttributeLogicalName("modifiedonbehalfby")]
	[RelationshipSchemaName("lk_webresourcebase_modifiedonbehalfby")]
	[RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_webresourcebase_modifiedonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_webresourcebase_modifiedonbehalfby", null);
		set => SetRelatedEntity("lk_webresourcebase_modifiedonbehalfby", null, value);
	}

	[AttributeLogicalName("createdonbehalfby")]
	[RelationshipSchemaName("lk_webresourcebase_createdonbehalfby")]
	[RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_webresourcebase_createdonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_webresourcebase_createdonbehalfby", null);
		set => SetRelatedEntity("lk_webresourcebase_createdonbehalfby", null, value);
	}

	[RelationshipSchemaName("solution_configuration_webresource")]
	[RelationshipMetadata("OneToMany", "webresourceid", "solution", "configurationpageid", "Referenced")]
	public IEnumerable<Solution> solution_configuration_webresource
	{
		get => GetRelatedEntities<Solution>("solution_configuration_webresource", null);
		set => SetRelatedEntities("solution_configuration_webresource", null, value);
	}

	/// <summary>
	/// Gets the logical column name for a property on the WebResource entity, using the AttributeLogicalNameAttribute if present.
	/// </summary>
	/// <param name="column">Expression to pick the column</param>
	/// <returns>Name of column</returns>
	/// <exception cref="ArgumentNullException">If no expression is provided</exception>
	/// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
	public static string GetColumnName(Expression<Func<WebResource, object>> column)
	{
		return TableAttributeHelpers.GetColumnName(column);
	}

	/// <summary>
	/// Retrieves the WebResource with the specified columns.
	/// </summary>
	/// <param name="service">Organization service</param>
	/// <param name="id">Id of WebResource to retrieve</param>
	/// <param name="columns">Expressions that specify columns to retrieve</param>
	/// <returns>The retrieved WebResource</returns>
	public static WebResource Retrieve(IOrganizationService service, Guid id, params Expression<Func<WebResource, object>>[] columns)
	{
		return service.Retrieve(id, columns);
	}
}
