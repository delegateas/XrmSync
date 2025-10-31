using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum customapifieldtype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Boolean", 1033)]
    Boolean = 0,

    [EnumMember]
    [OptionSetMetadata("DateTime", 1033)]
    DateTime = 1,

    [EnumMember]
    [OptionSetMetadata("Decimal", 1033)]
    @Decimal = 2,

    [EnumMember]
    [OptionSetMetadata("Entity", 1033)]
    Entity = 3,

    [EnumMember]
    [OptionSetMetadata("EntityCollection", 1033)]
    EntityCollection = 4,

    [EnumMember]
    [OptionSetMetadata("EntityReference", 1033)]
    EntityReference = 5,

    [EnumMember]
    [OptionSetMetadata("Float", 1033)]
    @Float = 6,

    [EnumMember]
    [OptionSetMetadata("Integer", 1033)]
    Integer = 7,

    [EnumMember]
    [OptionSetMetadata("Money", 1033)]
    Money = 8,

    [EnumMember]
    [OptionSetMetadata("Picklist", 1033)]
    Picklist = 9,

    [EnumMember]
    [OptionSetMetadata("String", 1033)]
    @String = 10,

    [EnumMember]
    [OptionSetMetadata("StringArray", 1033)]
    StringArray = 11,

    [EnumMember]
    [OptionSetMetadata("Guid", 1033)]
    Guid = 12,
}