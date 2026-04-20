using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>Managed Identity for plugin assemblies.</para>
/// <para>Display Name: Managed Identity</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("managedidentity")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class ManagedIdentity : ExtendedEntity
#pragma warning restore CS8981
{
	public const string EntityLogicalName = "managedidentity";

	public ManagedIdentity() : base(EntityLogicalName) { }
	public ManagedIdentity(Guid id) : base(EntityLogicalName, id) { }

	private string DebuggerDisplay => GetDebuggerDisplay("name");

	[AttributeLogicalName("managedidentityid")]
	public override Guid Id
	{
		get => base.Id;
		set => SetId("managedidentityid", value);
	}

	/// <summary>
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
	/// <para>Azure AD application (client) ID.</para>
	/// <para>Display Name: Application Id</para>
	/// </summary>
	[AttributeLogicalName("applicationid")]
	[DisplayName("Application Id")]
	public Guid? ApplicationId
	{
		get => GetAttributeValue<Guid?>("applicationid");
		set => SetAttributeValue("applicationid", value);
	}

	/// <summary>
	/// <para>Azure AD tenant ID.</para>
	/// <para>Display Name: Tenant Id</para>
	/// </summary>
	[AttributeLogicalName("tenantid")]
	[DisplayName("Tenant Id")]
	public Guid? TenantId
	{
		get => GetAttributeValue<Guid?>("tenantid");
		set => SetAttributeValue("tenantid", value);
	}

	/// <summary>
	/// <para>Credential source for the managed identity.</para>
	/// <para>Display Name: Credential Source</para>
	/// </summary>
	[AttributeLogicalName("credentialsource")]
	[DisplayName("Credential Source")]
	public managedidentity_credentialsource? CredentialSource
	{
		get => this.GetOptionSetValue<managedidentity_credentialsource>("credentialsource");
		set => this.SetOptionSetValue("credentialsource", value);
	}

	/// <summary>
	/// <para>Subject scope for the managed identity.</para>
	/// <para>Display Name: Subject Scope</para>
	/// </summary>
	[AttributeLogicalName("subjectscope")]
	[DisplayName("Subject Scope")]
	public managedidentity_subjectscope? SubjectScope
	{
		get => this.GetOptionSetValue<managedidentity_subjectscope>("subjectscope");
		set => this.SetOptionSetValue("subjectscope", value);
	}

	/// <summary>
	/// <para>Version of the managed identity.</para>
	/// <para>Display Name: Managed Identity Version</para>
	/// </summary>
	[AttributeLogicalName("managedidentityversion")]
	[DisplayName("Managed Identity Version")]
	public int? ManagedIdentityVersion
	{
		get => GetAttributeValue<int?>("managedidentityversion");
		set => SetAttributeValue("managedidentityversion", value);
	}
}
