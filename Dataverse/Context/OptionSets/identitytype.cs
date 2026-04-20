using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum identitytype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("App Registeration", 1033)]
    AppRegisteration = 0,

    [EnumMember]
    [OptionSetMetadata("AgentId", 1033)]
    AgentId = 1,

    [EnumMember]
    [OptionSetMetadata("AgentIdentityBlueprint", 1033)]
    AgentIdentityBlueprint = 2,

    [EnumMember]
    [OptionSetMetadata("AgentUser", 1033)]
    AgentUser = 3,
}
