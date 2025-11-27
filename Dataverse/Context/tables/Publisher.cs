using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>A publisher of a CRM solution.</para>
/// <para>Display Name: Publisher</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("publisher")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class Publisher : ExtendedEntity
#pragma warning restore CS8981
{
	public const string EntityLogicalName = "publisher";
	public const int EntityTypeCode = 7101;

	public Publisher() : base(EntityLogicalName) { }
	public Publisher(Guid id) : base(EntityLogicalName, id) { }

	private string DebuggerDisplay => GetDebuggerDisplay("friendlyname");

	[AttributeLogicalName("publisherid")]
	public override Guid Id
	{
		get
		{
			return base.Id;
		}
		set
		{
			SetId("publisherid", value);
		}
	}

	/// <summary>
	/// <para>Unique identifier for address 1.</para>
	/// <para>Display Name: Address 1: ID</para>
	/// </summary>
	[AttributeLogicalName("address1_addressid")]
	[DisplayName("Address 1: ID")]
	public Guid? Address1_AddressId
	{
		get => GetAttributeValue<Guid?>("address1_addressid");
		set => SetAttributeValue("address1_addressid", value);
	}

	/// <summary>
	/// <para>Type of address for address 1, such as billing, shipping, or primary address.</para>
	/// <para>Display Name: Address 1: Address Type</para>
	/// </summary>
	[AttributeLogicalName("address1_addresstypecode")]
	[DisplayName("Address 1: Address Type")]
	public publisher_address1_addresstypecode? Address1_AddressTypeCode
	{
		get => this.GetOptionSetValue<publisher_address1_addresstypecode>("address1_addresstypecode");
		set => this.SetOptionSetValue("address1_addresstypecode", value);
	}

	/// <summary>
	/// <para>City name for address 1.</para>
	/// <para>Display Name: City</para>
	/// </summary>
	[AttributeLogicalName("address1_city")]
	[DisplayName("City")]
	[MaxLength(80)]
	public string Address1_City
	{
		get => GetAttributeValue<string>("address1_city");
		set => SetAttributeValue("address1_city", value);
	}

	/// <summary>
	/// <para>Country/region name for address 1.</para>
	/// <para>Display Name: Country/Region</para>
	/// </summary>
	[AttributeLogicalName("address1_country")]
	[DisplayName("Country/Region")]
	[MaxLength(80)]
	public string Address1_Country
	{
		get => GetAttributeValue<string>("address1_country");
		set => SetAttributeValue("address1_country", value);
	}

	/// <summary>
	/// <para>County name for address 1.</para>
	/// <para>Display Name: Address 1: County</para>
	/// </summary>
	[AttributeLogicalName("address1_county")]
	[DisplayName("Address 1: County")]
	[MaxLength(50)]
	public string Address1_County
	{
		get => GetAttributeValue<string>("address1_county");
		set => SetAttributeValue("address1_county", value);
	}

	/// <summary>
	/// <para>Fax number for address 1.</para>
	/// <para>Display Name: Address 1: Fax</para>
	/// </summary>
	[AttributeLogicalName("address1_fax")]
	[DisplayName("Address 1: Fax")]
	[MaxLength(50)]
	public string Address1_Fax
	{
		get => GetAttributeValue<string>("address1_fax");
		set => SetAttributeValue("address1_fax", value);
	}

	/// <summary>
	/// <para>Latitude for address 1.</para>
	/// <para>Display Name: Address 1: Latitude</para>
	/// </summary>
	[AttributeLogicalName("address1_latitude")]
	[DisplayName("Address 1: Latitude")]
	public double? Address1_Latitude
	{
		get => GetAttributeValue<double?>("address1_latitude");
		set => SetAttributeValue("address1_latitude", value);
	}

	/// <summary>
	/// <para>First line for entering address 1 information.</para>
	/// <para>Display Name: Street 1</para>
	/// </summary>
	[AttributeLogicalName("address1_line1")]
	[DisplayName("Street 1")]
	[MaxLength(50)]
	public string Address1_Line1
	{
		get => GetAttributeValue<string>("address1_line1");
		set => SetAttributeValue("address1_line1", value);
	}

	/// <summary>
	/// <para>Second line for entering address 1 information.</para>
	/// <para>Display Name: Street 2</para>
	/// </summary>
	[AttributeLogicalName("address1_line2")]
	[DisplayName("Street 2")]
	[MaxLength(50)]
	public string Address1_Line2
	{
		get => GetAttributeValue<string>("address1_line2");
		set => SetAttributeValue("address1_line2", value);
	}

	/// <summary>
	/// <para>Third line for entering address 1 information.</para>
	/// <para>Display Name: Street 3</para>
	/// </summary>
	[AttributeLogicalName("address1_line3")]
	[DisplayName("Street 3")]
	[MaxLength(50)]
	public string Address1_Line3
	{
		get => GetAttributeValue<string>("address1_line3");
		set => SetAttributeValue("address1_line3", value);
	}

	/// <summary>
	/// <para>Longitude for address 1.</para>
	/// <para>Display Name: Address 1: Longitude</para>
	/// </summary>
	[AttributeLogicalName("address1_longitude")]
	[DisplayName("Address 1: Longitude")]
	public double? Address1_Longitude
	{
		get => GetAttributeValue<double?>("address1_longitude");
		set => SetAttributeValue("address1_longitude", value);
	}

	/// <summary>
	/// <para>Name to enter for address 1.</para>
	/// <para>Display Name: Address 1: Name</para>
	/// </summary>
	[AttributeLogicalName("address1_name")]
	[DisplayName("Address 1: Name")]
	[MaxLength(100)]
	public string Address1_Name
	{
		get => GetAttributeValue<string>("address1_name");
		set => SetAttributeValue("address1_name", value);
	}

	/// <summary>
	/// <para>ZIP Code or postal code for address 1.</para>
	/// <para>Display Name: ZIP/Postal Code</para>
	/// </summary>
	[AttributeLogicalName("address1_postalcode")]
	[DisplayName("ZIP/Postal Code")]
	[MaxLength(20)]
	public string Address1_PostalCode
	{
		get => GetAttributeValue<string>("address1_postalcode");
		set => SetAttributeValue("address1_postalcode", value);
	}

	/// <summary>
	/// <para>Post office box number for address 1.</para>
	/// <para>Display Name: Address 1: Post Office Box</para>
	/// </summary>
	[AttributeLogicalName("address1_postofficebox")]
	[DisplayName("Address 1: Post Office Box")]
	[MaxLength(20)]
	public string Address1_PostOfficeBox
	{
		get => GetAttributeValue<string>("address1_postofficebox");
		set => SetAttributeValue("address1_postofficebox", value);
	}

	/// <summary>
	/// <para>Method of shipment for address 1.</para>
	/// <para>Display Name: Address 1: Shipping Method</para>
	/// </summary>
	[AttributeLogicalName("address1_shippingmethodcode")]
	[DisplayName("Address 1: Shipping Method")]
	public publisher_address1_shippingmethodcode? Address1_ShippingMethodCode
	{
		get => this.GetOptionSetValue<publisher_address1_shippingmethodcode>("address1_shippingmethodcode");
		set => this.SetOptionSetValue("address1_shippingmethodcode", value);
	}

	/// <summary>
	/// <para>State or province for address 1.</para>
	/// <para>Display Name: State/Province</para>
	/// </summary>
	[AttributeLogicalName("address1_stateorprovince")]
	[DisplayName("State/Province")]
	[MaxLength(50)]
	public string Address1_StateOrProvince
	{
		get => GetAttributeValue<string>("address1_stateorprovince");
		set => SetAttributeValue("address1_stateorprovince", value);
	}

	/// <summary>
	/// <para>First telephone number associated with address 1.</para>
	/// <para>Display Name: Phone</para>
	/// </summary>
	[AttributeLogicalName("address1_telephone1")]
	[DisplayName("Phone")]
	[MaxLength(50)]
	public string Address1_Telephone1
	{
		get => GetAttributeValue<string>("address1_telephone1");
		set => SetAttributeValue("address1_telephone1", value);
	}

	/// <summary>
	/// <para>Second telephone number associated with address 1.</para>
	/// <para>Display Name: Address 1: Telephone 2</para>
	/// </summary>
	[AttributeLogicalName("address1_telephone2")]
	[DisplayName("Address 1: Telephone 2")]
	[MaxLength(50)]
	public string Address1_Telephone2
	{
		get => GetAttributeValue<string>("address1_telephone2");
		set => SetAttributeValue("address1_telephone2", value);
	}

	/// <summary>
	/// <para>Third telephone number associated with address 1.</para>
	/// <para>Display Name: Address 1: Telephone 3</para>
	/// </summary>
	[AttributeLogicalName("address1_telephone3")]
	[DisplayName("Address 1: Telephone 3")]
	[MaxLength(50)]
	public string Address1_Telephone3
	{
		get => GetAttributeValue<string>("address1_telephone3");
		set => SetAttributeValue("address1_telephone3", value);
	}

	/// <summary>
	/// <para>United Parcel Service (UPS) zone for address 1.</para>
	/// <para>Display Name: Address 1: UPS Zone</para>
	/// </summary>
	[AttributeLogicalName("address1_upszone")]
	[DisplayName("Address 1: UPS Zone")]
	[MaxLength(4)]
	public string Address1_UPSZone
	{
		get => GetAttributeValue<string>("address1_upszone");
		set => SetAttributeValue("address1_upszone", value);
	}

	/// <summary>
	/// <para>UTC offset for address 1. This is the difference between local time and standard Coordinated Universal Time.</para>
	/// <para>Display Name: Address 1: UTC Offset</para>
	/// </summary>
	[AttributeLogicalName("address1_utcoffset")]
	[DisplayName("Address 1: UTC Offset")]
	[Range(-1500, 1500)]
	public int? Address1_UTCOffset
	{
		get => GetAttributeValue<int?>("address1_utcoffset");
		set => SetAttributeValue("address1_utcoffset", value);
	}

	/// <summary>
	/// <para>Unique identifier for address 2.</para>
	/// <para>Display Name: Address 2: ID</para>
	/// </summary>
	[AttributeLogicalName("address2_addressid")]
	[DisplayName("Address 2: ID")]
	public Guid? Address2_AddressId
	{
		get => GetAttributeValue<Guid?>("address2_addressid");
		set => SetAttributeValue("address2_addressid", value);
	}

	/// <summary>
	/// <para>Type of address for address 2. such as billing, shipping, or primary address.</para>
	/// <para>Display Name: Address 2: Address Type</para>
	/// </summary>
	[AttributeLogicalName("address2_addresstypecode")]
	[DisplayName("Address 2: Address Type")]
	public publisher_address2_addresstypecode? Address2_AddressTypeCode
	{
		get => this.GetOptionSetValue<publisher_address2_addresstypecode>("address2_addresstypecode");
		set => this.SetOptionSetValue("address2_addresstypecode", value);
	}

	/// <summary>
	/// <para>City name for address 2.</para>
	/// <para>Display Name: Address 2: City</para>
	/// </summary>
	[AttributeLogicalName("address2_city")]
	[DisplayName("Address 2: City")]
	[MaxLength(80)]
	public string Address2_City
	{
		get => GetAttributeValue<string>("address2_city");
		set => SetAttributeValue("address2_city", value);
	}

	/// <summary>
	/// <para>Country/region name for address 2.</para>
	/// <para>Display Name: Address 2: Country/Region</para>
	/// </summary>
	[AttributeLogicalName("address2_country")]
	[DisplayName("Address 2: Country/Region")]
	[MaxLength(80)]
	public string Address2_Country
	{
		get => GetAttributeValue<string>("address2_country");
		set => SetAttributeValue("address2_country", value);
	}

	/// <summary>
	/// <para>County name for address 2.</para>
	/// <para>Display Name: Address 2: County</para>
	/// </summary>
	[AttributeLogicalName("address2_county")]
	[DisplayName("Address 2: County")]
	[MaxLength(50)]
	public string Address2_County
	{
		get => GetAttributeValue<string>("address2_county");
		set => SetAttributeValue("address2_county", value);
	}

	/// <summary>
	/// <para>Fax number for address 2.</para>
	/// <para>Display Name: Address 2: Fax</para>
	/// </summary>
	[AttributeLogicalName("address2_fax")]
	[DisplayName("Address 2: Fax")]
	[MaxLength(50)]
	public string Address2_Fax
	{
		get => GetAttributeValue<string>("address2_fax");
		set => SetAttributeValue("address2_fax", value);
	}

	/// <summary>
	/// <para>Latitude for address 2.</para>
	/// <para>Display Name: Address 2: Latitude</para>
	/// </summary>
	[AttributeLogicalName("address2_latitude")]
	[DisplayName("Address 2: Latitude")]
	public double? Address2_Latitude
	{
		get => GetAttributeValue<double?>("address2_latitude");
		set => SetAttributeValue("address2_latitude", value);
	}

	/// <summary>
	/// <para>First line for entering address 2 information.</para>
	/// <para>Display Name: Address 2: Street 1</para>
	/// </summary>
	[AttributeLogicalName("address2_line1")]
	[DisplayName("Address 2: Street 1")]
	[MaxLength(50)]
	public string Address2_Line1
	{
		get => GetAttributeValue<string>("address2_line1");
		set => SetAttributeValue("address2_line1", value);
	}

	/// <summary>
	/// <para>Second line for entering address 2 information.</para>
	/// <para>Display Name: Address 2: Street 2</para>
	/// </summary>
	[AttributeLogicalName("address2_line2")]
	[DisplayName("Address 2: Street 2")]
	[MaxLength(50)]
	public string Address2_Line2
	{
		get => GetAttributeValue<string>("address2_line2");
		set => SetAttributeValue("address2_line2", value);
	}

	/// <summary>
	/// <para>Third line for entering address 2 information.</para>
	/// <para>Display Name: Address 2: Street 3</para>
	/// </summary>
	[AttributeLogicalName("address2_line3")]
	[DisplayName("Address 2: Street 3")]
	[MaxLength(50)]
	public string Address2_Line3
	{
		get => GetAttributeValue<string>("address2_line3");
		set => SetAttributeValue("address2_line3", value);
	}

	/// <summary>
	/// <para>Longitude for address 2.</para>
	/// <para>Display Name: Address 2: Longitude</para>
	/// </summary>
	[AttributeLogicalName("address2_longitude")]
	[DisplayName("Address 2: Longitude")]
	public double? Address2_Longitude
	{
		get => GetAttributeValue<double?>("address2_longitude");
		set => SetAttributeValue("address2_longitude", value);
	}

	/// <summary>
	/// <para>Name to enter for address 2.</para>
	/// <para>Display Name: Address 2: Name</para>
	/// </summary>
	[AttributeLogicalName("address2_name")]
	[DisplayName("Address 2: Name")]
	[MaxLength(100)]
	public string Address2_Name
	{
		get => GetAttributeValue<string>("address2_name");
		set => SetAttributeValue("address2_name", value);
	}

	/// <summary>
	/// <para>ZIP Code or postal code for address 2.</para>
	/// <para>Display Name: Address 2: ZIP/Postal Code</para>
	/// </summary>
	[AttributeLogicalName("address2_postalcode")]
	[DisplayName("Address 2: ZIP/Postal Code")]
	[MaxLength(20)]
	public string Address2_PostalCode
	{
		get => GetAttributeValue<string>("address2_postalcode");
		set => SetAttributeValue("address2_postalcode", value);
	}

	/// <summary>
	/// <para>Post office box number for address 2.</para>
	/// <para>Display Name: Address 2: Post Office Box</para>
	/// </summary>
	[AttributeLogicalName("address2_postofficebox")]
	[DisplayName("Address 2: Post Office Box")]
	[MaxLength(20)]
	public string Address2_PostOfficeBox
	{
		get => GetAttributeValue<string>("address2_postofficebox");
		set => SetAttributeValue("address2_postofficebox", value);
	}

	/// <summary>
	/// <para>Method of shipment for address 2.</para>
	/// <para>Display Name: Address 2: Shipping Method</para>
	/// </summary>
	[AttributeLogicalName("address2_shippingmethodcode")]
	[DisplayName("Address 2: Shipping Method")]
	public publisher_address2_shippingmethodcode? Address2_ShippingMethodCode
	{
		get => this.GetOptionSetValue<publisher_address2_shippingmethodcode>("address2_shippingmethodcode");
		set => this.SetOptionSetValue("address2_shippingmethodcode", value);
	}

	/// <summary>
	/// <para>State or province for address 2.</para>
	/// <para>Display Name: Address 2: State/Province</para>
	/// </summary>
	[AttributeLogicalName("address2_stateorprovince")]
	[DisplayName("Address 2: State/Province")]
	[MaxLength(50)]
	public string Address2_StateOrProvince
	{
		get => GetAttributeValue<string>("address2_stateorprovince");
		set => SetAttributeValue("address2_stateorprovince", value);
	}

	/// <summary>
	/// <para>First telephone number associated with address 2.</para>
	/// <para>Display Name: Address 2: Telephone 1</para>
	/// </summary>
	[AttributeLogicalName("address2_telephone1")]
	[DisplayName("Address 2: Telephone 1")]
	[MaxLength(50)]
	public string Address2_Telephone1
	{
		get => GetAttributeValue<string>("address2_telephone1");
		set => SetAttributeValue("address2_telephone1", value);
	}

	/// <summary>
	/// <para>Second telephone number associated with address 2.</para>
	/// <para>Display Name: Address 2: Telephone 2</para>
	/// </summary>
	[AttributeLogicalName("address2_telephone2")]
	[DisplayName("Address 2: Telephone 2")]
	[MaxLength(50)]
	public string Address2_Telephone2
	{
		get => GetAttributeValue<string>("address2_telephone2");
		set => SetAttributeValue("address2_telephone2", value);
	}

	/// <summary>
	/// <para>Third telephone number associated with address 2.</para>
	/// <para>Display Name: Address 2: Telephone 3</para>
	/// </summary>
	[AttributeLogicalName("address2_telephone3")]
	[DisplayName("Address 2: Telephone 3")]
	[MaxLength(50)]
	public string Address2_Telephone3
	{
		get => GetAttributeValue<string>("address2_telephone3");
		set => SetAttributeValue("address2_telephone3", value);
	}

	/// <summary>
	/// <para>United Parcel Service (UPS) zone for address 2.</para>
	/// <para>Display Name: Address 2: UPS Zone</para>
	/// </summary>
	[AttributeLogicalName("address2_upszone")]
	[DisplayName("Address 2: UPS Zone")]
	[MaxLength(4)]
	public string Address2_UPSZone
	{
		get => GetAttributeValue<string>("address2_upszone");
		set => SetAttributeValue("address2_upszone", value);
	}

	/// <summary>
	/// <para>UTC offset for address 2. This is the difference between local time and standard Coordinated Universal Time.</para>
	/// <para>Display Name: Address 2: UTC Offset</para>
	/// </summary>
	[AttributeLogicalName("address2_utcoffset")]
	[DisplayName("Address 2: UTC Offset")]
	[Range(-1500, 1500)]
	public int? Address2_UTCOffset
	{
		get => GetAttributeValue<int?>("address2_utcoffset");
		set => SetAttributeValue("address2_utcoffset", value);
	}

	/// <summary>
	/// <para>Unique identifier of the user who created the publisher.</para>
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
	/// <para>Date and time when the publisher was created.</para>
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
	/// <para>Unique identifier of the delegate user who created the publisher.</para>
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
	/// <para>Default option value prefix used for newly created options for solutions associated with this publisher.</para>
	/// <para>Display Name: Option Value Prefix</para>
	/// </summary>
	[AttributeLogicalName("customizationoptionvalueprefix")]
	[DisplayName("Option Value Prefix")]
	[Range(10000, 99999)]
	public int? CustomizationOptionValuePrefix
	{
		get => GetAttributeValue<int?>("customizationoptionvalueprefix");
		set => SetAttributeValue("customizationoptionvalueprefix", value);
	}

	/// <summary>
	/// <para>Prefix used for new entities, attributes, and entity relationships for solutions associated with this publisher.</para>
	/// <para>Display Name: Prefix</para>
	/// </summary>
	[AttributeLogicalName("customizationprefix")]
	[DisplayName("Prefix")]
	[MaxLength(8)]
	public string CustomizationPrefix
	{
		get => GetAttributeValue<string>("customizationprefix");
		set => SetAttributeValue("customizationprefix", value);
	}

	/// <summary>
	/// <para>Description of the solution.</para>
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
	/// <para>Email address for the publisher.</para>
	/// <para>Display Name: Email</para>
	/// </summary>
	[AttributeLogicalName("emailaddress")]
	[DisplayName("Email")]
	[MaxLength(100)]
	public string EMailAddress
	{
		get => GetAttributeValue<string>("emailaddress");
		set => SetAttributeValue("emailaddress", value);
	}

	/// <summary>
	/// <para>For internal use only.</para>
	/// <para>Display Name: Entity Image Id</para>
	/// </summary>
	[AttributeLogicalName("entityimageid")]
	[DisplayName("Entity Image Id")]
	public Guid? EntityImageId
	{
		get => GetAttributeValue<Guid?>("entityimageid");
		set => SetAttributeValue("entityimageid", value);
	}

	/// <summary>
	/// <para>User display name for this publisher.</para>
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
	/// <para>Indicates whether the publisher was created as part of a managed solution installation.</para>
	/// <para>Display Name: Is Read-Only Publisher</para>
	/// </summary>
	[AttributeLogicalName("isreadonly")]
	[DisplayName("Is Read-Only Publisher")]
	public bool? IsReadonly
	{
		get => GetAttributeValue<bool?>("isreadonly");
		set => SetAttributeValue("isreadonly", value);
	}

	/// <summary>
	/// <para>Unique identifier of the user who last modified the publisher.</para>
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
	/// <para>Date and time when the publisher was last modified.</para>
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
	/// <para>Unique identifier of the delegate user who modified the publisher.</para>
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
	/// <para>Unique identifier of the organization associated with the publisher.</para>
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
	/// <para>Default locale of the publisher in Microsoft Pinpoint.</para>
	/// <para>Display Name: pinpointpublisherdefaultlocale</para>
	/// </summary>
	[AttributeLogicalName("pinpointpublisherdefaultlocale")]
	[DisplayName("pinpointpublisherdefaultlocale")]
	[MaxLength(16)]
	public string PinpointPublisherDefaultLocale
	{
		get => GetAttributeValue<string>("pinpointpublisherdefaultlocale");
		set => SetAttributeValue("pinpointpublisherdefaultlocale", value);
	}

	/// <summary>
	/// <para>Identifier of the publisher in Microsoft Pinpoint.</para>
	/// <para>Display Name: pinpointpublisherid</para>
	/// </summary>
	[AttributeLogicalName("pinpointpublisherid")]
	[DisplayName("pinpointpublisherid")]
	public long? PinpointPublisherId
	{
		get => GetAttributeValue<long?>("pinpointpublisherid");
		set => SetAttributeValue("pinpointpublisherid", value);
	}

	/// <summary>
	/// <para>Display Name: Publisher Identifier</para>
	/// </summary>
	[AttributeLogicalName("publisherid")]
	[DisplayName("Publisher Identifier")]
	public Guid PublisherId
	{
		get => GetAttributeValue<Guid>("publisherid");
		set => SetId("publisherid", value);
	}

	/// <summary>
	/// <para>URL for the supporting website of this publisher.</para>
	/// <para>Display Name: Website</para>
	/// </summary>
	[AttributeLogicalName("supportingwebsiteurl")]
	[DisplayName("Website")]
	[MaxLength(200)]
	public string SupportingWebsiteUrl
	{
		get => GetAttributeValue<string>("supportingwebsiteurl");
		set => SetAttributeValue("supportingwebsiteurl", value);
	}

	/// <summary>
	/// <para>The unique name of this publisher.</para>
	/// <para>Display Name: Name</para>
	/// </summary>
	[AttributeLogicalName("uniquename")]
	[DisplayName("Name")]
	[MaxLength(256)]
	public string UniqueName
	{
		get => GetAttributeValue<string>("uniquename");
		set => SetAttributeValue("uniquename", value);
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

	[AttributeLogicalName("createdby")]
	[RelationshipSchemaName("lk_publisher_createdby")]
	[RelationshipMetadata("ManyToOne", "createdby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_publisher_createdby
	{
		get => GetRelatedEntity<SystemUser>("lk_publisher_createdby", null);
		set => SetRelatedEntity("lk_publisher_createdby", null, value);
	}

	[AttributeLogicalName("modifiedonbehalfby")]
	[RelationshipSchemaName("lk_publisherbase_modifiedonbehalfby")]
	[RelationshipMetadata("ManyToOne", "modifiedonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_publisherbase_modifiedonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_publisherbase_modifiedonbehalfby", null);
		set => SetRelatedEntity("lk_publisherbase_modifiedonbehalfby", null, value);
	}

	[AttributeLogicalName("modifiedby")]
	[RelationshipSchemaName("lk_publisher_modifiedby")]
	[RelationshipMetadata("ManyToOne", "modifiedby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_publisher_modifiedby
	{
		get => GetRelatedEntity<SystemUser>("lk_publisher_modifiedby", null);
		set => SetRelatedEntity("lk_publisher_modifiedby", null, value);
	}

	[AttributeLogicalName("createdonbehalfby")]
	[RelationshipSchemaName("lk_publisherbase_createdonbehalfby")]
	[RelationshipMetadata("ManyToOne", "createdonbehalfby", "systemuser", "systemuserid", "Referencing")]
	public SystemUser lk_publisherbase_createdonbehalfby
	{
		get => GetRelatedEntity<SystemUser>("lk_publisherbase_createdonbehalfby", null);
		set => SetRelatedEntity("lk_publisherbase_createdonbehalfby", null, value);
	}

	[RelationshipSchemaName("publisher_solution")]
	[RelationshipMetadata("OneToMany", "publisherid", "solution", "publisherid", "Referenced")]
	public IEnumerable<Solution> publisher_solution
	{
		get => GetRelatedEntities<Solution>("publisher_solution", null);
		set => SetRelatedEntities("publisher_solution", null, value);
	}

	/// <summary>
	/// Gets the logical column name for a property on the Publisher entity, using the AttributeLogicalNameAttribute if present.
	/// </summary>
	/// <param name="column">Expression to pick the column</param>
	/// <returns>Name of column</returns>
	/// <exception cref="ArgumentNullException">If no expression is provided</exception>
	/// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
	public static string GetColumnName(Expression<Func<Publisher, object>> column)
	{
		return TableAttributeHelpers.GetColumnName(column);
	}

	/// <summary>
	/// Retrieves the Publisher with the specified columns.
	/// </summary>
	/// <param name="service">Organization service</param>
	/// <param name="id">Id of Publisher to retrieve</param>
	/// <param name="columns">Expressions that specify columns to retrieve</param>
	/// <returns>The retrieved Publisher</returns>
	public static Publisher Retrieve(IOrganizationService service, Guid id, params Expression<Func<Publisher, object>>[] columns)
	{
		return service.Retrieve(id, columns);
	}
}
