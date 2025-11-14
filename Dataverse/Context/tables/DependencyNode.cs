using Microsoft.Xrm.Sdk;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk.Client;

namespace XrmSync.Dataverse.Context;

/// <summary>
/// <para>The representation of a component dependency node in CRM.</para>
/// <para>Display Name: Dependency Node</para>
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[EntityLogicalName("dependencynode")]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DataContract]
#pragma warning disable CS8981 // Allows: Only lowercase characters
public partial class DependencyNode : ExtendedEntity
#pragma warning restore CS8981
{
    public const string EntityLogicalName = "dependencynode";
    public const int EntityTypeCode = 7106;

    public DependencyNode() : base(EntityLogicalName) { }
    public DependencyNode(Guid id) : base(EntityLogicalName, id) { }

    private string DebuggerDisplay => GetDebuggerDisplay("");

    [AttributeLogicalName("dependencynodeid")]
    public override Guid Id {
        get {
            return base.Id;
        }
        set {
            SetId("dependencynodeid", value);
        }
    }

    /// <summary>
    /// <para>Unique identifier of the user who created the solution</para>
    /// <para>Display Name: Base Solution</para>
    /// </summary>
    [AttributeLogicalName("basesolutionid")]
    [DisplayName("Base Solution")]
    public EntityReference? BaseSolutionId
    {
        get => GetAttributeValue<EntityReference?>("basesolutionid");
        set => SetAttributeValue("basesolutionid", value);
    }

    /// <summary>
    /// <para>The type code of the component.</para>
    /// <para>Display Name: Type Code</para>
    /// </summary>
    [AttributeLogicalName("componenttype")]
    [DisplayName("Type Code")]
    public componenttype? ComponentType
    {
        get => this.GetOptionSetValue<componenttype>("componenttype");
        set => this.SetOptionSetValue("componenttype", value);
    }

    /// <summary>
    /// <para>Display Name: Dependency Node Identifier</para>
    /// </summary>
    [AttributeLogicalName("dependencynodeid")]
    [DisplayName("Dependency Node Identifier")]
    public Guid DependencyNodeId
    {
        get => GetAttributeValue<Guid>("dependencynodeid");
        set => SetId("dependencynodeid", value);
    }

    /// <summary>
    /// <para>Introduced version for the component</para>
    /// <para>Display Name: Introduced Version</para>
    /// </summary>
    [AttributeLogicalName("introducedversion")]
    [DisplayName("Introduced Version")]
    public double? IntroducedVersion
    {
        get => GetAttributeValue<double?>("introducedversion");
        set => SetAttributeValue("introducedversion", value);
    }

    /// <summary>
    /// <para>Whether this component is shared by two solutions with the same publisher.</para>
    /// <para>Display Name: Shared Component</para>
    /// </summary>
    [AttributeLogicalName("issharedcomponent")]
    [DisplayName("Shared Component")]
    public bool? IsSharedComponent
    {
        get => GetAttributeValue<bool?>("issharedcomponent");
        set => SetAttributeValue("issharedcomponent", value);
    }

    /// <summary>
    /// <para>Unique identifier of the object with which the node is associated.</para>
    /// <para>Display Name: Regarding</para>
    /// </summary>
    [AttributeLogicalName("objectid")]
    [DisplayName("Regarding")]
    public Guid? ObjectId
    {
        get => GetAttributeValue<Guid?>("objectid");
        set => SetAttributeValue("objectid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the parent entity.</para>
    /// <para>Display Name: Parent Entity</para>
    /// </summary>
    [AttributeLogicalName("parentid")]
    [DisplayName("Parent Entity")]
    public Guid? ParentId
    {
        get => GetAttributeValue<Guid?>("parentid");
        set => SetAttributeValue("parentid", value);
    }

    /// <summary>
    /// <para>Unique identifier of the top solution.</para>
    /// <para>Display Name: Top Solution</para>
    /// </summary>
    [AttributeLogicalName("topsolutionid")]
    [DisplayName("Top Solution")]
    public EntityReference? TopSolutionId
    {
        get => GetAttributeValue<EntityReference?>("topsolutionid");
        set => SetAttributeValue("topsolutionid", value);
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

    [AttributeLogicalName("basesolutionid")]
    [RelationshipSchemaName("solution_base_dependencynode")]
    [RelationshipMetadata("ManyToOne", "basesolutionid", "solution", "solutionid", "Referencing")]
    public Solution solution_base_dependencynode
    {
        get => GetRelatedEntity<Solution>("solution_base_dependencynode", null);
        set => SetRelatedEntity("solution_base_dependencynode", null, value);
    }

    [AttributeLogicalName("topsolutionid")]
    [RelationshipSchemaName("solution_top_dependencynode")]
    [RelationshipMetadata("ManyToOne", "topsolutionid", "solution", "solutionid", "Referencing")]
    public Solution solution_top_dependencynode
    {
        get => GetRelatedEntity<Solution>("solution_top_dependencynode", null);
        set => SetRelatedEntity("solution_top_dependencynode", null, value);
    }

    [RelationshipSchemaName("dependencynode_descendent_dependency")]
    [RelationshipMetadata("OneToMany", "dependencynodeid", "dependency", "dependentcomponentnodeid", "Referenced")]
    public IEnumerable<Dependency> dependencynode_descendent_dependency
    {
        get => GetRelatedEntities<Dependency>("dependencynode_descendent_dependency", null);
        set => SetRelatedEntities("dependencynode_descendent_dependency", null, value);
    }

    [RelationshipSchemaName("dependencynode_ancestor_dependency")]
    [RelationshipMetadata("OneToMany", "dependencynodeid", "dependency", "requiredcomponentnodeid", "Referenced")]
    public IEnumerable<Dependency> dependencynode_ancestor_dependency
    {
        get => GetRelatedEntities<Dependency>("dependencynode_ancestor_dependency", null);
        set => SetRelatedEntities("dependencynode_ancestor_dependency", null, value);
    }

    /// <summary>
    /// Gets the logical column name for a property on the DependencyNode entity, using the AttributeLogicalNameAttribute if present.
    /// </summary>
    /// <param name="column">Expression to pick the column</param>
    /// <returns>Name of column</returns>
    /// <exception cref="ArgumentNullException">If no expression is provided</exception>
    /// <exception cref="ArgumentException">If the expression is not x => x.column</exception>
    public static string GetColumnName(Expression<Func<DependencyNode, object>> column)
    {
        return TableAttributeHelpers.GetColumnName(column);
    }

    /// <summary>
    /// Retrieves the DependencyNode with the specified columns.
    /// </summary>
    /// <param name="service">Organization service</param>
    /// <param name="id">Id of DependencyNode to retrieve</param>
    /// <param name="columns">Expressions that specify columns to retrieve</param>
    /// <returns>The retrieved DependencyNode</returns>
    public static DependencyNode Retrieve(IOrganizationService service, Guid id, params Expression<Func<DependencyNode, object>>[] columns)
    {
        return service.Retrieve(id, columns);
    }
}