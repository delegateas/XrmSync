using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum dependencytype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("None", 1033)]
    None = 0,

    [EnumMember]
    [OptionSetMetadata("Solution Internal", 1033)]
    SolutionInternal = 1,

    [EnumMember]
    [OptionSetMetadata("Published", 1033)]
    Published = 2,

    [EnumMember]
    [OptionSetMetadata("Unpublished", 1033)]
    Unpublished = 4,
}