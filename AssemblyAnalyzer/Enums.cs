namespace DG.XrmPluginSync.AssemblyAnalyzer;

internal enum ExecutionMode
{
    Synchronous,
    Asynchronous
}

internal enum ExecutionStage
{
    PreValidation = 10,
    Pre = 20,
    Post = 40
}

internal enum ImageType
{
    PreImage = 0,
    PostImage = 1,
    Both = 2
}
