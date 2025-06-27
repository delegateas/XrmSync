using DG.XrmPluginSync.Dataverse;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmPluginSync.SyncService.Common;

public class Message(CrmDataHelper CrmDataHelper)
{
    public static string GetMessagePropertyName(string eventOperation)
    {
        switch (eventOperation)
        {
            case "Assign": return "Target";
            case "Create": return "id";
            case "Delete": return "Target";
            case "DeliverIncoming": return "emailid";
            case "DeliverPromote": return "emailid";
            case "Merge": return "Target";
            case "Route": return "Target";
            case "Send": return "emailid";
            case "SetState": return "entityMoniker";
            case "SetStateDynamicEntity": return "entityMoniker";
            case "Update": return "Target";
            default:
                return null;
        }
    }
    public Entity GetMessage(string eventOperation)
    {
        var query = new QueryExpression("sdkmessage");
        query.ColumnSet = new ColumnSet("sdkmessageid", "name");
        query.TopCount = 1;

        var filter = new FilterExpression();
        filter.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, eventOperation));
        query.Criteria = filter;

        return CrmDataHelper.RetrieveFirstOrDefault(query);
    }
    public Entity GetMessageFilter(string primaryObjectType, Guid sdkMessageId)
    {
        var query = new QueryExpression("sdkmessagefilter");
        query.ColumnSet = new ColumnSet("sdkmessagefilterid");

        var filter = new FilterExpression();
        filter.AddCondition(new ConditionExpression("sdkmessageid", ConditionOperator.Equal, sdkMessageId));
        query.Criteria = filter;

        if (!string.IsNullOrEmpty(primaryObjectType))
            filter.AddCondition(new ConditionExpression("primaryobjecttypecode", ConditionOperator.Equal, primaryObjectType));

        return CrmDataHelper.RetrieveFirstOrDefault(query);
    }
}
