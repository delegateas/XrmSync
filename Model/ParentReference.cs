namespace XrmSync.Model;

public record ParentReference<TEntity, TParent>(TEntity Entity, TParent Parent)
	where TEntity : EntityBase
	where TParent : EntityBase;
