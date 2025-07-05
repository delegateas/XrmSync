namespace DG.XrmPluginSync.Model;

public enum ExecutionMode
{
    Synchronous,
    Asynchronous
}

public enum ExecutionStage
{
    PreValidation = 10,
    PreOperation = 20,
    PostOperation = 40
}

public enum ImageType
{
    PreImage = 0,
    PostImage = 1,
    Both = 2
}
