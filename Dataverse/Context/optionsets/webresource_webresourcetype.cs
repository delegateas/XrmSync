using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.19")]
[DataContract]
#pragma warning disable CS8981
public enum webresource_webresourcetype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Webpage (HTML)", 1033)]
    WebpageHTML = 1,

    [EnumMember]
    [OptionSetMetadata("Style Sheet (CSS)", 1033)]
    StyleSheetCSS = 2,

    [EnumMember]
    [OptionSetMetadata("Script (JScript)", 1033)]
    ScriptJScript = 3,

    [EnumMember]
    [OptionSetMetadata("Data (XML)", 1033)]
    DataXML = 4,

    [EnumMember]
    [OptionSetMetadata("PNG format", 1033)]
    PNGformat = 5,

    [EnumMember]
    [OptionSetMetadata("JPG format", 1033)]
    JPGformat = 6,

    [EnumMember]
    [OptionSetMetadata("GIF format", 1033)]
    GIFformat = 7,

    [EnumMember]
    [OptionSetMetadata("Silverlight (XAP)", 1033)]
    SilverlightXAP = 8,

    [EnumMember]
    [OptionSetMetadata("Style Sheet (XSL)", 1033)]
    StyleSheetXSL = 9,

    [EnumMember]
    [OptionSetMetadata("ICO format", 1033)]
    ICOformat = 10,

    [EnumMember]
    [OptionSetMetadata("Vector format (SVG)", 1033)]
    VectorformatSVG = 11,

    [EnumMember]
    [OptionSetMetadata("String (RESX)", 1033)]
    StringRESX = 12,
}