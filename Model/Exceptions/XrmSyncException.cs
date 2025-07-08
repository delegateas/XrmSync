using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DG.XrmSync.Model.Exceptions;

public class XrmSyncException : Exception
{
    public XrmSyncException() : base()
    {
    }

    public XrmSyncException(string? message) : base(message)
    {
    }

    public XrmSyncException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
