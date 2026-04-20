using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum credentialsource
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("ClientSecret", 1033)]
    ClientSecret = 0,

    [EnumMember]
    [OptionSetMetadata("KeyVault", 1033)]
    KeyVault = 1,

    [EnumMember]
    [OptionSetMetadata("IsManaged", 1033)]
    IsManaged = 2,

    [EnumMember]
    [OptionSetMetadata("MicrosoftFirstPartyCertificate", 1033)]
    MicrosoftFirstPartyCertificate = 3,
}
