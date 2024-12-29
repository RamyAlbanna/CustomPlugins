using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

public class AccountsUpdate : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        if (context.MessageName != "Update")
            return;

        Entity accountRecord = (Entity)context.InputParameters["Target"];

        if (accountRecord.LogicalName != "account") return;

        EntityReference newOwner = (EntityReference)accountRecord["ownerid"];
        QueryExpression query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("contactid", "ownerid", "statecode"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("parentcustomerid", ConditionOperator.Equal, accountRecord.Id),
                    new ConditionExpression("statecode", ConditionOperator.Equal, 0)
                }
            }
        };

        try
        {
            EntityCollection contacts = service.RetrieveMultiple(query);
            foreach (var contact in contacts.Entities)
            {
                Entity contactToUpdate = new Entity("contact", contact.Id);
                contactToUpdate["ownerid"] = newOwner;
                service.Update(contactToUpdate);
            }
        }
        catch (Exception ex)
        {
            tracingService.Trace("Error occurred: " + ex.Message);
            throw;
        }
    }
}