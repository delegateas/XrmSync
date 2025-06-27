namespace DG.XrmPluginSync.SyncService.Models.Requests;

public interface IRequest
{
    /// <summary>
    /// Get the name of the request
    /// </summary>
    /// <returns>Name of the request</returns>
    string GetName();
    
    /// <summary>
    /// Get a list of the arguments provided
    /// </summary>
    /// <returns>KeyValuePair list of arguments</returns>
    IList<(string key, string value)> GetArguments();
    
    /// <summary>
    /// Validates request and cast exception if invalid
    /// </summary>
    void Validate();
}
