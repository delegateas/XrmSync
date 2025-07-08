using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DG.XrmSync.AssemblyAnalyzer
{
    public class AnalysisException : Exception
    {
        public AnalysisException() : base()
        {
        }

        public AnalysisException(string? message) : base(message)
        {
        }

        public AnalysisException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
