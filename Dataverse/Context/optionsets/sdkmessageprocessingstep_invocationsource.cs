using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_invocationsource
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Internal", 1033)]
    [OptionSetMetadata("Intern", 1030)]
    @Internal = -1,

    [EnumMember]
    [OptionSetMetadata("Parent", 1033)]
    [OptionSetMetadata("Overordnet", 1030)]
    Parent = 0,

    [EnumMember]
    [OptionSetMetadata("Child", 1033)]
    [OptionSetMetadata("Underordnet", 1030)]
    Child = 1,
}
