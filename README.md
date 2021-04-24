# LeadMultiContacts
Normally CRM Dynamics 365 does not have the ability for users to assign more than one Contact to a Lead. 
Even if you do use a subgrid related to the Contacts entity, you still do not have a way to migrate all the contacts to a Account once you qualify a Lead.

Something about the way Microsoft envisions how people should use the Lead entity vs you the consumner...

I believe there are solutions sold online that create this very same functionality, but if you have the know-how how to implement this plugin and maintain it, 
then go right ahead and use this. 


## Prerequisites
This plugin requires the following things setup in your CRM Dynamics environment 
- A Subgrid in your Leads Entity that has a relationship with the Contacts Entity, you must be able to create new contacts through that subgrid of course
- In your Contacts entity, the field "Original Lead" (sorry, I do not know the actual english field name for this, I use the dutch version of CRM Dynamics) 
  must be unused. This is because once you create a new contact through the Lead entity subgrid, this field will be automatically populated.
  If you already use this field for other reasons, find a way to create a new field and populate that one with the Lead record on new contact creation.
- In your Accounts entity, the field "Original Lead" must be unused. If you use this for other reasons as well, find a another way to create a field and populate that 
  with the lead entity on qualification.
  
## What does this plugin do exactly?
This plugin triggers on the Create event in the Accounts entity.
The reason is simple, when you qualify a Lead, it CREATES a new account and everything gets fired off like the one-to-many relationship that migrates fields in 
Leads to Accounts.
That is the perfect point to start migrating the contacts as well.

Step by step, it then does the following:
* Get the GUID of the newly created Account
* Check if the "Original Lead" Field is populated, if yes get the GUID, if not do nothing
* RetrieveMultiple in the Contacts entity for any contacts with the same GUID filled in the "Original Lead" field
* For each of the contacts found populate the parentaccountid field with the GUID of the newly created account in Contacts

Upon opening the Account, you'll see that the Contacts subgrid will be populated with the same contacts created in the Leads entity


