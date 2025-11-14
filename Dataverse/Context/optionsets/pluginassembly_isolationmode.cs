using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[DataContract]
#pragma warning disable CS8981
public enum pluginassembly_isolationmode
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("None", 1033)]
    None = 1,

    [EnumMember]
    [OptionSetMetadata("Sandbox", 1033)]
    Sandbox = 2,

    [EnumMember]
    [OptionSetMetadata("External", 1033)]
    External = 3,
}