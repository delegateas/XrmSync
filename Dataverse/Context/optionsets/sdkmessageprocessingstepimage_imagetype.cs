using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstepimage_imagetype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("PreImage", 1033)]
    PreImage = 0,

    [EnumMember]
    [OptionSetMetadata("PostImage", 1033)]
    PostImage = 1,

    [EnumMember]
    [OptionSetMetadata("Both", 1033)]
    Both = 2,
}