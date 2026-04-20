using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_statecode
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Enabled", 1033)]
    [OptionSetMetadata("Aktiveret", 1030)]
    Enabled = 0,

    [EnumMember]
    [OptionSetMetadata("Disabled", 1033)]
    [OptionSetMetadata("Deaktiveret", 1030)]
    Disabled = 1,
}
