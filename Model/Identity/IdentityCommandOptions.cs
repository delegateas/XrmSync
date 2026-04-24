namespace XrmSync.Model.Identity;

public record IdentityCommandOptions(IdentityOperation Operation, string AssemblyPath, string SolutionName, string ClientId, string TenantId)
{
	public static IdentityCommandOptions Empty => new(IdentityOperation.Remove, string.Empty, string.Empty, string.Empty, string.Empty);
}

