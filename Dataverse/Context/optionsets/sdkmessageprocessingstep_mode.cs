using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_mode
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Synchronous", 1033)]
    [OptionSetMetadata("Synkron", 1030)]
    Synchronous = 0,

    [EnumMember]
    [OptionSetMetadata("Asynchronous", 1033)]
    [OptionSetMetadata("Asynkron", 1030)]
    Asynchronous = 1,
}
