namespace XrmSync.Actions;

internal interface IAction
{
    Task<bool> RunAction(CancellationToken cancellationToken);
}

internal interface ISaveConfigAction
{
    Task<bool> SaveConfigAsync(string? filename, CancellationToken cancellationToken);
}
