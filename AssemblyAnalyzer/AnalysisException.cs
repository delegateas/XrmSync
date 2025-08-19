﻿using XrmSync.Model.Exceptions;

namespace XrmSync.AssemblyAnalyzer
{
    public class AnalysisException : XrmSyncException
    {
        public AnalysisException()
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
