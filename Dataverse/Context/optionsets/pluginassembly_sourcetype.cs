using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[DataContract]
#pragma warning disable CS8981
public enum pluginassembly_sourcetype
#pragma warning restore CS8981
{
	[EnumMember]
	[OptionSetMetadata("Database", 1033)]
	Database = 0,

	[EnumMember]
	[OptionSetMetadata("Disk", 1033)]
	Disk = 1,

	[EnumMember]
	[OptionSetMetadata("Normal", 1033)]
	Normal = 2,

	[EnumMember]
	[OptionSetMetadata("AzureWebApp", 1033)]
	AzureWebApp = 3,

	[EnumMember]
	[OptionSetMetadata("File Store", 1033)]
	FileStore = 4,
}
