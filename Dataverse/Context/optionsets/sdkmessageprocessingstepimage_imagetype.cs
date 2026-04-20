using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstepimage_imagetype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("PreImage", 1033)]
    [OptionSetMetadata("PreImage", 1030)]
    PreImage = 0,

    [EnumMember]
    [OptionSetMetadata("PostImage", 1033)]
    [OptionSetMetadata("PostImage", 1030)]
    PostImage = 1,

    [EnumMember]
    [OptionSetMetadata("Both", 1033)]
    [OptionSetMetadata("Begge", 1030)]
    Both = 2,
}
