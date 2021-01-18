﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    public class API
    {
        oAuth2 _authClient = null;
        Xero.NetStandard.OAuth2.Api.AccountingApi xeroAPI_A = new Xero.NetStandard.OAuth2.Api.AccountingApi();
        public string TenantID { get; set; } // The Tenant ID to use for API calls
        /// <summary>
        /// this will be the Access Token used for the API calls. Read Only
        /// </summary>
        public string AccessToken { get; private set; }
        /// <summary>
        /// If provided, the API setup will try and match the name with the correct tenant otherwise the first tenant will be selected
        /// </summary>
        public string TenantName { get; set; }
        /// <summary>
        /// Provide access to the Full Xero Token object
        /// </summary>
        public Model.XeroOAuthToken XeroToken { get { return _authClient.XeroToken; } }
        /// <summary>
        /// Provides access to the available tenants authorized
        /// </summary>
        public List<Model.Tenant> Tenants { get { return XeroToken.Tenants; } }
        /// <summary>
        /// Provide access to the currently selected Tenant , selected by TenantID
        /// </summary>
        public Model.Tenant SelectedTenant
        {
            get
            {
                if (!string.IsNullOrEmpty(TenantID))
                {
                    return XeroToken.Tenants.Find(x => x.TenantId.ToString().ToLower() == TenantID);
                }
                return null;
            }
        }
        public bool? AutoSelectTenant { get; set; }
        public API()
        {
            _authClient = new oAuth2();
            AutoSelectTenant = true;
        }
        public API(string clientID, Uri callBackUrl, string scope, string state, Model.XeroOAuthToken token)
        {
            _authClient = new oAuth2();

            _authClient.XeroClientID = clientID;
            _authClient.XeroCallbackUri = callBackUrl;
            _authClient.XeroScope = scope;
            _authClient.XeroState = state;
            _authClient.StatusUpdate += StatusUpdateMessage;
            _authClient.XeroToken = token;

            var task = Task.Run(() => _authClient.InitializeoAuth2(token));
            task.Wait();
            _authClient.XeroToken = task.Result; // Update the token incase it was refreshed or new
            oAuth2.XeroAuth2EventArgs args = new oAuth2.XeroAuth2EventArgs() { MessageText = "Ready", XeroTokenData = token, Status = oAuth2.XeroEventStatus.Success };
            onStatusMessageReceived(args);
            if ((AutoSelectTenant.HasValue && AutoSelectTenant.Value == true) || !AutoSelectTenant.HasValue)
            {
                TenantID = _authClient.XeroToken.Tenants[0].TenantId.ToString();
            }

            AccessToken = _authClient.XeroToken.AccessToken;

        }
        public void InitializeAPI(Model.XeroOAuthToken token)
        {
            var task = Task.Run(() => _authClient.InitializeoAuth2(token));
            task.Wait();

            oAuth2.XeroAuth2EventArgs args = new oAuth2.XeroAuth2EventArgs() { MessageText = "Ready", XeroTokenData = token, Status = oAuth2.XeroEventStatus.Success };
            onStatusMessageReceived(args);
        }



        #region Accounts
        /// <summary>
        /// Retrieve the full chart of accounts or filtered list
        /// </summary>
        /// <param name="filter">string containing the filter to apply - e.g. "Class = \"REVENUE\" "</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of Accounts</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Account> Accounts(string filter = null, DateTime? ModifiedSince = null, string order = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetAccountsAsync(AccessToken, TenantID, ModifiedSince, filter, order));
            task.Wait();
            if (task.Result._Accounts.Count > 0)
            {
                return task.Result._Accounts;
            }
            return null;
        }
        /// <summary>
        /// Retreive a single Account record 
        /// </summary>
        /// <param name="accountID">Unique identifier for retrieving single object</param>
        /// <returns>Account</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Account Account(Guid accountID)
        {
            var task = Task.Run(() => xeroAPI_A.GetAccountAsync(AccessToken, TenantID, accountID));
            task.Wait();
            if (task.Result._Accounts.Count > 0)
            {
                return task.Result._Accounts[0];
            }
            return null;
        }
        /// <summary>
        /// Create an Account, Currently only supports single accounts
        /// </summary>
        /// <param name="record">Account object</param>
        public Xero.NetStandard.OAuth2.Model.Accounting.Account CreateAccount(Xero.NetStandard.OAuth2.Model.Accounting.Account record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Account");
            }

            var task = Task.Run(() => xeroAPI_A.CreateAccountAsync(AccessToken, TenantID, record));
            task.Wait();
            if (task.Result._Accounts.Count > 0)
            {
                return task.Result._Accounts[0];
            }
            return null;
        }
        /// <summary>
        /// Update an Account
        /// </summary>
        /// <param name="record">Account object (Must contain the AccountID</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Account UpdateAccount(Xero.NetStandard.OAuth2.Model.Accounting.Account record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Account");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Account>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Accounts();
            header._Accounts = list;

            var task = Task.Run(() => xeroAPI_A.UpdateAccountAsync(AccessToken, TenantID, record.AccountID.Value, header));
            task.Wait();
            if (task.Result._Accounts.Count > 0)
            {
                return task.Result._Accounts[0];
            }
            return null;
        }
        /// <summary>
        /// Delete an Account.
        /// Non-system accounts and accounts not used on transactions can be deleted using the delete method. 
        /// If an account is not able to be deleted you can update the status to "ARCHIVED"
        /// </summary>
        /// <param name="accountID">Unique identifier of the account to delete</param>
        /// <returns></returns>
        public bool DeleteAccount(Guid AccountID)
        {
            if (AccountID == null)
            {
                throw new ArgumentNullException("Missing Account ID");
            }
            var task = Task.Run(() => xeroAPI_A.DeleteAccountAsync(AccessToken, TenantID, AccountID));
            task.Wait();
            if (task.Result._Accounts.Count > 0)
            {
                return true;
            }

            return false;
        }
        #endregion




        /// <summary>
        /// Get a list of bank transactions. Retrieve any spend or receive money transactions 
        /// </summary>
        /// <param name="filter">a filter to limit the returned records (leave empty for all records)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="unitdp">e.g. unitdp&#x3D;4 – (Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>List of BankTransaction records</returns>
        #region Bank Transactions 
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransaction> BankTransactions(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, int? unitdp = null)
        {
            int page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }

            var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransaction>(); // Hold the invoices

            var task = Task.Run(() => xeroAPI_A.GetBankTransactionsAsync(AccessToken, TenantID, ModifiedSince, filter, order, page, unitdp));
            task.Wait();

            int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned
            if (task.Result._BankTransactions != null && task.Result._BankTransactions.Count > 0)
            {
                records.AddRange(task.Result._BankTransactions); // Record the first page results
                if (!onlypage.HasValue)
                {
                    // If onlypage is set then the client only wants that page of records so stop processing
                    while (count == 100)
                    {
                        var nextPage = new List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransaction>();
                        task = Task.Run(() => xeroAPI_A.GetBankTransactionsAsync(AccessToken, TenantID, ModifiedSince, filter, order, page, unitdp));
                        task.Wait();
                        records.AddRange(task.Result._BankTransactions); // Add the next page records returned
                        count = nextPage.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    }
                }
            }

            if (records.Count > 0)
            {
                return records;
            }
            return null;
        }
        /// <summary>
        /// Return a single transaction 
        /// </summary>
        /// <param name="transactionID">Guid of the Transaction</param>
        /// <returns>single BankTransaction record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransaction BankTransaction(Guid transactionID)
        {
            var task = Task.Run(() => xeroAPI_A.GetBankTransactionAsync(AccessToken, TenantID, transactionID));
            task.Wait();
            if (task.Result._BankTransactions.Count > 0)
            {
                return task.Result._BankTransactions[0];
            }
            return null;
        }
        #endregion




        #region Bank Transfers 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">a filter to limit the returned records (leave empty for all records)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <returns></returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer> BankTransfers(string filter = null, string order = null, DateTime? ModifiedSince = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetBankTransfersAsync(AccessToken, TenantID, ModifiedSince, filter, order));
            task.Wait();
            if (task.Result._BankTransfers.Count > 0)
            {
                return task.Result._BankTransfers;
            }
            return null;
        }
        /// <summary>
        /// Return a single Bank Transfer Record 
        /// </summary>
        /// <param name="bankTransferID">Xero generated unique identifier for a bank transfer</param>
        /// <returns>single BankTransfer record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer BankTransfer(Guid bankTransferID)
        {
            var task = Task.Run(() => xeroAPI_A.GetBankTransferAsync(AccessToken, TenantID, bankTransferID));
            task.Wait();
            if (task.Result._BankTransfers.Count > 0)
            {
                return task.Result._BankTransfers[0];
            }
            return null;
        }
        /// <summary>
        /// Allows you to create a single bank transfers
        /// </summary>
        /// <param name="record">Single Bank Transfer Object</param>
        /// <returns>The inserted record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer CreateBankTransfer(Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing BankTransfer");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.BankTransfers();
            header._BankTransfers = list;

            var task = Task.Run(() => xeroAPI_A.CreateBankTransferAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._BankTransfers.Count > 0)
            {
                return task.Result._BankTransfers[0];
            }
            return null;
        }
        /// <summary>
        /// Allows you to create a number bank transfers
        /// </summary>
        /// <param name="record">BankTransfers with array of BankTransfer objects in request body</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer CreateBankTransfers(List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException("Missing BankTransfer list");
            }

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.BankTransfers();
            header._BankTransfers = records;

            var task = Task.Run(() => xeroAPI_A.CreateBankTransferAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._BankTransfers.Count > 0)
            {
                return task.Result._BankTransfers[0];
            }
            return null;
        }
        #endregion




        #region Batch Payments
        /// <summary>
        /// Retrieve either one or many BatchPayments for invoices
        /// </summary>
        /// <param name="filter">Filter by an any element (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment BatchPayments(string filter = null, string order = null, DateTime? ModifiedSince = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetBatchPaymentsAsync(AccessToken, TenantID, ModifiedSince, filter, order));
            task.Wait();
            if (task.Result._BatchPayments.Count > 0)
            {
                return task.Result._BatchPayments[0];
            }
            return null;
        }
        /// <summary>
        /// Create one BatchPayments for invoices
        /// </summary>
        /// <param name="record">BatchPayment Record</param>
        /// <returns>BatchPayment record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment CreateBatchPayment(Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing BatchPayment");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.BatchPayments();
            header._BatchPayments = list;

            var task = Task.Run(() => xeroAPI_A.CreateBatchPaymentAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._BatchPayments.Count > 0)
            {
                return task.Result._BatchPayments[0];
            }
            return null;
        }
        /// <summary>
        /// Create a number BatchPayments for invoices
        /// </summary>
        /// <param name="records">Lists of BatchPayment Records</param>
        /// <returns>List of BatchPayment records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment> CreateBatchPayments(List<Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing BatchPayments list");
            }

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.BatchPayments();
            header._BatchPayments = records;

            var task = Task.Run(() => xeroAPI_A.CreateBatchPaymentAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._BatchPayments.Count > 0)
            {
                return task.Result._BatchPayments;
            }
            return null;
        }
        #endregion




        #region BrandingThemes
        /// <summary>
        /// Get a list of Branding Themes. There is no filtering available
        /// </summary>
        /// <returns>List containing all the branding themes</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BrandingTheme> BrandingThemes()
        {
            var task = Task.Run(() => xeroAPI_A.GetBrandingThemesAsync(AccessToken, TenantID));
            task.Wait();
            if (task.Result._BrandingThemes.Count > 0)
            {
                return task.Result._BrandingThemes;
            }
            return null;
        }
        /// <summary>
        /// Get a single branding theme object 
        /// </summary>
        /// <param name="brandingThemeID">The Guid of the branding theme required</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.BrandingTheme BrandingTheme(Guid brandingThemeID)
        {
            var task = Task.Run(() => xeroAPI_A.GetBrandingThemeAsync(AccessToken, TenantID, brandingThemeID));
            task.Wait();
            if (task.Result._BrandingThemes.Count > 0)
            {
                return task.Result._BrandingThemes[0];
            }
            return null;
        }
        #endregion

        #region Budgets
        #endregion



        #region Contacts
        /// <summary>
        /// Return a list of Contacts
        /// </summary>
        /// <param name="filter">Filter by an any element (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="iDs">Filter by a comma separated list of ContactIDs. Allows you to retrieve a specific set of contacts in a single call. (optional)</param>
        /// <param name="includeArchived">e.g. includeArchived&#x3D;true - Contacts with a status of ARCHIVED will be included in the response (optional)</param>
        /// <returns>List of Contacts</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> Contacts(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, List<Guid> iDs = null, bool? includeArchived = null)
        {
            int page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }

            var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>(); // Hold the invoices

            var task = Task.Run(() => xeroAPI_A.GetContactsAsync(AccessToken, TenantID, ModifiedSince, filter, order, iDs, page, includeArchived));
            task.Wait();

            int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned
            if (task.Result._Contacts != null && task.Result._Contacts.Count > 0)
            {
                records.AddRange(task.Result._Contacts); // Record the first page results
                if (!onlypage.HasValue)
                {
                    // If onlypage is set then the client only wants that page of records so stop processing
                    while (count == 100)
                    {
                        var nextPage = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>();
                        task = Task.Run(() => xeroAPI_A.GetContactsAsync(AccessToken, TenantID, ModifiedSince, filter, order, iDs, page, includeArchived));
                        task.Wait();
                        records.AddRange(task.Result._Contacts); // Add the next page records returned
                        count = nextPage.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    }
                }
            }

            if (records.Count > 0)
            {
                return records;
            }
            return null;
        }
        /// <summary>
        /// Return a single contact
        /// </summary>
        /// <param name="contactID">Unique identifier for a Contact</param>
        /// <returns>A contact reocrd</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact GetContact(Guid contactID)
        {
            if (contactID == null)
            {
                throw new ArgumentNullException("Missing Contact ID");
            }

            var task = Task.Run(() => xeroAPI_A.GetContactAsync(AccessToken, TenantID, contactID));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts[0];
            }
            return null;
        }
        /// <summary>
        /// Create a single contact 
        /// </summary>
        /// <param name="record">object holding the Contact Record</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact CreateContact(Xero.NetStandard.OAuth2.Model.Accounting.Contact record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact ");
            }
            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
            header._Contacts = list;

            var task = Task.Run(() => xeroAPI_A.CreateContactsAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts[0];
            }
            return null;
        }
        /// <summary>
        /// Create a number of contact 
        /// </summary>
        /// <param name="records">List of contacts to create</param>
        /// <returns>List of created contacts</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> CreateContacts(List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contacts List");
            }
            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
            header._Contacts = records;

            var task = Task.Run(() => xeroAPI_A.CreateContactsAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts;
            }
            return null;
        }
        /// <summary>
        /// Update an existing single contact. When you are updating a contact you don’t need to specify every element. If you exclude an element then the existing value will be preserved.
        /// </summary>
        /// <param name="record">Object holding the contact</param>
        /// <returns>Updated contact record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact UpdateContact(Xero.NetStandard.OAuth2.Model.Accounting.Contact record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
            header._Contacts = list;

            var task = Task.Run(() => xeroAPI_A.UpdateContactAsync(AccessToken, TenantID, record.ContactID.Value, header));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts[0];
            }
            return null;
        }
        /// <summary>
        /// Update a list of contacts - not sure it will work
        /// </summary>
        /// <param name="records">List of objects holding the contacts to update</param>
        /// <returns>List of updated contacts</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> UpdateContacts(List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contact list");
            }

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
            header._Contacts = records;

            var task = Task.Run(() => xeroAPI_A.UpdateContactAsync(AccessToken, TenantID, new Guid(), header));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts;
            }
            return null;
        }
        #endregion



        #region Contact Groups
        /// <summary>
        /// Retrieve the ContactID and Name of all the contacts in a contact group
        /// </summary>
        /// <param name="filter">Filter by an any element (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of ContactGroup records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup> ContactGroup(string filter = null, string order = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetContactGroupsAsync(AccessToken, TenantID, filter, order));
            task.Wait();
            if (task.Result._ContactGroups.Count > 0)
            {
                return task.Result._ContactGroups;
            }
            return null;
        }
        /// <summary>
        /// Return a unique Contact Group by ID
        /// </summary>
        /// <param name="contactGroupID">Unique identifier for a Contact Group</param>
        /// <returns>The ContactGroup Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup ContactGroup(Guid contactGroupID)
        {
            var task = Task.Run(() => xeroAPI_A.GetContactGroupAsync(AccessToken, TenantID, contactGroupID));
            task.Wait();
            if (task.Result._ContactGroups.Count > 0)
            {
                return task.Result._ContactGroups[0];
            }
            return null;
        }
        /// <summary>
        /// Create a single Contact Group
        /// </summary>
        /// <param name="record">ContactGroup Record</param>
        /// <returns>Create ContactGroup Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup CreateContactGroup(Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact Group Record ");
            }
            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.ContactGroups();
            header._ContactGroups = list;

            var task = Task.Run(() => xeroAPI_A.CreateContactGroupAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._ContactGroups.Count > 0)
            {
                return task.Result._ContactGroups[0];
            }
            return null;
        }
        /// <summary>
        /// Create a number of Contact Groups
        /// </summary>
        /// <param name="records">List of contact Group records</param>
        /// <returns></returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup> CreateContactGroups(List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contact Group Records ");
            }

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.ContactGroups();
            header._ContactGroups = records;

            var task = Task.Run(() => xeroAPI_A.CreateContactGroupAsync(AccessToken, TenantID, header));
            task.Wait();
            if (task.Result._ContactGroups.Count > 0)
            {
                return task.Result._ContactGroups;
            }
            return null;
        }
        /// <summary>
        /// Delete a contact from a Contact Group
        /// </summary>
        /// <param name="ContactGroupID">Group ID</param>
        /// <param name="ContactID">Contact ID</param>
        /// <returns></returns>
        public bool DeleteContactGroupContact(Guid ContactGroupID, Guid ContactID)
        {
            if (ContactGroupID == null || ContactID == null)
            {
                throw new ArgumentNullException("Missing Contact Group ID or ContactID");
            }

            var task = Task.Run(() => xeroAPI_A.DeleteContactGroupContactAsync(AccessToken, TenantID, ContactGroupID, ContactID));

            task.Wait();
            if (task.IsCompleted)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Remove all contacts from a Group
        /// </summary>
        /// <param name="ContactGroupID">Group ID</param>
        /// <returns></returns>
        public bool DeleteAllContactsFromGroup(Guid ContactGroupID)
        {
            if (ContactGroupID == null)
            {
                throw new ArgumentNullException("Missing Contact Group ID ");
            }
            var task = Task.Run(() => xeroAPI_A.DeleteContactGroupContactsAsync(AccessToken, TenantID, ContactGroupID));

            task.Wait();
            if (task.IsCompleted)
            {
                return true;
            }

            return false;
        }
        #endregion





        #region Credit Notes
        /// <summary>
        /// Retrieve a list of Credit Notes
        /// </summary>
        /// <param name="filter">Filter to limit the number of records returned</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>List of Credit Note Records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote> CreditNotes(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, int? unitdp = null)
        {
            int page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }

            var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote>(); // Hold the invoices

            var task = Task.Run(() => xeroAPI_A.GetCreditNotesAsync(AccessToken, TenantID, ModifiedSince, filter, order, page, unitdp));
            task.Wait();

            int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned
            if (task.Result._CreditNotes != null && task.Result._CreditNotes.Count > 0)
            {
                records.AddRange(task.Result._CreditNotes); // Record the first page results
                if (!onlypage.HasValue)
                {
                    // If onlypage is set then the client only wants that page of records so stop processing
                    while (count == 100)
                    {
                        var nextPage = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>();
                        task = Task.Run(() => xeroAPI_A.GetCreditNotesAsync(AccessToken, TenantID, ModifiedSince, filter, order, page, unitdp));
                        task.Wait();
                        records.AddRange(task.Result._CreditNotes); // Add the next page records returned
                        count = nextPage.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    }
                }
            }

            if (records.Count > 0)
            {
                return records;
            }
            return null;
        }
        /// <summary>
        /// Return a single Credit Note
        /// </summary>
        /// <param name="creditNoteID">Unique identifier for a Credit Note</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Single Credit Note Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.CreditNote CreditNote(Guid creditNoteID, int? unitdp = null)
        {
            if (creditNoteID == null)
            {
                throw new ArgumentNullException("Missing Credit Note ID");
            }
            var task = Task.Run(() => xeroAPI_A.GetCreditNoteAsync(AccessToken, TenantID, creditNoteID, unitdp));
            task.Wait();
            if (task.Result._CreditNotes.Count > 0)
            {
                return task.Result._CreditNotes[0];
            }
            return null;
        }
        /// <summary>
        /// Create a single Credit Note
        /// </summary>
        /// <param name="record">Credit Note Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>The created Credit Note</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.CreditNote CreateCreditNote(Xero.NetStandard.OAuth2.Model.Accounting.CreditNote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Credit Note Record");
            }
            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.CreditNotes();
            header._CreditNotes = list;

            var task = Task.Run(() => xeroAPI_A.CreateCreditNotesAsync(AccessToken, TenantID, header, null, unitdp));
            task.Wait();
            if (task.Result._CreditNotes.Count > 0)
            {
                return task.Result._CreditNotes[0];
            }
            return null;
        }
        /// <summary>
        /// Update Existing Credit Note
        /// </summary>
        /// <param name="record">Credit Note Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.CreditNote UpdateCreditNote(Xero.NetStandard.OAuth2.Model.Accounting.CreditNote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Credit Note");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.CreditNotes();
            header._CreditNotes = list;

            var task = Task.Run(() => xeroAPI_A.UpdateCreditNoteAsync(AccessToken, TenantID, record.CreditNoteID.Value, header));
            task.Wait();
            if (task.Result._CreditNotes.Count > 0)
            {
                return task.Result._CreditNotes[0];
            }
            return null;
        }
        #endregion



        #region Currencies
        /// <summary>
        /// Return a list of Currenty records
        /// </summary>
        /// <param name="filter">Filter to limit the number of records returned</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of Currency Records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Currency> Currencies(string filter = null, string order = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetCurrenciesAsync(AccessToken, TenantID, filter, order));
            task.Wait();
            if (task.Result._Currencies.Count > 0)
            {
                return task.Result._Currencies;
            }
            return null;
        }
        /// <summary>
        /// Create a Currency record
        /// </summary>
        /// <param name="record">Currency Record</param>
        /// <returns>The Created Currency Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Currency CreateCurrency(Xero.NetStandard.OAuth2.Model.Accounting.Currency record)
        {
            var task = Task.Run(() => xeroAPI_A.CreateCurrencyAsync(AccessToken, TenantID, record));
            task.Wait();
            if (task.Result._Currencies.Count > 0)
            {
                return task.Result._Currencies[0];
            }
            return null;
        }
        #endregion



        #region History and Notes
        //TODO Added History 

        #endregion



        #region Invoices
        /// <summary>
        /// Get a list of Invoices. 
        /// </summary>
        /// <param name="filter">Filter to limit the number of records returned</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="iDs">Filter by a comma-separated list of InvoicesIDs. (optional)</param>
        /// <param name="invoiceNumbers">Filter by a comma-separated list of InvoiceNumbers. (optional)</param>
        /// <param name="contactIDs">Filter by a comma-separated list of ContactIDs. (optional)</param>
        /// <param name="statuses">Filter by a comma-separated list Statuses. For faster response times it is recommend using these explicit parameters instead of passing OR conditions into the Where filter. (optional)</param>
        /// <param name="includeArchived">e.g. includeArchived&#x3D;true - Contacts with a status of ARCHIVED will be included in the response (optional)</param>
        /// <param name="createdByMyApp">When set to true you&#39;ll only retrieve Invoices created by your app (optional)</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> Invoices(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, List<Guid> iDs = null, List<string> invoiceNumbers = null,
            List<Guid> contactIDs = null, List<string> statuses = null, bool? includeArchived = null, bool? createdByMyApp = null, int? unitdp = null)
        {
            int page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }

            var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>(); // Hold the invoices

            var task = Task.Run(() => xeroAPI_A.GetInvoicesAsync(AccessToken, TenantID, ModifiedSince, filter, order, iDs, invoiceNumbers, contactIDs, statuses, page, includeArchived, createdByMyApp, unitdp));
            task.Wait();

            int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned
            if (task.Result._Invoices != null && task.Result._Invoices.Count > 0)
            {
                records.AddRange(task.Result._Invoices); // Record the first page results
                if (!onlypage.HasValue)
                {
                    // If onlypage is set then the client only wants that page of records so stop processing
                    while (count == 100)
                    {
                        var nextPage = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>();
                        task = Task.Run(() => xeroAPI_A.GetInvoicesAsync(AccessToken, TenantID, ModifiedSince, filter, order, iDs, invoiceNumbers, contactIDs, statuses, page, includeArchived, createdByMyApp, unitdp));
                        task.Wait();
                        records.AddRange(task.Result._Invoices); // Add the next page records returned
                        count = nextPage.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    }
                }
            }

            if (records.Count > 0)
            {
                return records;
            }
            return null;
        }
        /// <summary>
        /// Return a single Invoice Record 
        /// </summary>
        /// <param name="invoiceID">Unique identifier for an Invoice</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice Invoice(Guid invoiceID, int? unitdp = null)
        {
            if (invoiceID == null)
            {
                throw new ArgumentNullException("Missing InvoiceID");
            }
            var task = Task.Run(() => xeroAPI_A.GetInvoiceAsync(AccessToken, TenantID, invoiceID, unitdp));
            task.Wait();
            if (task.Result._Invoices.Count > 0)
            {
                return task.Result._Invoices[0];
            }
            return null;
        }
        /// <summary>
        /// Create Single Invoice 
        /// </summary>
        /// <param name="invoice">Invoice record</param>
        /// /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Created Invoice Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice CreateInvoice(Xero.NetStandard.OAuth2.Model.Accounting.Invoice record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Invoice Record");
            }
            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
            header._Invoices = list;

            var task = Task.Run(() => xeroAPI_A.CreateInvoicesAsync(AccessToken, TenantID, header, null, unitdp));
            task.Wait();
            if (task.Result._Invoices.Count > 0)
            {
                return task.Result._Invoices[0];
            }
            return null;
        }
        /// <summary>
        /// Create a list of invoices
        /// </summary>
        /// <param name="records"></param>
        /// <returns>List of created invoice Records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> CreateInvoices(List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> records, int? unitdp = null)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Invoice Records ");
            }
            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
            header._Invoices = records;

            var task = Task.Run(() => xeroAPI_A.CreateInvoicesAsync(AccessToken, TenantID, header, null, unitdp));
            task.Wait();
            if (task.Result._Invoices.Count > 0)
            {
                return task.Result._Invoices;
            }
            return null;
        }
        /// <summary>
        /// Update a single Invoice Record
        /// </summary>
        /// <param name="record">Invoice record to update</param>
        /// <returns>Updated Invoice Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice UpdateInvoice(Xero.NetStandard.OAuth2.Model.Accounting.Invoice record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Invoice");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
            header._Invoices = list;

            var task = Task.Run(() => xeroAPI_A.UpdateInvoiceAsync(AccessToken, TenantID, record.InvoiceID.Value, header));
            task.Wait();
            if (task.Result._Invoices.Count > 0)
            {
                return task.Result._Invoices[0];
            }
            return null;
        }

        #endregion




        #region Items (Products)
        /// <summary>
        /// Retrive a list of Items
        /// </summary>
        /// /// <param name="filter">Filter to limit the number of records returned</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>        
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <returns>List of Items / Products</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Item> Items(string filter = null, string order = null, int? unitdp = null, DateTime? ModifiedSince = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetItemsAsync(AccessToken, TenantID, ModifiedSince, filter, order, unitdp));
            task.Wait();
            if (task.Result._Items.Count > 0)
            {
                return task.Result._Items;
            }
            return null;
        }
        /// <summary>
        /// Return a single Item/Product
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>        
        /// <returns>Item Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Item Item(Guid itemID, int? unitdp = null)
        {
            if (itemID == null)
            {
                throw new ArgumentNullException("Missing Item ID");
            }
            var task = Task.Run(() => xeroAPI_A.GetItemAsync(AccessToken, TenantID, itemID, unitdp));
            task.Wait();
            if (task.Result._Items.Count > 0)
            {
                return task.Result._Items[0];
            }
            return null;
        }
        /// <summary>
        /// Update an Item/Product Record
        /// </summary>
        /// <param name="record">Item Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Updated Item Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Item UpdateItem(Xero.NetStandard.OAuth2.Model.Accounting.Item record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Item");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Item>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Items();
            header._Items = list;

            var task = Task.Run(() => xeroAPI_A.UpdateItemAsync(AccessToken, TenantID, record.ItemID.Value, header, unitdp));
            task.Wait();
            if (task.Result._Items.Count > 0)
            {
                return task.Result._Items[0];
            }
            return null;
        }
        /// <summary>
        ///  Create a single Item/Product Record
        /// </summary>
        /// <param name="record">Item/Product Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Created Item Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Item CreateItem(Xero.NetStandard.OAuth2.Model.Accounting.Item record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Item record");
            }

            var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Item>();
            list.Add(record);

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Items();
            header._Items = list;

            var task = Task.Run(() => xeroAPI_A.CreateItemsAsync(AccessToken, TenantID, header, null, unitdp));
            task.Wait();
            if (task.Result._Items.Count > 0)
            {
                return task.Result._Items[0];
            }
            return null;
        }
        /// <summary>
        /// Create a number of Item/Product Records
        /// </summary>
        /// <param name="records">List of Item/Product Records</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Item> CreateItems(List<Xero.NetStandard.OAuth2.Model.Accounting.Item> records, int? unitdp = null)
        {
            if (records == null)
            {
                throw new ArgumentNullException("Missing Item records");
            }

            var header = new Xero.NetStandard.OAuth2.Model.Accounting.Items();
            header._Items = records;

            var task = Task.Run(() => xeroAPI_A.CreateItemsAsync(AccessToken, TenantID, header, null, unitdp));
            task.Wait();
            if (task.Result._Items.Count > 0)
            {
                return task.Result._Items;
            }
            return null;
        }
        /// <summary>
        /// Delete an Item/Product
        /// </summary>
        /// <param name="itemID">Item ID</param>
        /// <returns></returns>
        public bool DeleteItem(Guid itemID)
        {
            if (itemID == null)
            {
                throw new ArgumentNullException("Missing Item ID");
            }

            var task = Task.Run(() => xeroAPI_A.DeleteItemAsync(AccessToken, TenantID, itemID));
            task.Wait();
            if (task.IsCompleted)
            {
                return true;
            }

            return false;
        }
        #endregion




        #region Journals
        /// <summary>
        /// Retrieve a list of Jornals.A maximum of 100 journals will be returned in any response. Use the offset or If-Modified-Since filters (see below) with multiple API calls to retrieve larger sets of journals. Journals are ordered oldest to newest.
        /// </summary>
        /// <param name="ifModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="offset">Offset by a specified journal number. e.g. journals with a JournalNumber greater than the offset will be returned (optional)</param>
        /// <param name="paymentsOnly">Filter to retrieve journals on a cash basis. Journals are returned on an accrual basis by default. (optional)</param>
        /// <returns>List of Journal Records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Journal> Journals(DateTime? ModifiedSince = null, int? offset = null, bool? paymentsOnly = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetJournalsAsync(AccessToken, TenantID, ModifiedSince, offset, paymentsOnly));
            task.Wait();
            if (task.Result._Journals.Count > 0)
            {
                return task.Result._Journals;
            }
            return null;
        }
        #endregion




        #region Organisation
        /// <summary>
        /// Retrieve Organisation details
        /// </summary>
        /// <returns></returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Organisation> Organisations()
        {
            var task = Task.Run(() => xeroAPI_A.GetOrganisationsAsync(AccessToken, TenantID));
            task.Wait();
            if (task.Result._Organisations.Count > 0)
            {
                return task.Result._Organisations;
            }
            return null;
        }

        #endregion




        #region Quotes
        /// <summary>
        /// Retrieve a list of Quotes
        /// </summary>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="dateFrom">Filter for quotes after a particular date (optional)</param>
        /// <param name="dateTo">Filter for quotes before a particular date (optional)</param>
        /// <param name="expiryDateFrom">Filter for quotes expiring after a particular date (optional)</param>
        /// <param name="expiryDateTo">Filter for quotes before a particular date (optional)</param>
        /// <param name="contactID">Filter for quotes belonging to a particular contact (optional)</param>
        /// <param name="status">Filter for quotes of a particular Status (optional)</param>
        /// <param name="quoteNumber">Filter by quote number (e.g. GET https://.../Quotes?QuoteNumber&#x3D;QU-0001) (optional)</param>
        /// <returns>List of Quotes</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Quote> Quotes(string order = null, int? onlypage = null, DateTime? ModifiedSince = null, DateTime? dateFrom = null, DateTime? dateTo = null, DateTime? expiryDateFrom = null, DateTime? expiryDateTo = null, Guid? contactID = null, string status = null, string quoteNumber = null)
        {
            int page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }

            var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.Quote>(); // Hold the invoices

            var task = Task.Run(() => xeroAPI_A.GetQuotesAsync(AccessToken, TenantID, ModifiedSince, dateFrom, dateTo, expiryDateFrom, expiryDateTo, contactID, status, page, order, quoteNumber));
            task.Wait();

            int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned
            if (task.Result._Quotes != null && task.Result._Quotes.Count > 0)
            {
                records.AddRange(task.Result._Quotes); // Record the first page results
                if (!onlypage.HasValue)
                {
                    // If onlypage is set then the client only wants that page of records so stop processing
                    while (count == 100)
                    {
                        var nextPage = new List<Xero.NetStandard.OAuth2.Model.Accounting.Quote>();
                        task = Task.Run(() => xeroAPI_A.GetQuotesAsync(AccessToken, TenantID, ModifiedSince, dateFrom, dateTo, expiryDateFrom, expiryDateTo, contactID, status, page, order, quoteNumber));));
                        task.Wait();
                        records.AddRange(task.Result._Quotes); // Add the next page records returned
                        count = nextPage.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    }
                }
            }

            if (records.Count > 0)
            {
                return records;
            }
            return null;
        }

        #endregion








        #region Event Passthrough
        public virtual void onStatusMessageReceived(oAuth2.XeroAuth2EventArgs e)
        {
            EventHandler<oAuth2.XeroAuth2EventArgs> handler = StatusUpdate;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<oAuth2.XeroAuth2EventArgs> StatusUpdate;
        // Pass through the Auth2 status messages
        private void StatusUpdateMessage(object sender, oAuth2.XeroAuth2EventArgs e)
        {
            oAuth2.XeroAuth2EventArgs args = new oAuth2.XeroAuth2EventArgs() { MessageText = e.MessageText, XeroTokenData = e.XeroTokenData, Status = e.Status };
            onStatusMessageReceived(args);
        }
        #endregion
    }
}
