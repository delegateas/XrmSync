using Microsoft.Extensions.Configuration;
using XrmSync.Model;

namespace XrmSync;

internal interface ISyncOptionsBuilder
{
    XrmSyncOptions Build();
}