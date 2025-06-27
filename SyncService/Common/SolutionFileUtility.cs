using DG.XrmPluginSync.Model;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DG.XrmPluginSync.SyncService.Common;

public enum ExecutionMode
{
    Synchronous,
    Asynchronous
}

public enum ExecutionStage
{
    PreValidation = 10,
    Pre = 20,
    Post = 40
}

public enum ImageType
{
    PreImage = 0,
    PostImage = 1,
    Both = 2
}

public class SolutionInformation
{
    public string SolutionName { get; set; }
    public bool IsManaged { get; set; }
}

internal static class SolutionFileUtility
{
    public static SolutionInformation GetSolutionInformationFromFile(string path)
    {
        using (var zipStream = new FileStream(path, FileMode.Open))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            return GetSolutionInformation(archive);
    }

    private static SolutionInformation GetSolutionInformation(ZipArchive archive)
    {
        var solutionEntry = archive.GetEntry("solution.xml");
        if (solutionEntry == null)
            throw new Exception("Invalid CRM package. solution.xml file not found");

        var solutionDoc = XDocument.Load(solutionEntry.Open());
        var solutionNameNode = solutionDoc.XPathSelectElement("/ImportExportXml/SolutionManifest/UniqueName");
        var managedNode = solutionDoc.XPathSelectElement("/ImportExportXml/SolutionManifest/Managed");

        if (solutionNameNode.IsEmpty || managedNode.IsEmpty)
            throw new Exception("Invalid CRM package. Solution name or managed setting not found in solution package.");


        return new SolutionInformation { SolutionName = solutionNameNode.Value, IsManaged = managedNode.Value == "1" };
    }
}

