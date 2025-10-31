using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum systemuser_emailrouteraccessapproval
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Empty", 1033)]
    Empty = 0,

    [EnumMember]
    [OptionSetMetadata("Approved", 1033)]
    Approved = 1,

    [EnumMember]
    [OptionSetMetadata("Pending Approval", 1033)]
    PendingApproval = 2,

    [EnumMember]
    [OptionSetMetadata("Rejected", 1033)]
    Rejected = 3,
}