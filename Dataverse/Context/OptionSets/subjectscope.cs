using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum subjectscope
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("GlobalScope", 1033)]
    GlobalScope = 0,

    [EnumMember]
    [OptionSetMetadata("EnviornmentScope", 1033)]
    EnviornmentScope = 1,

    [EnumMember]
    [OptionSetMetadata("DevOnlyScope", 1033)]
    DevOnlyScope = 2,
}
