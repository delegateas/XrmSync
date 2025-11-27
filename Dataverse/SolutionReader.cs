using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

internal class SolutionReader(IDataverseReader reader) : ISolutionReader
{
	public string ConnectedHost => reader.ConnectedHost;

	public (Guid SolutionId, string Prefix) RetrieveSolution(string uniqueName)
	{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
		var solution = (
			from s in reader.Solutions
			join p in reader.Publishers on s.PublisherId.Id equals p.PublisherId
			where s.UniqueName == uniqueName
			select new
			{
				s.Id,
				p.CustomizationPrefix
			}).FirstOrDefault()
			?? throw new XrmSyncException($"No solution with unique name {uniqueName} found");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

		return (
			solution.Id,
			solution.CustomizationPrefix
		);
	}
}
