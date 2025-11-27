using XrmSync.Model;

namespace XrmSync.Analyzer
{
	public interface IAssemblyAnalyzer
	{
		AssemblyInfo AnalyzeAssembly(string dllPath, string prefix);
	}
}
