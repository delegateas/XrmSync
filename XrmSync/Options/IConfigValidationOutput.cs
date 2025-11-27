namespace XrmSync.Options;

internal interface IConfigValidationOutput
{
	Task OutputValidationResult(CancellationToken cancellationToken = default);
	Task OutputConfigList(CancellationToken cancellationToken = default);
}
