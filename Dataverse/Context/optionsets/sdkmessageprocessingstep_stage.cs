using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_stage
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Initial Pre-operation (For internal use only)", 1033)]
    [OptionSetMetadata("Starthandling (kun til intern brug)", 1030)]
    InitialPreoperationForinternaluseonly = 5,

    [EnumMember]
    [OptionSetMetadata("Pre-validation", 1033)]
    [OptionSetMetadata("Startvalidering", 1030)]
    Prevalidation = 10,

    [EnumMember]
    [OptionSetMetadata("Internal Pre-operation Before External Plugins (For internal use only)", 1033)]
    [OptionSetMetadata("Intern starthandling før eksterne plug-ins (kun til intern brug)", 1030)]
    InternalPreoperationBeforeExternalPluginsForinternaluseonly = 15,

    [EnumMember]
    [OptionSetMetadata("Pre-operation", 1033)]
    [OptionSetMetadata("Starthandling", 1030)]
    Preoperation = 20,

    [EnumMember]
    [OptionSetMetadata("Internal Pre-operation After External Plugins (For internal use only)", 1033)]
    [OptionSetMetadata("Intern starthandling efter eksterne plug-ins (kun til intern brug)", 1030)]
    InternalPreoperationAfterExternalPluginsForinternaluseonly = 25,

    [EnumMember]
    [OptionSetMetadata("Main Operation (For internal use only)", 1033)]
    [OptionSetMetadata("Hovedhandling (kun til intern brug)", 1030)]
    MainOperationForinternaluseonly = 30,

    [EnumMember]
    [OptionSetMetadata("Internal Post-operation Before External Plugins (For internal use only)", 1033)]
    [OptionSetMetadata("Intern efterfølgende handling før eksterne plug-ins (kun til intern brug)", 1030)]
    InternalPostoperationBeforeExternalPluginsForinternaluseonly = 35,

    [EnumMember]
    [OptionSetMetadata("Post-operation", 1033)]
    [OptionSetMetadata("Efterfølgende handling", 1030)]
    Postoperation = 40,

    [EnumMember]
    [OptionSetMetadata("Internal Post-operation After External Plugins (For internal use only)", 1033)]
    [OptionSetMetadata("Intern efterfølgende handling efter eksterne plug-ins (kun til intern brug)", 1030)]
    InternalPostoperationAfterExternalPluginsForinternaluseonly = 45,

    [EnumMember]
    [OptionSetMetadata("Post-operation (Deprecated)", 1033)]
    [OptionSetMetadata("Efterfølgende handling (frarådes)", 1030)]
    PostoperationDeprecated = 50,

    [EnumMember]
    [OptionSetMetadata("Final Post-operation (For internal use only)", 1033)]
    [OptionSetMetadata("Afsluttende efterfølgende handling (kun til intern brug)", 1030)]
    FinalPostoperationForinternaluseonly = 55,

    [EnumMember]
    [OptionSetMetadata("Pre-Commit stage fired before transaction commit (For internal use only)", 1033)]
    [OptionSetMetadata("Fase før bekræftelse blev udløst før bekræftelse af transaktion (kun til intern brug)", 1030)]
    PreCommitstagefiredbeforetransactioncommitForinternaluseonly = 80,

    [EnumMember]
    [OptionSetMetadata("Post-Commit stage fired after transaction commit (For internal use only)", 1033)]
    [OptionSetMetadata("Fase efter bekræftelse blev udløst efter bekræftelse af transaktion (kun til intern brug)", 1030)]
    PostCommitstagefiredaftertransactioncommitForinternaluseonly = 90,
}
