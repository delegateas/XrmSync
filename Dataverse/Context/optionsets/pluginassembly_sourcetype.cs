using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum pluginassembly_sourcetype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Database", 1033)]
    [OptionSetMetadata("Database", 1030)]
    Database = 0,

    [EnumMember]
    [OptionSetMetadata("Disk", 1033)]
    [OptionSetMetadata("Disk", 1030)]
    Disk = 1,

    [EnumMember]
    [OptionSetMetadata("Normal", 1033)]
    [OptionSetMetadata("Normal", 1030)]
    Normal = 2,

    [EnumMember]
    [OptionSetMetadata("AzureWebApp", 1033)]
    [OptionSetMetadata("AzureWebApp", 1030)]
    AzureWebApp = 3,

    [EnumMember]
    [OptionSetMetadata("File Store", 1033)]
    FileStore = 4,
}
