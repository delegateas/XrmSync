using XrmSync.Model;

namespace XrmSync.AssemblyAnalyzer
{
    public interface IAssemblyAnalyzer
    {
        AssemblyInfo AnalyzeAssembly(string dllPath);
    }
}