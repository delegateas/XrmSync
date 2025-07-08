namespace XrmSync.AssemblyAnalyzer
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
