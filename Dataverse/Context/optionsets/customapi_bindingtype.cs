using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum customapi_bindingtype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Global", 1033)]
    Global = 0,

    [EnumMember]
    [OptionSetMetadata("Entity", 1033)]
    Entity = 1,

    [EnumMember]
    [OptionSetMetadata("Entity Collection", 1033)]
    EntityCollection = 2,
}