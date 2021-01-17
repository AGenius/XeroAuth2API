# XeroAuth2API
Provides a simple way to get your Xero oAuth2 tokens and wrap the Net-Standard API calls into easier to use Synchronous calls.

The core oAuth2 process is wrapped up in a nice self contained class to do all the work for you.

Just provide the required Xero Client ID, Callback URL, scope and original saved token (if you have one) and the wrapper will launch a web browser if the token has expired or is not yet generated.

It uses a wrapper around the System.Net.HttListener class that waits for the call back response once the user has authenticated and will then exchange the provided authentication code for the access tokens.

The oAuth2 wrapper will also handle the refresh of the access tokens if required.

The oAuth2 process is wrapped inside a API Wrapper designed to simplify the API calls to Xero and convert all the Async calls to sync calls. there are also events to provide feedback to the calling application

You will need the XeroAPI/Net-Standard Nuget package installed
[![NuGet.org](https://img.shields.io/badge/NuGet.org-Xero.NetStandard.OAuth2-brightgreen?style=plastic&logo=appveyor)](https://www.nuget.org/packages/Xero.NetStandard.OAuth2/)

Visit https://github.com/XeroAPI/Xero-NetStandard

### Limitations
Presently the project is being worked on so dont expect it to work perfectly.
The API only handlesexposes parts of the Xero AccountApi but more will follow

### Create a Xero App first 
Follow these steps to create your Xero app to allow access to your Xero tenant(s)

* Create a free Xero user account (if you don't have one)
* Use this URL for beta access to oAuth2 [https://developer.xero.com/myapps](https://developer.xero.com/myapps)
* Click "New app" link
* Enter your App name, company url, privacy policy url, and redirect URI (this is your callback url - localhost, etc) I would suggest http://localhost:8888/callback/
* choose PKCE
* Agree to terms and condition and click "Create App".
* Copy your client id and client secret and save for use later.
* Click the "Save" button. 

## Inspired By
Code for the listener was found here
http://www.gabescode.com/dotnet/2018/11/01/basic-HttpListener-web-service.html 
I built something similar in the past using the TCP Listener and started to look at midifing this to do the listening but found it didnt work as I hoped. Subsequently an example provided by Xero pointed to this listener and I managed to make it work.


## Getting Started
Example of how to use this API Wrapper

```c#
XeroAuth2API.API xeroAPI = new XeroAuth2API.API(XeroClientID, XeroCallbackUri, XeroScope, XeroState, savedAccessToken);
```
By default the API will select the first tenant in the list , if you only have 1 authorized then all is fine otherwise ensure you select it (either allow your user to choose or select it yourself

```c#
// Find the Demo Company TenantID
XeroAuth2API.Model.Tenant Tenant = xeroAPI.Tenants.Find(x => x.TenantName.ToLower() == "demo company (uk)");
xeroAPI.TenantID = Tenant.TenantId.ToString(); // Ensure its selected
```

To get started there are a few main classes.

* The LocalHttpListener class. This is a wrapper for the System.Net.HttpListener and is key to getting the code from Xero
* oAuth2. This is the class that handles the Auth code flow and sets up the Listener. This is not directly exposed and will be used by the API class to do the work.
* API. The main wrapper that keeps the access simple.

To get started you will just need two things to make calls to the Accounting Api. These are obtained by the oAuth2 Process
* xero-tenant-id
* accessToken

## Examples

To request data from Xero its a simple as

```c#
var accounts = xeroAPI.Accounts(); 
```
or fetch a single item.

```c#
var singleAccount = xeroAPI.Account(accounts[5].AccountID.Value);
```
Invoices
```c#
var invoices = xeroAPI.Invoices();
var singleinvcoice = xeroAPI.Invoice(invoices[5].InvoiceID.Value);
```

You can even create a record using a single call

```c#
var createdInv = xeroAPI.CreateInvoice(invoiceRecord);
```

This requires an invoice object to be created and can be accomplished via building the object first

```c#
// Create a test invoice
var xeroContact = new Xero.NetStandard.OAuth2.Model.Accounting.Contact
{
    Name = "Client Name",
    FirstName = "Client",
    LastName = "Name",
    EmailAddress = "emailaddress@na.com",
    IsCustomer = true,

    AccountNumber = $"NEW-ACC",
    // xeroContact.Website = "http://google.com"; // Currently the Zero API has this read only!!

    Addresses = new List<Xero.NetStandard.OAuth2.Model.Accounting.Address>()
};

var address = new Xero.NetStandard.OAuth2.Model.Accounting.Address
{
    AddressType = Xero.NetStandard.OAuth2.Model.Accounting.Address.AddressTypeEnum.STREET,
    AddressLine1 = "Address1_Line1",
    AddressLine2 = "Address1_Line2",
    AddressLine3 = "Address1_Line3",
    City = "Address1_City",
    Region = "Address1_County",
    PostalCode = "Address1_PostalCode",
    Country = "Address1_Country"
};

xeroContact.Addresses.Add(address);

xeroContact.Phones = new List<Xero.NetStandard.OAuth2.Model.Accounting.Phone>();

var phone = new Xero.NetStandard.OAuth2.Model.Accounting.Phone();
phone.PhoneType = Xero.NetStandard.OAuth2.Model.Accounting.Phone.PhoneTypeEnum.DEFAULT;
phone.PhoneNumber = "Telephone1";

xeroContact.Phones.Add(phone);

var fax = new Xero.NetStandard.OAuth2.Model.Accounting.Phone();
fax.PhoneType = Xero.NetStandard.OAuth2.Model.Accounting.Phone.PhoneTypeEnum.FAX;
fax.PhoneNumber = "Fax";
xeroContact.Phones.Add(fax);

var mobile = new Xero.NetStandard.OAuth2.Model.Accounting.Phone();
mobile.PhoneType = Xero.NetStandard.OAuth2.Model.Accounting.Phone.PhoneTypeEnum.MOBILE;
mobile.PhoneNumber = "MobilePhone";
xeroContact.Phones.Add(mobile);

// Build the Invoice Body
var invoiceRecord = new Xero.NetStandard.OAuth2.Model.Accounting.Invoice();
invoiceRecord.Contact = xeroContact;
invoiceRecord.Date = DateTime.Now;
invoiceRecord.DueDate = DateTime.Now.AddDays(30);
invoiceRecord.Status = Xero.NetStandard.OAuth2.Model.Accounting.Invoice.StatusEnum.DRAFT;
invoiceRecord.LineAmountTypes = Xero.NetStandard.OAuth2.Model.Accounting.LineAmountTypes.Exclusive;

invoiceRecord.Type = Xero.NetStandard.OAuth2.Model.Accounting.Invoice.TypeEnum.ACCREC;
invoiceRecord.Reference = $"oAuth2/Testing";
invoiceRecord.LineItems = new List<Xero.NetStandard.OAuth2.Model.Accounting.LineItem>();

// Line Item 1
// Create the Tracking Item
var tracking = new List<Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking>();
tracking.Add(new Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking { Name = "Region", Option = "Eastside" });

Xero.NetStandard.OAuth2.Model.Accounting.LineItem lineItem = new Xero.NetStandard.OAuth2.Model.Accounting.LineItem
{
    Description = $"Product Item 1{Environment.NewLine}Additional Description text",
    Quantity = 1,
    UnitAmount = 123m,
    LineAmount = 123m,
    TaxAmount = 123m * .20m,
    AccountCode = "200",
    Tracking = tracking
};

invoiceRecord.LineItems.Add(lineItem); // Add the line item to the invoice object

// Line Item 2
// Create the Tracking Item
tracking = new List<Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking>();
tracking.Add(new Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking { Name = "Region", Option = "South" });

Xero.NetStandard.OAuth2.Model.Accounting.LineItem lineItem2 = new Xero.NetStandard.OAuth2.Model.Accounting.LineItem
{
    Description = $"Product Item 2{Environment.NewLine}Additional Description text2",
    Quantity = 2,
    UnitAmount = 456m,
    LineAmount = 456m * 2,
    TaxAmount = (456m * 2) * .20m,
    AccountCode = "200",
    Tracking = tracking
};

invoiceRecord.LineItems.Add(lineItem2); // Add the line item to the invoice object             

if (invoiceRecord.ValidationErrors == null || invoiceRecord.ValidationErrors.Count == 0)
{
    var createdInv = xeroAPI.CreateInvoice(invoiceRecord);
    if (createdInv.InvoiceID != Guid.Empty)
    {
        Debug.WriteLine("Created New Invoice");
    }
}

```            





