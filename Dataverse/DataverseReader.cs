using DG.XrmSync.Dataverse.Interfaces;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmSync.Dataverse;

public sealed class DataverseReader(ServiceClient serviceClient) : IDataverseReader
{
    public Entity Retrieve(string logicalName, Guid id, ColumnSet columnSet)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The provided ID is empty.", nameof(id));
        }

        return serviceClient.Retrieve(logicalName, id, columnSet);
    }

    public Entity RetrieveFirstMatch(QueryExpression query)
    {
        query.TopCount = 1;
        var entities = serviceClient.RetrieveMultiple(query).Entities;
        if (entities.Count == 0)
        {
            throw new Exception($"No entities of type '{query.EntityName}' found with the given query.\n{ConvertQueryToString(query)}");
        }
        return entities.First();
    }

    public bool Exists(string logicalName, Guid id)
    {
        var entity = serviceClient.Retrieve(logicalName, id, new ColumnSet(null));
        return entity.Id != Guid.Empty;
    }

    public Entity? RetrieveFirstOrDefault(QueryExpression query)
    {
        query.TopCount = 1;
        var entities = serviceClient.RetrieveMultiple(query).Entities;
        if (entities.Count == 0)
        {
            return null;
        }
        return entities.First();
    }

    public List<Entity> RetrieveMultiple(QueryExpression queryExpression)
    {
        // Define the fetch attributes.
        // Set the number of records per page to retrieve.
        const int fetchCount = 5000;
        // Initialize the page number.
        var pageNumber = 1;
        // Specify the current paging cookie. For retrieving the first page, 
        // pagingCookie should be null.
        string? pagingCookie = null;

        var result = new List<Entity>();

        while (true)
        {
            queryExpression.PageInfo = BuildPagingCookie(fetchCount, pageNumber, pagingCookie);
            RetrieveMultipleRequest fetchRequest = new RetrieveMultipleRequest
            {
                Query = queryExpression
            };

            var response = (RetrieveMultipleResponse)serviceClient.Execute(fetchRequest);

            var returnCollection = response.EntityCollection;
            // Casting entity collection to typed list
            result.AddRange(returnCollection.Entities.ToList());

            if (returnCollection.MoreRecords)
            {
                // Increment the page number to retrieve the next page.
                pageNumber++;

                // Set the paging cookie to the paging cookie returned from current results.                            
                pagingCookie = returnCollection.PagingCookie;
            }
            else
            {
                // If no more records in the result nodes, exit the loop.
                break;
            }
        }
        return result;
    }
    private static PagingInfo BuildPagingCookie(int fetchCount, int pageNumber, string? pagingCookie)
    {
        return new PagingInfo
        {
            Count = fetchCount,
            PageNumber = pageNumber,
            ReturnTotalRecordCount = true,
            PagingCookie = pagingCookie
        };
    }

    private string ConvertQueryToString(QueryExpression query)
    {
        try
        {
            var conversionRequest = new QueryExpressionToFetchXmlRequest
            {
                Query = query
            };
            var conversionResponse = (QueryExpressionToFetchXmlResponse)serviceClient.Execute(conversionRequest);
            var fetchXml = conversionResponse.FetchXml;
            return fetchXml;
        }
        catch (Exception)
        {
            return "Unable to convert query to fetchXML";
        }
    }
}
