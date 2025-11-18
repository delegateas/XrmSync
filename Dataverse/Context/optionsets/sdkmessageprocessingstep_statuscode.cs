using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_statuscode
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Enabled", 1033)]
    Enabled = 1,

    [EnumMember]
    [OptionSetMetadata("Disabled", 1033)]
    Disabled = 2,
}