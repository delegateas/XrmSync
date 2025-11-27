namespace XrmSync.Model.Webresource;

public record WebresourceDefinition(string Name, string DisplayName, WebresourceType Type, string Content) : EntityBase(Name);

