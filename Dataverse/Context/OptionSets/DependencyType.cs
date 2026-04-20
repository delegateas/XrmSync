using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum dependencytype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("None", 1033)]
    [OptionSetMetadata("Ingen", 1030)]
    None = 0,

    [EnumMember]
    [OptionSetMetadata("Solution Internal", 1033)]
    [OptionSetMetadata("Løsning internt", 1030)]
    SolutionInternal = 1,

    [EnumMember]
    [OptionSetMetadata("Published", 1033)]
    [OptionSetMetadata("Udgivet", 1030)]
    Published = 2,

    [EnumMember]
    [OptionSetMetadata("Unpublished", 1033)]
    [OptionSetMetadata("Ikke-udgivet", 1030)]
    Unpublished = 4,
}
