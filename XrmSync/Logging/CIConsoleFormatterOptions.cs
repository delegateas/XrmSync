using Microsoft.Extensions.Logging.Console;

namespace XrmSync.Logging;

internal class CIConsoleFormatterOptions : SimpleConsoleFormatterOptions
{
	public bool CIMode { get; set; }

	public bool DryRun { get; set; }
}
