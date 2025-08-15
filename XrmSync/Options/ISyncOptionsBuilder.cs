using Microsoft.Extensions.Configuration;
using XrmSync.Model;

namespace XrmSync.Options;

internal interface ISyncOptionsBuilder
{
    XrmSyncOptions Build();
}