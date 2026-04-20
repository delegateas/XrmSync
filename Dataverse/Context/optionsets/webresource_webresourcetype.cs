using System.Runtime.Serialization;

namespace XrmSync.Dataverse.Context;

[System.CodeDom.Compiler.GeneratedCode("DataverseProxyGenerator", "4.0.0.25")]
[DataContract]
#pragma warning disable CS8981
public enum webresource_webresourcetype
#pragma warning restore CS8981
{
    [EnumMember]
    [OptionSetMetadata("Webpage (HTML)", 1033)]
    [OptionSetMetadata("Webside (HTML)", 1030)]
    WebpageHTML = 1,

    [EnumMember]
    [OptionSetMetadata("Style Sheet (CSS)", 1033)]
    [OptionSetMetadata("Typografiark (CSS)", 1030)]
    StyleSheetCSS = 2,

    [EnumMember]
    [OptionSetMetadata("Script (JScript)", 1033)]
    [OptionSetMetadata("Script (JScript)", 1030)]
    ScriptJScript = 3,

    [EnumMember]
    [OptionSetMetadata("Data (XML)", 1033)]
    [OptionSetMetadata("Data (XML)", 1030)]
    DataXML = 4,

    [EnumMember]
    [OptionSetMetadata("PNG format", 1033)]
    [OptionSetMetadata("PNG-format", 1030)]
    PNGformat = 5,

    [EnumMember]
    [OptionSetMetadata("JPG format", 1033)]
    [OptionSetMetadata("JPG-format", 1030)]
    JPGformat = 6,

    [EnumMember]
    [OptionSetMetadata("GIF format", 1033)]
    [OptionSetMetadata("GIF-format", 1030)]
    GIFformat = 7,

    [EnumMember]
    [OptionSetMetadata("Silverlight (XAP)", 1033)]
    [OptionSetMetadata("Silverlight (XAP)", 1030)]
    SilverlightXAP = 8,

    [EnumMember]
    [OptionSetMetadata("Style Sheet (XSL)", 1033)]
    [OptionSetMetadata("Typografiark (XSL)", 1030)]
    StyleSheetXSL = 9,

    [EnumMember]
    [OptionSetMetadata("ICO format", 1033)]
    [OptionSetMetadata("ICO-format", 1030)]
    ICOformat = 10,

    [EnumMember]
    [OptionSetMetadata("Vector format (SVG)", 1033)]
    [OptionSetMetadata("Vektorformat (SVG)", 1030)]
    VectorformatSVG = 11,

    [EnumMember]
    [OptionSetMetadata("String (RESX)", 1033)]
    [OptionSetMetadata("Streng (RESX)", 1030)]
    StringRESX = 12,
}
