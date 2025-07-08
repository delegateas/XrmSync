using System;
using System.Diagnostics;

namespace XrmSync.Model;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record EntityBase
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    private string DebuggerDisplay => GetDebuggerDisplay();

    protected string GetDebuggerDisplay()
    {
        string display = GetType().Name;
        if (!string.IsNullOrEmpty(Name))
        {
            display += string.Format(" ({0})", Name);
        }

        if (Id != Guid.Empty)
        {
            display += string.Format(" [{0}]", Id);
        }

        return display;
    }
}
