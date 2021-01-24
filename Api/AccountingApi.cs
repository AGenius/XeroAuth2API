using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace XeroAuth2API.Api
{
    /// <summary>
    /// Collection of wrapper functions to interact with the Accounting API endpoints
    /// </summary>
    public class AccountingApi
    {
        Xero.NetStandard.OAuth2.Api.AccountingApi APIClient = new Xero.NetStandard.OAuth2.Api.AccountingApi();
        internal API APICore { get; set; }


        #region Accounts
        /// <summary>
        /// Retrieve the full chart of accounts or filtered list
        /// </summary>
        /// <param name="filter">string containing the filter to apply - e.g. "Class = \"REVENUE\" "</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of Accounts</returns>
        public List<Account> Accounts(string filter = null, string order = null, DateTime? ModifiedSince = null)
        {
            APICore.onStatusUpdates("Fetch Accounts", XeroEventStatus.Log);
            try
            {
                var task = Task.Run(() => APIClient.GetAccountsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
                task.Wait();
                if (task.Result._Accounts.Count > 0)
                {
                    APICore.onStatusUpdates("Fetch Accounts - Success", XeroEventStatus.Log);
                    return task.Result._Accounts;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Provide a way to fetch Accounts using a collection of properties 
        /// </summary>
        /// <param name="Status">List of Status Enums - Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum </param>
        /// <param name="Type">List of AccountType Enums - Xero.NetStandard.OAuth2.Model.Accounting.AccountType</param>
        /// <param name="AccountClass">List of ClassEnum Enums - Xero.NetStandard.OAuth2.Model.Accounting.Account.ClassEnum</param>
        /// <param name="BankAccountType">List of BankAccountTypeEnum Enums - Xero.NetStandard.OAuth2.Model.Accounting.Account.BankAccountTypeEnum</param>
        /// <param name="TaxType">List of TaxType Enums - Xero.NetStandard.OAuth2.Model.Accounting.TaxType</param>
        /// <returns>List of Matching Records</returns>
        public List<Account> Accounts(List<Account.StatusEnum> Status, string order = null,
            List<AccountType> Type = null,
            List<Account.ClassEnum> AccountClass = null,
            List<Account.BankAccountTypeEnum> BankAccountType = null,
            List<TaxType> TaxType = null)
        {
            // Build the where from List collections
            string where = Common.BuildFilterString("Status", Status);

            if (Type != null && Type.Count > 0)
            {
                where += " && " + Common.BuildFilterString("Type", Type);
            }
            if (AccountClass != null && AccountClass.Count > 0)
            {
                where += " && " + Common.BuildFilterString("Class", AccountClass);
            }
            if (BankAccountType != null && BankAccountType.Count > 0)
            {
                where += " && " + Common.BuildFilterString("BankAccountType", BankAccountType);
            }
            if (TaxType != null && TaxType.Count > 0)
            {
                where += " && " + Common.BuildFilterString("TaxType", TaxType);
            }

            return Accounts(where, order);
        }
        /// <summary>
        /// Provide a way to fetch Accounts using a single Property
        /// </summary>
        /// <param name="Status">Status Enum - Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum </param>
        /// <param name="Type">AccountType Enum - Xero.NetStandard.OAuth2.Model.Accounting.AccountType</param>
        /// <param name="AccountClass">ClassEnum Enum - Xero.NetStandard.OAuth2.Model.Accounting.Account.ClassEnum</param>
        /// <param name="BankAccountType">BankAccountTypeEnum Enum - Xero.NetStandard.OAuth2.Model.Accounting.Account.BankAccountTypeEnum</param>
        /// <param name="TaxType">TaxType Enum - Xero.NetStandard.OAuth2.Model.Accounting.TaxType</param>
        /// <returns>List of Matching Records</returns>
        public List<Account> Accounts(Account.StatusEnum Status, string order = null,
            AccountType? Type = null,
            Account.ClassEnum? AccountClass = null,
            Account.BankAccountTypeEnum? BankAccountType = null,
            TaxType? TaxType = null)
        {
            // Build the where from List collections
            string where = Common.BuildFilterString("Status", Status);

            if (Type != null)
            {
                where += " && " + Common.BuildFilterString("Type", Type);
            }
            if (AccountClass != null)
            {
                where += " && " + Common.BuildFilterString("Class", AccountClass);
            }
            if (BankAccountType != null)
            {
                where += " && " + Common.BuildFilterString("BankAccountType", BankAccountType);
            }
            if (TaxType != null)
            {
                where += " && " + Common.BuildFilterString("TaxType", TaxType);
            }

            return Accounts(where, order);
        }
        /// <summary>
        /// Retreive a single Account record 
        /// </summary>
        /// <param name="accountID">Unique identifier for the record</param>
        /// <returns>Account</returns>
        public Account Account(Guid accountID)
        {
            APICore.onStatusUpdates("Fetch Account", XeroEventStatus.Log);
            try
            {
                var task = Task.Run(() => APIClient.GetAccountAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, accountID));
                task.Wait();
                if (task.Result._Accounts.Count > 0)
                {
                    APICore.onStatusUpdates("Fetch Account - Success", XeroEventStatus.Log);
                    return task.Result._Accounts[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create an Account, Currently only supports single accounts
        /// </summary>
        /// <param name="record">Account object</param>
        public Account CreateAccount(Account record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Account");
            }
            try
            {
                var task = Task.Run(() => APIClient.CreateAccountAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, record));
                task.Wait();
                if (task.Result._Accounts.Count > 0)
                {
                    return task.Result._Accounts[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update an Account
        /// </summary>
        /// <param name="record">Account object (Must contain the AccountID</param>
        /// <returns></returns>
        public Account UpdateAccount(Account record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Account");
            }
            try
            {
                var list = new List<Account>();
                list.Add(record);

                var header = new Accounts();
                header._Accounts = list;

                var task = Task.Run(() => APIClient.UpdateAccountAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, record.AccountID.Value, header));
                task.Wait();
                if (task.Result._Accounts.Count > 0)
                {
                    return task.Result._Accounts[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Delete an Account.
        /// Non-system accounts and accounts not used on transactions can be deleted using the delete method. 
        /// If an account is not able to be deleted you can update the status to "ARCHIVED"
        /// </summary>
        /// <param name="accountID">Unique identifier for the record</param>
        /// <returns></returns>
        public bool DeleteAccount(Guid AccountID)
        {
            if (AccountID == null)
            {
                throw new ArgumentNullException("Missing Account ID");
            }
            try
            {
                var task = Task.Run(() => APIClient.DeleteAccountAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, AccountID));
                task.Wait();
                if (task.Result._Accounts.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public List<BankTransaction> BankTransactions(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, int? unitdp = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                if (page == -1) page = null; // This allows a quick first page of records
                var records = new List<BankTransaction>(); // Hold the records 
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned            
                while (count == 100)
                {
                    var task = Task.Run(() => APIClient.GetBankTransactionsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order, page, unitdp));
                    task.Wait();
                    records.AddRange(task.Result._BankTransactions); // Add the next page records returned
                    count = task.Result._BankTransactions.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    if (page != null) page++;
                    if (onlypage.HasValue) count = -1;
                }

                if (records.Count > 0)
                {
                    return records;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a single transaction 
        /// </summary>
        /// <param name="transactionID">Unique identifier for the record</param>
        /// <returns>single BankTransaction record</returns>
        public BankTransaction BankTransaction(Guid transactionID)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetBankTransactionAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, transactionID));
                task.Wait();
                if (task.Result._BankTransactions.Count > 0)
                {
                    return task.Result._BankTransactions[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public List<BankTransfer> BankTransfers(string filter = null, string order = null, DateTime? ModifiedSince = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetBankTransfersAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
                task.Wait();
                if (task.Result._BankTransfers.Count > 0)
                {
                    return task.Result._BankTransfers;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a single Bank Transfer Record 
        /// </summary>
        /// <param name="bankTransferID">Unique identifier for the record</param>
        /// <returns>single BankTransfer record</returns>
        public BankTransfer BankTransfer(Guid bankTransferID)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetBankTransferAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, bankTransferID));
                task.Wait();
                if (task.Result._BankTransfers.Count > 0)
                {
                    return task.Result._BankTransfers[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Allows you to create a single bank transfers
        /// </summary>
        /// <param name="record">Single Bank Transfer Object</param>
        /// <returns>The inserted record</returns>
        public BankTransfer CreateBankTransfer(BankTransfer record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing BankTransfer");
            }
            try
            {
                var list = new List<BankTransfer>();
                list.Add(record);

                var header = new BankTransfers();
                header._BankTransfers = list;

                var task = Task.Run(() => APIClient.CreateBankTransferAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._BankTransfers.Count > 0)
                {
                    return task.Result._BankTransfers[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Allows you to create a number bank transfers
        /// </summary>
        /// <param name="record">BankTransfers with array of BankTransfer objects in request body</param>
        /// <returns></returns>
        public BankTransfer CreateBankTransfers(List<BankTransfer> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException("Missing BankTransfer list");
            }
            try
            {
                var header = new BankTransfers();
                header._BankTransfers = records;

                var task = Task.Run(() => APIClient.CreateBankTransferAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._BankTransfers.Count > 0)
                {
                    return task.Result._BankTransfers[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public BatchPayment BatchPayments(string filter = null, string order = null, DateTime? ModifiedSince = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetBatchPaymentsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
                task.Wait();
                if (task.Result._BatchPayments.Count > 0)
                {
                    return task.Result._BatchPayments[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create one BatchPayments for invoices
        /// </summary>
        /// <param name="record">BatchPayment Record</param>
        /// <returns>BatchPayment record</returns>
        public BatchPayment CreateBatchPayment(BatchPayment record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing BatchPayment");
            }
            try
            {
                var list = new List<BatchPayment>();
                list.Add(record);

                var header = new BatchPayments();
                header._BatchPayments = list;

                var task = Task.Run(() => APIClient.CreateBatchPaymentAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._BatchPayments.Count > 0)
                {
                    return task.Result._BatchPayments[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a number BatchPayments for invoices
        /// </summary>
        /// <param name="records">Lists of BatchPayment Records</param>
        /// <returns>List of BatchPayment records</returns>
        public List<BatchPayment> CreateBatchPayments(List<BatchPayment> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing BatchPayments list");
            }
            try
            {
                var header = new BatchPayments();
                header._BatchPayments = records;

                var task = Task.Run(() => APIClient.CreateBatchPaymentAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._BatchPayments.Count > 0)
                {
                    return task.Result._BatchPayments;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        #endregion




        #region BrandingThemes
        /// <summary>
        /// Get a list of Branding Themes. There is no filtering available
        /// </summary>
        /// <returns>List containing all the branding themes</returns>
        public List<BrandingTheme> BrandingThemes()
        {
            try
            {
                var task = Task.Run(() => APIClient.GetBrandingThemesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID));
                task.Wait();
                if (task.Result._BrandingThemes.Count > 0)
                {
                    return task.Result._BrandingThemes;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Get a single branding theme object 
        /// </summary>
        /// <param name="brandingThemeID">Unique identifier for the record</param>
        /// <returns></returns>
        public BrandingTheme BrandingTheme(Guid brandingThemeID)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetBrandingThemeAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, brandingThemeID));
                task.Wait();
                if (task.Result._BrandingThemes.Count > 0)
                {
                    return task.Result._BrandingThemes[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        #endregion

        #region Budgets
        #endregion



        #region Contacts
        /// <summary>
        /// Provide an Enum for the Contact Type
        /// </summary>
        public enum ContactType
        {
            isCustomer,
            isSupplier,
            Either
        }
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
        public List<Contact> Contacts(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, List<Guid> iDs = null, bool? includeArchived = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Contact>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned     
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => APIClient.GetContactsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order, iDs, page, includeArchived));
                    task.Wait();
                    records.AddRange(task.Result._Contacts); // Add the next page records returned
                    count = task.Result._Contacts.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    if (page != null) page++;
                    if (onlypage.HasValue) count = -1;
                }

                if (records.Count > 0)
                {
                    return records;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Provide a way to fetch Contacts using a single Property
        /// </summary>
        /// <param name="Status">List of Status Enum - Xero.NetStandard.OAuth2.Model.Accounting.Contact.StatusEnum </param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="AddressType">List of AddressType Enum - Xero.NetStandard.OAuth2.Model.Accounting.Address.AddressType</param>       
        /// <returns>List of Matching Records</returns>
        public List<Contact> Contacts(List<Contact.ContactStatusEnum> Status, string order = null,
            List<Address.AddressTypeEnum> AddressType = null)
        {
            // Build the where from List collections
            string where = Common.BuildFilterString("ContactStatus", Status);

            if (AddressType != null && AddressType.Count > 0)
            {
                where += " && " + Common.BuildFilterString("AddressType", AddressType);
            }

            return Contacts(where, order);
        }
        /// <summary>
        /// Provide a way to fetch Contacts using a single Property
        /// </summary>
        /// <param name="Status">Status Enum - Xero.NetStandard.OAuth2.Model.Accounting.Contact.StatusEnum </param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="AddressType">AddressType Enum - Xero.NetStandard.OAuth2.Model.Accounting.Address.AddressType</param>       
        /// <returns>List of Matching Records</returns>
        public List<Contact> Contacts(Contact.ContactStatusEnum Status, string order = null,
        Address.AddressTypeEnum? AddressType = null,
        ContactType contactType = ContactType.Either)
        {
            // Build the where from enums
            string where = Common.BuildFilterString("ContactStatus", Status);

            if (AddressType != null)
            {
                where += " && " + Common.BuildFilterString("AddressType", AddressType);
            }
            switch (contactType)
            {
                case ContactType.isCustomer:
                    where += " && isCustomer=True";
                    break;
                case ContactType.isSupplier:
                    where += " && isSupplier=True";
                    break;
            }

            return Contacts(where, order);
        }
        /// <summary>
        /// Return a single contact
        /// </summary>
        /// <param name="contactID">Unique identifier for the record</param>
        /// <returns>A contact reocrd</returns>
        public Contact Contact(Guid contactID)
        {
            if (contactID == null)
            {
                throw new ArgumentNullException("Missing Contact ID");
            }
            try
            {
                var task = Task.Run(() => APIClient.GetContactAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, contactID));
                task.Wait();
                if (task.Result._Contacts.Count > 0)
                {
                    return task.Result._Contacts[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a single contact 
        /// </summary>
        /// <param name="record">object holding the Contact Record</param>
        /// <returns></returns>
        public Contact CreateContact(Contact record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact ");
            }
            try
            {
                var list = new List<Contact>();
                list.Add(record);

                var header = new Contacts();
                header._Contacts = list;

                var task = Task.Run(() => APIClient.CreateContactsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._Contacts.Count > 0)
                {
                    return task.Result._Contacts[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a number of contact 
        /// </summary>
        /// <param name="records">List of contacts to create</param>
        /// <returns>List of created contacts</returns>
        public List<Contact> CreateContacts(List<Contact> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contacts List");
            }
            try
            {
                var header = new Contacts();
                header._Contacts = records;

                var task = Task.Run(() => APIClient.CreateContactsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._Contacts.Count > 0)
                {
                    return task.Result._Contacts;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update an existing single contact. When you are updating a contact you don’t need to specify every element. If you exclude an element then the existing value will be preserved.
        /// </summary>
        /// <param name="record">Object holding the contact</param>
        /// <returns>Updated contact record</returns>
        public Contact UpdateContact(Contact record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact");
            }
            try
            {
                var list = new List<Contact>();
                list.Add(record);

                var header = new Contacts();
                header._Contacts = list;

                var task = Task.Run(() => APIClient.UpdateContactAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, record.ContactID.Value, header));
                task.Wait();
                if (task.Result._Contacts.Count > 0)
                {
                    return task.Result._Contacts[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update a list of contacts - not sure it will work
        /// </summary>
        /// <param name="records">List of objects holding the contacts to update</param>
        /// <returns>List of updated contacts</returns>
        public List<Contact> UpdateContacts(List<Contact> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contact list");
            }
            try
            {
                var header = new Contacts();
                header._Contacts = records;

                var task = Task.Run(() => APIClient.UpdateContactAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, new Guid(), header));
                task.Wait();
                if (task.Result._Contacts.Count > 0)
                {
                    return task.Result._Contacts;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public List<ContactGroup> ContactGroup(string filter = null, string order = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetContactGroupsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, filter, order));
                task.Wait();
                if (task.Result._ContactGroups.Count > 0)
                {
                    return task.Result._ContactGroups;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a unique Contact Group by ID
        /// </summary>
        /// <param name="contactGroupID">Unique identifier for the record</param>
        /// <returns>The ContactGroup Record</returns>
        public ContactGroup ContactGroup(Guid contactGroupID)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetContactGroupAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, contactGroupID));
                task.Wait();
                if (task.Result._ContactGroups.Count > 0)
                {
                    return task.Result._ContactGroups[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a single Contact Group
        /// </summary>
        /// <param name="record">ContactGroup Record</param>
        /// <returns>Create ContactGroup Record</returns>
        public ContactGroup CreateContactGroup(ContactGroup record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact Group Record ");
            }
            try
            {
                var list = new List<ContactGroup>();
                list.Add(record);

                var header = new ContactGroups();
                header._ContactGroups = list;

                var task = Task.Run(() => APIClient.CreateContactGroupAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._ContactGroups.Count > 0)
                {
                    return task.Result._ContactGroups[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a number of Contact Groups
        /// </summary>
        /// <param name="records">List of contact Group records</param>
        /// <returns></returns>
        public List<ContactGroup> CreateContactGroups(List<ContactGroup> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contact Group Records ");
            }
            try
            {
                var header = new ContactGroups();
                header._ContactGroups = records;

                var task = Task.Run(() => APIClient.CreateContactGroupAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._ContactGroups.Count > 0)
                {
                    return task.Result._ContactGroups;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Delete a contact from a Contact Group
        /// </summary>
        /// <param name="ContactGroupID">Unique identifier for the record</param>
        /// <param name="ContactID">Contact ID</param>
        /// <returns></returns>
        public bool DeleteContactGroupContact(Guid ContactGroupID, Guid ContactID)
        {
            if (ContactGroupID == null || ContactID == null)
            {
                throw new ArgumentNullException("Missing Contact Group ID or ContactID");
            }
            try
            {
                var task = Task.Run(() => APIClient.DeleteContactGroupContactAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ContactGroupID, ContactID));
                task.Wait();
                if (task.IsCompleted)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
            try
            {
                var task = Task.Run(() => APIClient.DeleteContactGroupContactsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ContactGroupID));
                task.Wait();
                if (task.IsCompleted)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public List<CreditNote> CreditNotes(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, int? unitdp = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<CreditNote>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned            // If onlypage is set then the client only wants that page of records so stop processing
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => APIClient.GetCreditNotesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order, page, unitdp));
                    task.Wait();
                    records.AddRange(task.Result._CreditNotes); // Add the next page records returned
                    count = task.Result._CreditNotes.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    if (page != null) page++;
                    if (onlypage.HasValue) count = -1;
                }

                if (records.Count > 0)
                {
                    return records;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a list of CreditNotes using enums
        /// </summary>
        /// <param name="Status">List of StatusEnum Enum - Xero.NetStandard.OAuth2.Model.Accounting.CreditNote.StatusEnum </param>         
        /// <param name="FromDate">DateTime - CreditNote dated from this value</param>   
        /// <param name="ToDate">DateTime - CreditNote dated opto this value</param>           
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of CreditNotes</returns>
        public List<CreditNote> CreditNotes(List<CreditNote.StatusEnum> Status, DateTime? FromDate = null, DateTime? ToDate = null, string order = null)
        {
            string where = string.Empty;
            // Build the where from List collections
            if (Status != null)
            {
                where = Common.BuildFilterString("Status", Status);
            }

            // Add the Date Range to the filter string
            if (FromDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                if (!string.IsNullOrEmpty(where))
                {
                    where += " && ";
                }
                where += $"Date >= DateTime ({FromDate.Value.Year},{FromDate.Value.Month},{FromDate.Value.Day}) ";
            }
            if (ToDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                if (!string.IsNullOrEmpty(where))
                {
                    where += " && ";
                }
                where += $"Date <= DateTime ({ToDate.Value.Year},{ToDate.Value.Month},{ToDate.Value.Day}) ";
            }

            return CreditNotes(where, order);
        }
        /// <summary>
        /// Return a list of CreditNotes using enums
        /// </summary>
        /// <param name="Status">StatusEnum Enum - Xero.NetStandard.OAuth2.Model.Accounting.CreditNote.StatusEnum </param>        
        /// <param name="FromDate">DateTime - CreditNotes dated from this value</param>   
        /// <param name="ToDate">DateTime - CreditNotes dated opto this value</param>           
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of CreditNotes</returns>
        public List<CreditNote> CreditNotes(CreditNote.StatusEnum Status, DateTime? FromDate = null, DateTime? ToDate = null, string order = null)
        {
            // Build the where from List collections
            string where = Common.BuildFilterString("Status", Status);
            // Add the Date Range to the filter string

            if (FromDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                where += " && " + $"Date >= DateTime ({FromDate.Value.Year},{FromDate.Value.Month},{FromDate.Value.Day}) ";
            }
            if (ToDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                where += " && " + $"Date <= DateTime ({ToDate.Value.Year},{ToDate.Value.Month},{ToDate.Value.Day}) ";
            }

            return CreditNotes(where, order);
        }
        /// <summary>
        /// Return a single Credit Note
        /// </summary>
        /// <param name="creditNoteID">Unique identifier for the record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Single Credit Note Record</returns>
        public CreditNote CreditNote(Guid creditNoteID, int? unitdp = null)
        {
            if (creditNoteID == null)
            {
                throw new ArgumentNullException("Missing Credit Note ID");
            }
            try
            {
                var task = Task.Run(() => APIClient.GetCreditNoteAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, creditNoteID, unitdp));
                task.Wait();
                if (task.Result._CreditNotes.Count > 0)
                {
                    return task.Result._CreditNotes[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a single Credit Note
        /// </summary>
        /// <param name="record">Credit Note Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>The created Credit Note</returns>
        public CreditNote CreateCreditNote(CreditNote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Credit Note Record");
            }
            try
            {
                var list = new List<CreditNote>();
                list.Add(record);

                var header = new CreditNotes();
                header._CreditNotes = list;

                var task = Task.Run(() => APIClient.CreateCreditNotesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header, null, unitdp));
                task.Wait();
                if (task.Result._CreditNotes.Count > 0)
                {
                    return task.Result._CreditNotes[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update Existing Credit Note
        /// </summary>
        /// <param name="record">Credit Note Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public CreditNote UpdateCreditNote(CreditNote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Credit Note");
            }
            try
            {
                var list = new List<CreditNote>();
                list.Add(record);

                var header = new CreditNotes();
                header._CreditNotes = list;

                var task = Task.Run(() => APIClient.UpdateCreditNoteAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, record.CreditNoteID.Value, header));
                task.Wait();
                if (task.Result._CreditNotes.Count > 0)
                {
                    return task.Result._CreditNotes[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public List<Currency> Currencies(string filter = null, string order = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetCurrenciesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, filter, order));
                task.Wait();
                if (task.Result._Currencies.Count > 0)
                {
                    return task.Result._Currencies;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a Currency record
        /// </summary>
        /// <param name="record">Currency Record</param>
        /// <returns>The Created Currency Record</returns>
        public Currency CreateCurrency(Currency record)
        {
            try
            {
                var task = Task.Run(() => APIClient.CreateCurrencyAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, record));
                task.Wait();
                if (task.Result._Currencies.Count > 0)
                {
                    return task.Result._Currencies[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional) provide 0 for first page quick fetch (no additional collections)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="iDs">Filter by a comma-separated list of InvoicesIDs. (optional)</param>
        /// <param name="invoiceNumbers">Filter by a comma-separated list of InvoiceNumbers. (optional)</param>
        /// <param name="contactIDs">Filter by a comma-separated list of ContactIDs. (optional)</param>
        /// <param name="statuses">Filter by a comma-separated list Statuses. For faster response times it is recommend using these explicit parameters instead of passing OR conditions into the Where filter. (optional)</param>
        /// <param name="includeArchived">e.g. includeArchived&#x3D;true - Contacts with a status of ARCHIVED will be included in the response (optional)</param>
        /// <param name="createdByMyApp">When set to true you&#39;ll only retrieve Invoices created by your app (optional)</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public List<Invoice> Invoices(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, List<Guid> iDs = null,
            List<string> invoiceNumbers = null,
            List<Guid> contactIDs = null,
            List<string> statuses = null, bool? includeArchived = null, bool? createdByMyApp = null, int? unitdp = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Invoice>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned      
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => APIClient.GetInvoicesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order, iDs, invoiceNumbers, contactIDs, statuses, page, includeArchived, createdByMyApp, unitdp));
                    task.Wait();
                    records.AddRange(task.Result._Invoices); // Add the next page records returned
                    count = task.Result._Invoices.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    if (page != null) page++;
                    if (onlypage.HasValue) count = -1;
                }

                if (records.Count > 0)
                {
                    return records;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a list of Invoices using enums
        /// </summary>
        /// <param name="Status">List of StatusEnum Enum - Xero.NetStandard.OAuth2.Model.Accounting.Invoice.StatusEnum </param>        
        /// <param name="FromDate">DateTime - Invoices dated from this value</param>   
        /// <param name="ToDate">DateTime - Invoices dated opto this value</param>           
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of Invoices</returns>
        public List<Invoice> Invoices(List<Invoice.StatusEnum> Status, DateTime? FromDate = null, DateTime? ToDate = null, string order = null)
        {
            // Build the where from List collections
            string where = Common.BuildFilterString("Status", Status);
            // Add the Date Range to the filter string

            if (FromDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                where += " && " + $"Date >= DateTime ({FromDate.Value.Year},{FromDate.Value.Month},{FromDate.Value.Day}) ";
            }
            if (ToDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                where += " && " + $"Date <= DateTime ({ToDate.Value.Year},{ToDate.Value.Month},{ToDate.Value.Day}) ";
            }

            return Invoices(where, order);
        }
        /// <summary>
        /// Return a list of Invoices using enums
        /// </summary>
        /// <param name="Status">StatusEnum Enum - Xero.NetStandard.OAuth2.Model.Accounting.Invoice.StatusEnum </param>        
        /// <param name="FromDate">DateTime - Invoices dated from this value</param>   
        /// <param name="ToDate">DateTime - Invoices dated opto this value</param>           
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of Invoices</returns>
        public List<Invoice> Invoices(Invoice.StatusEnum Status, DateTime? FromDate = null, DateTime? ToDate = null, string order = null)
        {
            // Build the where from List collections
            string where = Common.BuildFilterString("Status", Status);
            // Add the Date Range to the filter string

            if (FromDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                where += " && " + $"Date >= DateTime ({FromDate.Value.Year},{FromDate.Value.Month},{FromDate.Value.Day}) ";
            }
            if (ToDate.HasValue)
            {
                // Remove the Time Portion when adding to filter
                where += " && " + $"Date <= DateTime ({ToDate.Value.Year},{ToDate.Value.Month},{ToDate.Value.Day}) ";
            }

            return Invoices(where, order);
        }
        /// <summary>
        /// Return a single Invoice Record 
        /// </summary>
        /// <param name="invoiceID">Unique identifier for the record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public Invoice Invoice(Guid invoiceID, int? unitdp = null)
        {
            if (invoiceID == null)
            {
                throw new ArgumentNullException("Missing InvoiceID");
            }
            try
            {
                var task = Task.Run(() => APIClient.GetInvoiceAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, invoiceID, unitdp));
                task.Wait();
                if (task.Result._Invoices.Count > 0)
                {
                    return task.Result._Invoices[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create Single Invoice 
        /// </summary>
        /// <param name="invoice">Invoice record</param>
        /// /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Created Invoice Record</returns>
        public Invoice CreateInvoice(Invoice record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Invoice Record");
            }
            try
            {
                var list = new List<Invoice>();
                list.Add(record);

                var header = new Invoices();
                header._Invoices = list;

                var task = Task.Run(() => APIClient.CreateInvoicesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header, null, unitdp));
                task.Wait();
                if (task.Result._Invoices.Count > 0)
                {
                    return task.Result._Invoices[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a list of invoices
        /// </summary>
        /// <param name="records"></param>
        /// <returns>List of created invoice Records</returns>
        public List<Invoice> CreateInvoices(List<Invoice> records, int? unitdp = null)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Invoice Records ");
            }
            try
            {
                var header = new Invoices();
                header._Invoices = records;

                var task = Task.Run(() => APIClient.CreateInvoicesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header, null, unitdp));
                task.Wait();
                if (task.Result._Invoices.Count > 0)
                {
                    return task.Result._Invoices;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update a single Invoice Record
        /// </summary>
        /// <param name="record">Invoice record to update</param>
        /// <returns>Updated Invoice Record</returns>
        public Invoice UpdateInvoice(Invoice record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Invoice");
            }
            try
            {
                var list = new List<Invoice>();
                list.Add(record);

                var header = new Invoices();
                header._Invoices = list;

                var task = Task.Run(() => APIClient.UpdateInvoiceAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, record.InvoiceID.Value, header));
                task.Wait();
                if (task.Result._Invoices.Count > 0)
                {
                    return task.Result._Invoices[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public List<Item> Items(string filter = null, string order = null, int? unitdp = null, DateTime? ModifiedSince = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetItemsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order, unitdp));
                task.Wait();
                if (task.Result._Items.Count > 0)
                {
                    return task.Result._Items;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a single Item/Product
        /// </summary>
        /// <param name="itemID">Unique identifier for the record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>        
        /// <returns>Item Record</returns>
        public Item Item(Guid itemID, int? unitdp = null)
        {
            if (itemID == null)
            {
                throw new ArgumentNullException("Missing Item ID");
            }
            try
            {
                var task = Task.Run(() => APIClient.GetItemAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, itemID, unitdp));
                task.Wait();
                if (task.Result._Items.Count > 0)
                {
                    return task.Result._Items[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update an Item/Product Record
        /// </summary>
        /// <param name="record">Item Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Updated Item Record</returns>
        public Item UpdateItem(Item record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Item");
            }
            try
            {
                var list = new List<Item>();
                list.Add(record);

                var header = new Items();
                header._Items = list;

                var task = Task.Run(() => APIClient.UpdateItemAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, record.ItemID.Value, header, unitdp));
                task.Wait();
                if (task.Result._Items.Count > 0)
                {
                    return task.Result._Items[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        ///  Create a single Item/Product Record
        /// </summary>
        /// <param name="record">Item/Product Record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Created Item Record</returns>
        public Item CreateItem(Item record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Item record");
            }
            try
            {
                var list = new List<Item>();
                list.Add(record);

                var header = new Items();
                header._Items = list;

                var task = Task.Run(() => APIClient.CreateItemsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header, null, unitdp));
                task.Wait();
                if (task.Result._Items.Count > 0)
                {
                    return task.Result._Items[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create a number of Item/Product Records
        /// </summary>
        /// <param name="records">List of Item/Product Records</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public List<Item> CreateItems(List<Item> records, int? unitdp = null)
        {
            if (records == null)
            {
                throw new ArgumentNullException("Missing Item records");
            }
            try
            {
                var header = new Items();
                header._Items = records;

                var task = Task.Run(() => APIClient.CreateItemsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header, null, unitdp));
                task.Wait();
                if (task.Result._Items.Count > 0)
                {
                    return task.Result._Items;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Delete an Item/Product
        /// </summary>
        /// <param name="itemID">Unique identifier for the record</param>
        /// <returns></returns>
        public bool DeleteItem(Guid itemID)
        {
            if (itemID == null)
            {
                throw new ArgumentNullException("Missing Item ID");
            }
            try
            {
                var task = Task.Run(() => APIClient.DeleteItemAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, itemID));
                task.Wait();
                if (task.IsCompleted)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return false;
        }
        #endregion




        #region Journals
        /// <summary>
        /// Retrieve a list of Jornals.A maximum of 100 journals will be returned in any response. Use the offset or If-Modified-Since filters (see below) with multiple API calls to retrieve larger sets of journals. Journals are ordered oldest to newest.
        /// </summary>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="offset">Offset by a specified journal number. e.g. journals with a JournalNumber greater than the offset will be returned (optional)</param>
        /// <param name="paymentsOnly">Filter to retrieve journals on a cash basis. Journals are returned on an accrual basis by default. (optional)</param>
        /// <returns>List of Journal Records</returns>
        public List<Journal> Journals(DateTime? ModifiedSince = null, int? offset = null, bool? paymentsOnly = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetJournalsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, offset, paymentsOnly));
                task.Wait();
                if (task.Result._Journals.Count > 0)
                {
                    return task.Result._Journals;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        #endregion




        #region Organisation
        /// <summary>
        /// Retrieve Organisation details
        /// </summary>
        /// <returns></returns>
        public List<Organisation> Organisations()
        {
            try
            {
                var task = Task.Run(() => APIClient.GetOrganisationsAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID));
                task.Wait();
                if (task.Result._Organisations.Count > 0)
                {
                    return task.Result._Organisations;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
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
        public List<Quote> Quotes(string order = null, int? onlypage = null, DateTime? ModifiedSince = null, DateTime? dateFrom = null, DateTime? dateTo = null, DateTime? expiryDateFrom = null, DateTime? expiryDateTo = null, Guid? contactID = null, string status = null, string quoteNumber = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Quote>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned             
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => APIClient.GetQuotesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, dateFrom, dateTo, expiryDateFrom, expiryDateTo, contactID, status, page, order, quoteNumber));
                    task.Wait();
                    records.AddRange(task.Result._Quotes); // Add the next page records returned
                    count = task.Result._Quotes.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    if (page != null) page++;
                    if (onlypage.HasValue) count = -1;
                }

                if (records.Count > 0)
                {
                    return records;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Retrieve single Quote Record
        /// </summary>
        /// <param name="quoteID">Unique identifier for the record</param>
        /// <returns>Quote Object</returns>
        public Quote Quote(Guid quoteID)
        {
            if (quoteID == null)
            {
                throw new ArgumentNullException("Missing QuoteID");
            }
            try
            {
                var task = Task.Run(() => APIClient.GetQuoteAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, quoteID));
                task.Wait();
                if (task.Result._Quotes.Count > 0)
                {
                    return task.Result._Quotes[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        public Quote CreateQuote(Quote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Quote Record");
            }
            try
            {
                var list = new List<Quote>();
                list.Add(record);

                var header = new Quotes();
                header._Quotes = list;

                var task = Task.Run(() => APIClient.CreateQuotesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._Quotes.Count > 0)
                {
                    return task.Result._Quotes[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        #endregion




        #region Tax Rates
        /// <summary>
        /// Return a list of Tax Types
        /// </summary>
        /// <param name="filter">a filter to limit the returned records (leave empty for all records)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="taxType">Filter by tax type (optional)</param>
        /// <returns>List of TaxRate Records</returns>
        public List<TaxRate> TaxRates(string filter = null, string order = null, string taxType = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetTaxRatesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, filter, order, taxType));
                task.Wait();
                if (task.Result._TaxRates.Count > 0)
                {
                    return task.Result._TaxRates;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a single TaxRate record
        /// </summary>
        /// <param name="name">Name of the TaxRate</param>
        /// <returns>TaxRate Record</returns>
        public TaxRate TaxRate(string name)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetTaxRatesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, $"Name =\"{name}\""));
                task.Wait();
                if (task.Result._TaxRates.Count > 0)
                {
                    return task.Result._TaxRates[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update a single Tax Rate
        /// </summary>
        /// <param name="record">TaxRate Record</param>
        /// <returns></returns>
        public TaxRate CreateTaxRate(TaxRate record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing TaxRate");
            }
            try
            {
                var list = new List<TaxRate>();
                list.Add(record);

                var header = new TaxRates();
                header._TaxRates = list;

                var task = Task.Run(() => APIClient.CreateTaxRatesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._TaxRates.Count > 0)
                {
                    return task.Result._TaxRates[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Create multiple TaxRate records
        /// </summary>
        /// <param name="records">List of TaxRate Records</param>
        /// <returns>List of created TaxRate Records</returns>
        public List<TaxRate> CreateTaxRates(List<TaxRate> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing TaxRate Records");
            }
            try
            {
                var header = new TaxRates();
                header._TaxRates = records;

                var task = Task.Run(() => APIClient.CreateTaxRatesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._TaxRates.Count > 0)
                {
                    return task.Result._TaxRates;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update a TaxRate Record
        /// </summary>
        /// <param name="record">TaxRate Record to Update</param>
        /// <returns>Updated TaxRate Record</returns>
        public TaxRate UpdateTaxRate(TaxRate record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing TaxRate");
            }
            try
            {
                var list = new List<TaxRate>();
                list.Add(record);

                var header = new TaxRates();
                header._TaxRates = list;

                var task = Task.Run(() => APIClient.UpdateTaxRateAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._TaxRates.Count > 0)
                {
                    return task.Result._TaxRates[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Update multiple TaxRate records
        /// </summary>
        /// <param name="records">List of TaxRate Records to Update</param>
        /// <returns>List of Updated TaxRate Records</returns>
        public List<TaxRate> UpdateTaxRates(List<TaxRate> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing TaxRate Records");
            }
            try
            {
                var header = new TaxRates();
                header._TaxRates = records;

                var task = Task.Run(() => APIClient.UpdateTaxRateAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, header));
                task.Wait();
                if (task.Result._TaxRates.Count > 0)
                {
                    return task.Result._TaxRates;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        #endregion




        #region Tracking Categories
        /// <summary>
        /// Retrieve a list of TrackingCategory Records
        /// </summary>
        /// <param name="filter">a filter to limit the returned records (leave empty for all records) - Default is Status = Active</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="includeArchived"></param>
        /// <returns>List of TrackingCategory Records</returns>
        public List<TrackingCategory> TrackingCategories(string filter = "Status=\"ACTIVE\"", string order = null, bool? includeArchived = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetTrackingCategoriesAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, filter, order, includeArchived));
                task.Wait();
                if (task.Result._TrackingCategories.Count > 0)
                {
                    return task.Result._TrackingCategories;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a single TrackingCategory Record
        /// </summary>
        /// <param name="trackingCategoryID">Unique identifier for the record</param>
        /// <returns>TrackingCategory Record</returns>
        public TrackingCategory TrackingCategory(Guid trackingCategoryID)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetTrackingCategoryAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, trackingCategoryID));
                task.Wait();
                if (task.Result._TrackingCategories.Count > 0)
                {
                    return task.Result._TrackingCategories[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        #endregion




        #region Users
        /// <summary>
        /// Retrieve a list of User Records
        /// </summary>
        /// <param name="filter">string containing the filter to apply - e.g. "Class = \"REVENUE\" "</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of User Records</returns>
        public List<User> Users(string filter = null, DateTime? ModifiedSince = null, string order = null)
        {
            try
            {
                var task = Task.Run(() => APIClient.GetUsersAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
                task.Wait();
                if (task.Result._Users.Count > 0)
                {
                    return task.Result._Users;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        #endregion





        #region Purchase Orders
        /// <summary>
        /// Return a list or PurchaseOrders
        /// </summary>
        /// <param name="onlypage">Up to 100 records will be returned in a single API call with line items details (optional)</param>
        /// <param name="status">Filter by purchase order status (optional)</param>
        /// <param name="ModifiedSince">Only records created or modified since this timestamp will be returned (optional)</param>
        /// <param name="dateFrom">Filter by purchase order date (e.g. GET https://.../PurchaseOrders?DateFrom&#x3D;2015-12-01&amp;DateTo&#x3D;2015-12-31 (optional)</param>
        /// <param name="dateTo">Filter by purchase order date (e.g. GET https://.../PurchaseOrders?DateFrom&#x3D;2015-12-01&amp;DateTo&#x3D;2015-12-31 (optional)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <returns>List of Purchase Order Records</returns>
        public List<PurchaseOrder> PurchaseOrders(int? onlypage = null, string status = null, DateTime? ModifiedSince = null, string dateFrom = null, string dateTo = null, string order = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<PurchaseOrder>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned  
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => APIClient.GetPurchaseOrdersAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, ModifiedSince, status, dateFrom, dateTo, order, page));
                    task.Wait();
                    records.AddRange(task.Result._PurchaseOrders); // Add the next page records returned
                    count = task.Result._PurchaseOrders.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
                    if (page != null) page++;
                    if (onlypage.HasValue) count = -1;
                }

                if (records.Count > 0)
                {
                    return records;
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }
        /// <summary>
        /// Return a single PurchaseOrder
        /// </summary>
        /// <param name="purchaseOrderID">Unique identifier for the record</param>
        /// <returns>PurchaseOrder Record</returns>
        public PurchaseOrder PurchaseOrder(Guid purchaseOrderID)
        {
            if (purchaseOrderID == null)
            {
                throw new ArgumentNullException("Missing PurchaseOrder ID");
            }
            try
            {
                var task = Task.Run(() => APIClient.GetPurchaseOrderAsync(APICore.XeroConfig.XeroAPIToken.AccessToken, APICore.XeroConfig.SelectedTenantID, purchaseOrderID));
                task.Wait();
                if (task.Result._PurchaseOrders.Count > 0)
                {
                    return task.Result._PurchaseOrders[0];
                }
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }

            return null;
        }

        #endregion


    }
}
