namespace XrmSync.Model.Identity;

/// <summary>
/// The currently registered state of a managed identity record in Dataverse.
/// </summary>
public record ManagedIdentityInfo(Guid Id, string? Name, Guid? ApplicationId, Guid? TenantId);
