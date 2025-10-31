using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum systemuser_outgoingemaildeliverymethod
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("None", 1033)]
    None = 0,

    [EnumMember]
    [OptionSetMetadata("Microsoft Dynamics 365 for Outlook", 1033)]
    MicrosoftDynamics365forOutlook = 1,

    [EnumMember]
    [OptionSetMetadata("Server-Side Synchronization or Email Router", 1033)]
    ServerSideSynchronizationorEmailRouter = 2,
}