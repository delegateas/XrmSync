using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[DataContract]
#pragma warning disable CS8981
public enum customapi_allowedcustomprocessingsteptype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("None", 1033)]
    None = 0,

    [EnumMember]
    [OptionSetMetadata("Async Only", 1033)]
    AsyncOnly = 1,

    [EnumMember]
    [OptionSetMetadata("Sync and Async", 1033)]
    SyncandAsync = 2,
}