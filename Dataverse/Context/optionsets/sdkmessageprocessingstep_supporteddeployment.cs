using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.21")]
[DataContract]
#pragma warning disable CS8981
public enum sdkmessageprocessingstep_supporteddeployment
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Server Only", 1033)]
    ServerOnly = 0,

    [EnumMember]
    [OptionSetMetadata("Microsoft Dynamics 365 Client for Outlook Only", 1033)]
    MicrosoftDynamics365ClientforOutlookOnly = 1,

    [EnumMember]
    [OptionSetMetadata("Both", 1033)]
    Both = 2,
}