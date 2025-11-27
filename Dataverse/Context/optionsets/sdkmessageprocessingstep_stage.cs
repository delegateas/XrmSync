using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_stage
#pragma warning restore CS8981
{
	[EnumMember]
	[OptionSetMetadata("Initial Pre-operation (For internal use only)", 1033)]
	InitialPreoperationForinternaluseonly = 5,

	[EnumMember]
	[OptionSetMetadata("Pre-validation", 1033)]
	Prevalidation = 10,

	[EnumMember]
	[OptionSetMetadata("Internal Pre-operation Before External Plugins (For internal use only)", 1033)]
	InternalPreoperationBeforeExternalPluginsForinternaluseonly = 15,

	[EnumMember]
	[OptionSetMetadata("Pre-operation", 1033)]
	Preoperation = 20,

	[EnumMember]
	[OptionSetMetadata("Internal Pre-operation After External Plugins (For internal use only)", 1033)]
	InternalPreoperationAfterExternalPluginsForinternaluseonly = 25,

	[EnumMember]
	[OptionSetMetadata("Main Operation (For internal use only)", 1033)]
	MainOperationForinternaluseonly = 30,

	[EnumMember]
	[OptionSetMetadata("Internal Post-operation Before External Plugins (For internal use only)", 1033)]
	InternalPostoperationBeforeExternalPluginsForinternaluseonly = 35,

	[EnumMember]
	[OptionSetMetadata("Post-operation", 1033)]
	Postoperation = 40,

	[EnumMember]
	[OptionSetMetadata("Internal Post-operation After External Plugins (For internal use only)", 1033)]
	InternalPostoperationAfterExternalPluginsForinternaluseonly = 45,

	[EnumMember]
	[OptionSetMetadata("Post-operation (Deprecated)", 1033)]
	PostoperationDeprecated = 50,

	[EnumMember]
	[OptionSetMetadata("Final Post-operation (For internal use only)", 1033)]
	FinalPostoperationForinternaluseonly = 55,

	[EnumMember]
	[OptionSetMetadata("Pre-Commit stage fired before transaction commit (For internal use only)", 1033)]
	PreCommitstagefiredbeforetransactioncommitForinternaluseonly = 80,

	[EnumMember]
	[OptionSetMetadata("Post-Commit stage fired after transaction commit (For internal use only)", 1033)]
	PostCommitstagefiredaftertransactioncommitForinternaluseonly = 90,
}
