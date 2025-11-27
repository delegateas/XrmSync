using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Message that is supported by the SDK.</para>
/// <para>Display Name: Sdk Message</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("sdkmessage")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class SdkMessage : ExtendedEntity
#pragma warning restore CS8981
{
	public const string EntityLogicalName = "sdkmessage";
	public const int EntityTypeCode = 4606;

	public SdkMessage() : base(EntityLogicalName) { }
	public SdkMessage(Guid id) : base(EntityLogicalName, id) { }

	private string DebuggerDisplay => GetDebuggerDisplay("name");

	[AttributeLogicalName("sdkmessageid")]
	public override Guid Id
	{
		get
		{
			return base.Id;
		}
		set
		{
			SetId("sdkmessageid", value);
		}
	}

	/// <summary>
	/// <para>Information about whether the SDK message is automatically transacted.</para>
	/// <para>Display Name: Auto Transact</para>
	/// </summary>
	[AttributeLogicalName("autotransact")]
	[DisplayName("Auto Transact")]
	public bool? AutoTransact
	{
		get => GetAttributeValue<bool?>("autotransact");
		set => SetAttributeValue("autotransact", value);
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
	/// <para>If this is a categorized method, this is the name, otherwise None.</para>
	/// <para>Display Name: Category Name</para>
	/// </summary>
	[AttributeLogicalName("categoryname")]
	[DisplayName("Category Name")]
	[MaxLength(25)]
	public string CategoryName
	{
		get => GetAttributeValue<string>("categoryname");
		set => SetAttributeValue("categoryname", value);
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
	/// <para>Unique identifier of the user who created the SDK message.</para>
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
	/// <para>Date and time when the SDK message was created.</para>
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
	/// <para>Unique identifier of the delegate user who created the sdkmessage.</para>
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
	/// <para>Customization level of the SDK message.</para>
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
	/// <para>Name of the privilege that allows execution of the SDK message</para>
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
	/// <para>Indicates whether the SDK message should have its requests expanded per primary entity defined in its filters.</para>
	/// <para>Display Name: Expand</para>
	/// </summary>
	[AttributeLogicalName("expand")]
	[DisplayName("Expand")]
	public bool? Expand
	{
		get => GetAttributeValue<bool?>("expand");
		set => SetAttributeValue("expand", value);
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
	/// <para>Information about whether the SDK message is active.</para>
	/// <para>Display Name: Is Active</para>
	/// </summary>
	[AttributeLogicalName("isactive")]
	[DisplayName("Is Active")]
	public bool? IsActive
	{
		get => GetAttributeValue<bool?>("isactive");
		set => SetAttributeValue("isactive", value);
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
	/// <para>Indicates whether the SDK message is private.</para>
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
	/// <para>Identifies whether an SDK message will be ReadOnly or Read Write. false - ReadWrite, true - ReadOnly .</para>
	/// <para>Display Name: Intent</para>
	/// </summary>
	[AttributeLogicalName("isreadonly")]
	[DisplayName("Intent")]
	public bool? IsReadOnly
	{
		get => GetAttributeValue<bool?>("isreadonly");
		set => SetAttributeValue("isreadonly", value);
	}

	/// <summary>
	/// <para>For internal use only.</para>
	/// <para>Display Name: Is Valid for Execute Async</para>
	/// </summary>
	[AttributeLogicalName("isvalidforexecuteasync")]
	[DisplayName("Is Valid for Execute Async")]
	public bool? IsValidForExecuteAsync
	{
		get => GetAttributeValue<bool?>("isvalidforexecuteasync");
		set => SetAttributeValue("isvalidforexecuteasync", value);
	}

	/// <summary>
	/// <para>Unique identifier of the user who last modified the SDK message.</para>
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
	/// <para>Date and time when the SDK message was last modified.</para>
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
	/// <para>Unique identifier of the delegate user who last modified the sdkmessage.</para>
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
	/// <para>Name of the SDK message.</para>
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
	/// <para>Unique identifier of the organization with which the SDK message is associated.</para>
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
	/// <para>Display Name: sdkmessageid</para>
	/// </summary>
	[AttributeLogicalName("sdkmessageid")]
	[DisplayName("sdkmessageid")]
	public Guid SdkMessageId
	{
		get => GetAttributeValue<Guid>("sdkmessageid");
		set => SetId("sdkmessageid", value);
	}

	/// <summary>
	/// <para>Unique identifier of the SDK message.</para>
	/// <para>Display Name: sdkmessageidunique</para>
	/// </summary>
	[AttributeLogicalName("sdkmessageidunique")]
	[DisplayName("sdkmessageidunique")]
	public Guid? SdkMessageIdUnique
	{
		get => GetAttributeValue<Guid?>("sdkmessageidunique");
		set => SetAttributeValue("sdkmessageidunique", value);
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
	/// <para>Indicates whether the SDK message is a template.</para>
	/// <para>Display Name: Template</para>
	/// </summary>
	[AttributeLogicalName("template")]
	[DisplayName("Template")]
	public bool? Template
	{
		get => GetAttributeValue<bool?>("template");
		set => SetAttributeValue("template", value);
	}

	/// <summary>
	/// <para>For internal use only.</para>
	/// <para>Display Name: Throttle Settings</para>
	/// </summary>
	[AttributeLogicalName("throttlesettings")]
	[DisplayName("Throttle Settings")]
	[MaxLength(512)]
	public string ThrottleSettings
	{
		get => GetAttributeValue<string>("throttlesettings");
		set => SetAttributeValue("throttlesettings", value);
	}

	/// <summary>
	/// <para>Number that identifies a specific revision of the SDK message.</para>
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

	[AttributeLogicalName("createdonbehalfby")]
	[RelationshipSchemaName("lk_sdkmessage_createdonbehalfby")]
	[RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_sdkmessage_createdonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_sdkmessage_createdonbehalfby", null);
		set => SetRelatedEntity("lk_sdkmessage_createdonbehalfby", null, value);
	}

	[AttributeLogicalName("createdby")]
	[RelationshipSchemaName("createdby_sdkmessage")]
	[RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser createdby_sdkmessage
	{
		get => GetRelatedEntity<SystemUser>("createdby_sdkmessage", null);
		set => SetRelatedEntity("createdby_sdkmessage", null, value);
	}

	[AttributeLogicalName("modifiedby")]
	[RelationshipSchemaName("modifiedby_sdkmessage")]
	[RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser modifiedby_sdkmessage
	{
		get => GetRelatedEntity<SystemUser>("modifiedby_sdkmessage", null);
		set => SetRelatedEntity("modifiedby_sdkmessage", null, value);
	}

	[AttributeLogicalName("modifiedonbehalfby")]
	[RelationshipSchemaName("lk_sdkmessage_modifiedonbehalfby")]
	[RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_sdkmessage_modifiedonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_sdkmessage_modifiedonbehalfby", null);
		set => SetRelatedEntity("lk_sdkmessage_modifiedonbehalfby", null, value);
	}

	[RelationshipSchemaName("sdkmessageid_sdkmessageprocessingstep")]
	[RelationshipMetadata("OneToMany", "sdkmessageid", "sdkmessageprocessingstep", "sdkmessageid", "Referenced")]
	public IEnumerable<SdkMessageProcessingStep> sdkmessageid_sdkmessageprocessingstep
	{
		get => GetRelatedEntities<SdkMessageProcessingStep>("sdkmessageid_sdkmessageprocessingstep", null);
		set => SetRelatedEntities("sdkmessageid_sdkmessageprocessingstep", null, value);
	}

	[RelationshipSchemaName("sdkmessageid_sdkmessagefilter")]
	[RelationshipMetadata("OneToMany", "sdkmessageid", "sdkmessagefilter", "sdkmessageid", "Referenced")]
	public IEnumerable<SdkMessageFilter> sdkmessageid_sdkmessagefilter
	{
		get => GetRelatedEntities<SdkMessageFilter>("sdkmessageid_sdkmessagefilter", null);
		set => SetRelatedEntities("sdkmessageid_sdkmessagefilter", null, value);
	}

	[RelationshipSchemaName("sdkmessage_customapi")]
	[RelationshipMetadata("OneToMany", "sdkmessageid", "customapi", "sdkmessageid", "Referenced")]
	public IEnumerable<CustomAPI> sdkmessage_customapi
	{
		get => GetRelatedEntities<CustomAPI>("sdkmessage_customapi", null);
		set => SetRelatedEntities("sdkmessage_customapi", null, value);
	}

	/// <summary>
	/// Gets the logical column name for a property on the SdkMessage entity, using the AttributeLogicalNameAttribute if present.
	/// </summary>
	/// <param name="column">Expression to pick the column</param>
	/// <returns>Name of column</returns>
	/// <exception cref="ArgumentNullException">If no expression is provided</exception>
	/// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
	public static string GetColumnName(Expression<Func<SdkMessage, object>> column)
	{
		return TableAttributeHelpers.GetColumnName(column);
	}

	/// <summary>
	/// Retrieves the SdkMessage with the specified columns.
	/// </summary>
	/// <param name="service">Organization service</param>
	/// <param name="id">Id of SdkMessage to retrieve</param>
	/// <param name="columns">Expressions that specify columns to retrieve</param>
	/// <returns>The retrieved SdkMessage</returns>
	public static SdkMessage Retrieve(IOrganizationService service, Guid id, params Expression<Func<SdkMessage, object>>[] columns)
	{
		return service.Retrieve(id, columns);
	}
}
