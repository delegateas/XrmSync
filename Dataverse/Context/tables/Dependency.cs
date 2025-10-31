using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>A component dependency in CRM.</para>
/// <para>Display Name: Dependency</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[EntityLogicalName("dependency")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class Dependency : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "dependency";
    public const int EntityTypeCode = 7105;

    public Dependency() : base(EntityLogicalName) { }
    public Dependency(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("");

    [AttributeLogicalName("dependencyid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("dependencyid", value);
        }
    }

    /// <summary>
    /// <para>Display Name: Dependency Identifier</para>
    /// </summary>
    [AttributeLogicalName("dependencyid")]
    [DisplayName("Dependency Identifier")]
    public Guid DependencyId
    {
        get => GetAttributeValue<Guid>("dependencyid");
        set => SetId("dependencyid", value);
    }

    /// <summary>
    /// <para>The dependency type of the dependency.</para>
    /// <para>Display Name: Dependency Type</para>
    /// </summary>
    [AttributeLogicalName("dependencytype")]
    [DisplayName("Dependency Type")]
    public dependencytype? DependencyType
    {
        get => this.GetOptionSetValue<dependencytype>("dependencytype");
        set => this.SetOptionSetValue("dependencytype", value);
    }

    /// <summary>
    /// <para>Display Name: dependentcomponentbasesolutionid</para>
    /// </summary>
    [AttributeLogicalName("dependentcomponentbasesolutionid")]
    [DisplayName("dependentcomponentbasesolutionid")]
    public Guid? DependentComponentBaseSolutionId
    {
        get => GetAttributeValue<Guid?>("dependentcomponentbasesolutionid");
        set => SetAttributeValue("dependentcomponentbasesolutionid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the dependent component's node.</para>
    /// <para>Display Name: Dependent Component</para>
    /// </summary>
    [AttributeLogicalName("dependentcomponentnodeid")]
    [DisplayName("Dependent Component")]
    public EntityReference? DependentComponentNodeId
    {
        get => GetAttributeValue<EntityReference?>("dependentcomponentnodeid");
        set => SetAttributeValue("dependentcomponentnodeid", value);
    }

    /// <summary>
    /// <para>Display Name: dependentcomponentobjectid</para>
    /// </summary>
    [AttributeLogicalName("dependentcomponentobjectid")]
    [DisplayName("dependentcomponentobjectid")]
    public Guid? DependentComponentObjectId
    {
        get => GetAttributeValue<Guid?>("dependentcomponentobjectid");
        set => SetAttributeValue("dependentcomponentobjectid", value);
    }

    /// <summary>
    /// <para>Display Name: dependentcomponentparentid</para>
    /// </summary>
    [AttributeLogicalName("dependentcomponentparentid")]
    [DisplayName("dependentcomponentparentid")]
    public Guid? DependentComponentParentId
    {
        get => GetAttributeValue<Guid?>("dependentcomponentparentid");
        set => SetAttributeValue("dependentcomponentparentid", value);
    }

    /// <summary>
    /// <para>Display Name: dependentcomponenttype</para>
    /// </summary>
    [AttributeLogicalName("dependentcomponenttype")]
    [DisplayName("dependentcomponenttype")]
    public componenttype? DependentComponentType
    {
        get => this.GetOptionSetValue<componenttype>("dependentcomponenttype");
        set => this.SetOptionSetValue("dependentcomponenttype", value);
    }

    /// <summary>
    /// <para>Display Name: requiredcomponentbasesolutionid</para>
    /// </summary>
    [AttributeLogicalName("requiredcomponentbasesolutionid")]
    [DisplayName("requiredcomponentbasesolutionid")]
    public Guid? RequiredComponentBaseSolutionId
    {
        get => GetAttributeValue<Guid?>("requiredcomponentbasesolutionid");
        set => SetAttributeValue("requiredcomponentbasesolutionid", value);
    }

    /// <summary>
    /// <para>Display Name: requiredcomponentintroducedversion</para>
    /// </summary>
    [AttributeLogicalName("requiredcomponentintroducedversion")]
    [DisplayName("requiredcomponentintroducedversion")]
    public double? RequiredComponentIntroducedVersion
    {
        get => GetAttributeValue<double?>("requiredcomponentintroducedversion");
        set => SetAttributeValue("requiredcomponentintroducedversion", value);
    }

    /// <summary>
    /// <para>Unique identifier of the required component's node</para>
    /// <para>Display Name: Required Component</para>
    /// </summary>
    [AttributeLogicalName("requiredcomponentnodeid")]
    [DisplayName("Required Component")]
    public EntityReference? RequiredComponentNodeId
    {
        get => GetAttributeValue<EntityReference?>("requiredcomponentnodeid");
        set => SetAttributeValue("requiredcomponentnodeid", value);
    }

    /// <summary>
    /// <para>Display Name: requiredcomponentobjectid</para>
    /// </summary>
    [AttributeLogicalName("requiredcomponentobjectid")]
    [DisplayName("requiredcomponentobjectid")]
    public Guid? RequiredComponentObjectId
    {
        get => GetAttributeValue<Guid?>("requiredcomponentobjectid");
        set => SetAttributeValue("requiredcomponentobjectid", value);
    }

    /// <summary>
    /// <para>Display Name: requiredcomponentparentid</para>
    /// </summary>
    [AttributeLogicalName("requiredcomponentparentid")]
    [DisplayName("requiredcomponentparentid")]
    public Guid? RequiredComponentParentId
    {
        get => GetAttributeValue<Guid?>("requiredcomponentparentid");
        set => SetAttributeValue("requiredcomponentparentid", value);
    }

    /// <summary>
    /// <para>Display Name: requiredcomponenttype</para>
    /// </summary>
    [AttributeLogicalName("requiredcomponenttype")]
    [DisplayName("requiredcomponenttype")]
    public componenttype? RequiredComponentType
    {
        get => this.GetOptionSetValue<componenttype>("requiredcomponenttype");
        set => this.SetOptionSetValue("requiredcomponenttype", value);
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

    [AttributeLogicalName("dependentcomponentnodeid")]
    [RelationshipSchemaName("dependencynode_descendent_dependency")]
    [RelationshipMetadata("ManyToOne", "dependentcomponentnodeid", "dependencynode", "dependencynodeid", "Referencing")]
    public DependencyNode dependencynode_descendent_dependency
    {
        get => GetRelatedEntity<DependencyNode>("dependencynode_descendent_dependency", null);
        set => SetRelatedEntity("dependencynode_descendent_dependency", null, value);
    }

    [AttributeLogicalName("requiredcomponentnodeid")]
    [RelationshipSchemaName("dependencynode_ancestor_dependency")]
    [RelationshipMetadata("ManyToOne", "requiredcomponentnodeid", "dependencynode", "dependencynodeid", "Referencing")]
    public DependencyNode dependencynode_ancestor_dependency
    {
        get => GetRelatedEntity<DependencyNode>("dependencynode_ancestor_dependency", null);
        set => SetRelatedEntity("dependencynode_ancestor_dependency", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the Dependency entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<Dependency, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the Dependency with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of Dependency to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved Dependency</returns>
    public static Dependency Retrieve(IOrganizationService service, Guid id, params Expression<Func<Dependency, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }
}