using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[DataContract]
#pragma warning disable CS8981
public enum solution_solutiontype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("None", 1033)]
    None = 0,

    [EnumMember]
    [OptionSetMetadata("Snapshot", 1033)]
    Snapshot = 1,

    [EnumMember]
    [OptionSetMetadata("Internal", 1033)]
    @Internal = 2,
}