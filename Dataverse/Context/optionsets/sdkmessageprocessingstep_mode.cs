using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_mode
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Synchronous", 1033)]
    Synchronous = 0,

    [EnumMember]
    [OptionSetMetadata("Asynchronous", 1033)]
    Asynchronous = 1,
}