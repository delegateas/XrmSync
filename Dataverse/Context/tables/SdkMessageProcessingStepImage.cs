using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Copy of an entity's attributes before or after the core system operation.</para>
/// <para>Display Name: Sdk Message Processing Step Image</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("sdkmessageprocessingstepimage")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class SdkMessageProcessingStepImage : ExtendedEntity
#pragma warning restore CS8981
{
	public const string EntityLogicalName = "sdkmessageprocessingstepimage";
	public const int EntityTypeCode = 4615;

	public SdkMessageProcessingStepImage() : base(EntityLogicalName) { }
	public SdkMessageProcessingStepImage(Guid id) : base(EntityLogicalName, id) { }

	private string DebuggerDisplay => GetDebuggerDisplay("name");

	[AttributeLogicalName("sdkmessageprocessingstepimageid")]
	public override Guid Id
	{
		get
		{
			return base.Id;
		}
		set
		{
			SetId("sdkmessageprocessingstepimageid", value);
		}
	}

	/// <summary>
	/// <para>Comma-separated list of attributes that are to be passed into the SDK message processing step image.</para>
	/// <para>Display Name: Attributes</para>
	/// </summary>
	[AttributeLogicalName("attributes")]
	[DisplayName("Attributes")]
	[MaxLength(100000)]
	public string Attributes_1
	{
		get => GetAttributeValue<string>("attributes");
		set => SetAttributeValue("attributes", value);
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
	/// <para>Unique identifier of the user who created the SDK message processing step image.</para>
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
	/// <para>Date and time when the SDK message processing step image was created.</para>
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
	/// <para>Unique identifier of the delegate user who created the sdkmessageprocessingstepimage.</para>
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
	/// <para>Customization level of the SDK message processing step image.</para>
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
	/// <para>Description of the SDK message processing step image.</para>
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
	/// <para>Key name used to access the pre-image or post-image property bags in a step.</para>
	/// <para>Display Name: Entity Alias</para>
	/// </summary>
	[AttributeLogicalName("entityalias")]
	[DisplayName("Entity Alias")]
	[MaxLength(256)]
	public string EntityAlias
	{
		get => GetAttributeValue<string>("entityalias");
		set => SetAttributeValue("entityalias", value);
	}

	/// <summary>
	/// <para>Type of image requested.</para>
	/// <para>Display Name: Image Type</para>
	/// </summary>
	[AttributeLogicalName("imagetype")]
	[DisplayName("Image Type")]
	public sdkmessageprocessingstepimage_imagetype? ImageType
	{
		get => this.GetOptionSetValue<sdkmessageprocessingstepimage_imagetype>("imagetype");
		set => this.SetOptionSetValue("imagetype", value);
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
	/// <para>Name of the property on the Request message.</para>
	/// <para>Display Name: Message Property Name</para>
	/// </summary>
	[AttributeLogicalName("messagepropertyname")]
	[DisplayName("Message Property Name")]
	[MaxLength(256)]
	public string MessagePropertyName
	{
		get => GetAttributeValue<string>("messagepropertyname");
		set => SetAttributeValue("messagepropertyname", value);
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
	/// <para>Display Name: Modified By</para>
	/// </summary>
	[AttributeLogicalName("modifiedon")]
	[DisplayName("Modified By")]
	public DateTime? ModifiedOn
	{
		get => GetAttributeValue<DateTime?>("modifiedon");
		set => SetAttributeValue("modifiedon", value);
	}

	/// <summary>
	/// <para>Unique identifier of the delegate user who last modified the sdkmessageprocessingstepimage.</para>
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
	/// <para>Name of SdkMessage processing step image.</para>
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
	/// <para>Name of the related entity.</para>
	/// <para>Display Name: Related Attribute Name</para>
	/// </summary>
	[AttributeLogicalName("relatedattributename")]
	[DisplayName("Related Attribute Name")]
	[MaxLength(256)]
	public string RelatedAttributeName
	{
		get => GetAttributeValue<string>("relatedattributename");
		set => SetAttributeValue("relatedattributename", value);
	}

	/// <summary>
	/// <para>Unique identifier of the SDK message processing step.</para>
	/// <para>Display Name: SDK Message Processing Step</para>
	/// </summary>
	[AttributeLogicalName("sdkmessageprocessingstepid")]
	[DisplayName("SDK Message Processing Step")]
	public EntityReference? SdkMessageProcessingStepId
	{
		get => GetAttributeValue<EntityReference?>("sdkmessageprocessingstepid");
		set => SetAttributeValue("sdkmessageprocessingstepid", value);
	}

	/// <summary>
	/// <para>Display Name: sdkmessageprocessingstepimageid</para>
	/// </summary>
	[AttributeLogicalName("sdkmessageprocessingstepimageid")]
	[DisplayName("sdkmessageprocessingstepimageid")]
	public Guid SdkMessageProcessingStepImageId
	{
		get => GetAttributeValue<Guid>("sdkmessageprocessingstepimageid");
		set => SetId("sdkmessageprocessingstepimageid", value);
	}

	/// <summary>
	/// <para>Unique identifier of the SDK message processing step image.</para>
	/// <para>Display Name: sdkmessageprocessingstepimageidunique</para>
	/// </summary>
	[AttributeLogicalName("sdkmessageprocessingstepimageidunique")]
	[DisplayName("sdkmessageprocessingstepimageidunique")]
	public Guid? SdkMessageProcessingStepImageIdUnique
	{
		get => GetAttributeValue<Guid?>("sdkmessageprocessingstepimageidunique");
		set => SetAttributeValue("sdkmessageprocessingstepimageidunique", value);
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
	/// <para>Number that identifies a specific revision of the step image.</para>
	/// <para>Display Name: versionnumber</para>
	/// </summary>
	[AttributeLogicalName("versionnumber")]
	[DisplayName("versionnumber")]
	public long? VersionNumber
	{
		get => GetAttributeValue<long?>("versionnumber");
		set => SetAttributeValue("versionnumber", value);
	}

	[AttributeLogicalName("sdkmessageprocessingstepid")]
	[RelationshipSchemaName("sdkmessageprocessingstepid_sdkmessageprocessingstepimage")]
	[RelationshipMetadata("ManyToOne", "sdkmessageprocessingstepid", "sdkmessageprocessingstep", "sdkmessageprocessingstepid", "Referencing")]
	public SdkMessageProcessingStep sdkmessageprocessingstepid_sdkmessageprocessingstepimage
	{
		get => GetRelatedEntity<SdkMessageProcessingStep>("sdkmessageprocessingstepid_sdkmessageprocessingstepimage", null);
		set => SetRelatedEntity("sdkmessageprocessingstepid_sdkmessageprocessingstepimage", null, value);
	}

	[AttributeLogicalName("modifiedby")]
	[RelationshipSchemaName("modifiedby_sdkmessageprocessingstepimage")]
	[RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser modifiedby_sdkmessageprocessingstepimage
	{
		get => GetRelatedEntity<SystemUser>("modifiedby_sdkmessageprocessingstepimage", null);
		set => SetRelatedEntity("modifiedby_sdkmessageprocessingstepimage", null, value);
	}

	[AttributeLogicalName("createdonbehalfby")]
	[RelationshipSchemaName("lk_sdkmessageprocessingstepimage_createdonbehalfby")]
	[RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_sdkmessageprocessingstepimage_createdonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_sdkmessageprocessingstepimage_createdonbehalfby", null);
		set => SetRelatedEntity("lk_sdkmessageprocessingstepimage_createdonbehalfby", null, value);
	}

	[AttributeLogicalName("createdby")]
	[RelationshipSchemaName("createdby_sdkmessageprocessingstepimage")]
	[RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser createdby_sdkmessageprocessingstepimage
	{
		get => GetRelatedEntity<SystemUser>("createdby_sdkmessageprocessingstepimage", null);
		set => SetRelatedEntity("createdby_sdkmessageprocessingstepimage", null, value);
	}

	[AttributeLogicalName("modifiedonbehalfby")]
	[RelationshipSchemaName("lk_sdkmessageprocessingstepimage_modifiedonbehalfby")]
	[RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_sdkmessageprocessingstepimage_modifiedonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_sdkmessageprocessingstepimage_modifiedonbehalfby", null);
		set => SetRelatedEntity("lk_sdkmessageprocessingstepimage_modifiedonbehalfby", null, value);
	}

	/// <summary>
	/// Gets the logical column name for a property on the SdkMessageProcessingStepImage entity, using the AttributeLogicalNameAttribute if present.
	/// </summary>
	/// <param name="column">Expression to pick the column</param>
	/// <returns>Name of column</returns>
	/// <exception cref="ArgumentNullException">If no expression is provided</exception>
	/// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
	public static string GetColumnName(Expression<Func<SdkMessageProcessingStepImage, object>> column)
	{
		return TableAttributeHelpers.GetColumnName(column);
	}

	/// <summary>
	/// Retrieves the SdkMessageProcessingStepImage with the specified columns.
	/// </summary>
	/// <param name="service">Organization service</param>
	/// <param name="id">Id of SdkMessageProcessingStepImage to retrieve</param>
	/// <param name="columns">Expressions that specify columns to retrieve</param>
	/// <returns>The retrieved SdkMessageProcessingStepImage</returns>
	public static SdkMessageProcessingStepImage Retrieve(IOrganizationService service, Guid id, params Expression<Func<SdkMessageProcessingStepImage, object>>[] columns)
	{
		return service.Retrieve(id, columns);
	}
}
