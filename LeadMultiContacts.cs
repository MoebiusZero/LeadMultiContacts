using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace LeadMultiContacts
{
    public class LeadMultiContacts : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  W
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    if (entity.Attributes.Contains("originatingleadid"))
                    {
                        //Get the GUID of the newly created Account
                        Guid accountID = new Guid(context.OutputParameters["id"].ToString());
                        var accountIDbyte = accountID.ToByteArray();
                        tracingService.Trace($"AccountID = {accountID}");

                        //Get the GUID of the lead Origin for the new enrolled customer
                        EntityReference originlead = (EntityReference)entity.Attributes["originatingleadid"];
                        var actualOwningUnit = service.Retrieve(originlead.LogicalName, originlead.Id, new ColumnSet(true));
                        var leadID = originlead.Id;
                        tracingService.Trace($"Origin Lead ID = {leadID}");                        

                        //Create the filter to look for any contacts that have the same origin lead 
                        ConditionExpression guidcondition = new ConditionExpression();
                        guidcondition.AttributeName = "originatingleadid";
                        guidcondition.Operator = ConditionOperator.Equal;
                        guidcondition.Values.Add(leadID);

                        FilterExpression filter = new FilterExpression();
                        filter.Conditions.Add(guidcondition);

                        QueryExpression query = new QueryExpression("contact");
                        query.ColumnSet.AddColumns("firstname", "lastname");
                        query.Criteria.AddFilter(filter);

                        //For all found contacts, assign them the newly created account 
                        EntityCollection results = service.RetrieveMultiple(query);
                        foreach (Entity entcontact in results.Entities)
                        {                            
                            Guid contactID = entcontact.Id;
                            var contactIDbyte = contactID.ToByteArray();
                            tracingService.Trace($"Found this contact {entcontact.Id}");

                            EntityReference parentcustomerid = new EntityReference("account", new Guid(accountIDbyte));
                            entcontact["parentcustomerid"] = parentcustomerid;

                            service.Update(entcontact);
                        }
                    }               
                }
                catch (Exception ex)
                {
                    tracingService.Trace("LeadMultiContacts: {0}", ex.ToString());
                    throw new InvalidPluginExecutionException("An error occurred in LeadMultiContacts.", ex);                  
                    throw;
                }
            }
        }
    }
}
