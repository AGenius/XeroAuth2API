using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    public class API
    {
        oAuth2 _authClient = null;
        Xero.NetStandard.OAuth2.Api.AccountingApi xeroAPI_A = new Xero.NetStandard.OAuth2.Api.AccountingApi();
        Xero.NetStandard.OAuth2.Api.AssetApi xeroAPI_Assets = new Xero.NetStandard.OAuth2.Api.AssetApi();
        public Model.XeroConfiguration XeroConfig { get; set; }
        /// <summary>
        /// If provided, the API setup will try and match the name with the correct tenant otherwise the first tenant will be selected
        /// </summary>
        public string TenantName
        {
            get
            {
                if (XeroConfig != null)
                {
                    return XeroConfig.SelectedTenantName;
                }
                return null;
            }
        }
        /// <summary>
        /// Provides access to the available tenants authorized
        /// </summary>
        public List<Model.Tenant> Tenants
        {
            get
            {
                if (XeroConfig != null && XeroConfig.XeroAPIToken != null)
                {
                    return XeroConfig.XeroAPIToken.Tenants;
                }
                return null;
            }
        }

        /// <summary>
        /// Provide access to the currently selected Tenant , selected by TenantID
        /// </summary>
        public Model.Tenant SelectedTenant
        {
            get
            {
                return XeroConfig.SelectedTenant;
            }
            set
            {
                XeroConfig.SelectedTenant = value;
            }
        }


        #region Event

        public class LogMessage
        {
            public string MessageText { get; set; }
            public XeroEventStatus Status { get; set; }
        }
        public event EventHandler<StatusEventArgs> StatusUpdates;
        public class StatusEventArgs : EventArgs
        {
            public string MessageText { get; set; }
            public XeroEventStatus Status { get; set; }
        }
        public void onStatusUpdates(string message, XeroEventStatus status)
        {
            StatusEventArgs args = new StatusEventArgs() { MessageText = message, Status = status };
            StatusUpdates.SafeInvoke(this, args);
        }

        #endregion
        public API()
        {
            _authClient = new oAuth2();
            _authClient.ParentAPI = this;
            if (XeroConfig.AutoSelectTenant == null)
            {
                XeroConfig.AutoSelectTenant = true;
            }
        }
        public API(Model.XeroConfiguration config = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException("Missing XeroConfig");
            }
            if (config != null)
            {
                XeroConfig = config;
            }
            if (XeroConfig.AutoSelectTenant == null)
            {
                XeroConfig.AutoSelectTenant = true;
            }
            if (XeroConfig.codeVerifier == null)
            {
                XeroConfig.codeVerifier = GenerateCodeVerifier();
            }
            _authClient = new oAuth2();
            _authClient.ParentAPI = this;
            _authClient.XeroConfig = XeroConfig;
        }

        public void InitializeAPI()
        {
            if (XeroConfig == null)
            {
                throw new ArgumentNullException("Missing XeroConfig");
            }
            _authClient.XeroConfig = XeroConfig; // Always ensure the auth client has the XeroConfig 
            try
            {
                var task = Task.Run(() => _authClient.InitializeoAuth2());
                task.Wait();
                //        _authClient.XeroConfig = XeroConfig; // Ensure the auth client has an updated copy of the token
                onStatusUpdates("Checking Token", XeroEventStatus.Success);

                if ((XeroConfig.AutoSelectTenant.HasValue && XeroConfig.AutoSelectTenant.Value == true) || !XeroConfig.AutoSelectTenant.HasValue)
                {
                    XeroConfig.SelectedTenant = XeroConfig.XeroAPIToken.Tenants[0];
                }

                // Not Sure why I did this twice
                //task = Task.Run(() => _authClient.InitializeoAuth2(_authClient.XeroToken));
                //task.Wait();

                onStatusUpdates("Ready", XeroEventStatus.Success);
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
            }
        }
        private string GenerateCodeVerifier()
        {
            //Generate a random string for our code verifier
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);

            var codeVerifier = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return codeVerifier;
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
            onStatusUpdates("Fetch Accounts", XeroEventStatus.Log);
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetAccountsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
                task.Wait();
                if (task.Result._Accounts.Count > 0)
                {
                    onStatusUpdates("Fetch Accounts - Success", XeroEventStatus.Log);
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
        /// Retreive a single Account record 
        /// </summary>
        /// <param name="accountID">Unique identifier for the record</param>
        /// <returns>Account</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Account Account(Guid accountID)
        {
            onStatusUpdates("Fetch Account", XeroEventStatus.Log);
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetAccountAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, accountID));
                task.Wait();
                if (task.Result._Accounts.Count > 0)
                {
                    onStatusUpdates("Fetch Account - Success", XeroEventStatus.Log);
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Account CreateAccount(Xero.NetStandard.OAuth2.Model.Accounting.Account record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Account");
            }
            try
            {
                var task = Task.Run(() => xeroAPI_A.CreateAccountAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, record));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Account UpdateAccount(Xero.NetStandard.OAuth2.Model.Accounting.Account record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Account");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Account>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Accounts();
                header._Accounts = list;

                var task = Task.Run(() => xeroAPI_A.UpdateAccountAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, record.AccountID.Value, header));
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
                var task = Task.Run(() => xeroAPI_A.DeleteAccountAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, AccountID));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransaction> BankTransactions(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, int? unitdp = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                if (page == -1) page = null; // This allows a quick first page of records
                var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransaction>(); // Hold the records 
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned            
                while (count == 100)
                {
                    var task = Task.Run(() => xeroAPI_A.GetBankTransactionsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order, page, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransaction BankTransaction(Guid transactionID)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetBankTransactionAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, transactionID));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer> BankTransfers(string filter = null, string order = null, DateTime? ModifiedSince = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetBankTransfersAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer BankTransfer(Guid bankTransferID)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetBankTransferAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, bankTransferID));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer CreateBankTransfer(Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing BankTransfer");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.BankTransfers();
                header._BankTransfers = list;

                var task = Task.Run(() => xeroAPI_A.CreateBankTransferAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer CreateBankTransfers(List<Xero.NetStandard.OAuth2.Model.Accounting.BankTransfer> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException("Missing BankTransfer list");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.BankTransfers();
                header._BankTransfers = records;

                var task = Task.Run(() => xeroAPI_A.CreateBankTransferAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment BatchPayments(string filter = null, string order = null, DateTime? ModifiedSince = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetBatchPaymentsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment CreateBatchPayment(Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing BatchPayment");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.BatchPayments();
                header._BatchPayments = list;

                var task = Task.Run(() => xeroAPI_A.CreateBatchPaymentAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment> CreateBatchPayments(List<Xero.NetStandard.OAuth2.Model.Accounting.BatchPayment> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing BatchPayments list");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.BatchPayments();
                header._BatchPayments = records;

                var task = Task.Run(() => xeroAPI_A.CreateBatchPaymentAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BrandingTheme> BrandingThemes()
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetBrandingThemesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.BrandingTheme BrandingTheme(Guid brandingThemeID)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetBrandingThemeAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, brandingThemeID));
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
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned     
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => xeroAPI_A.GetContactsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order, iDs, page, includeArchived));
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
        /// Return a single contact
        /// </summary>
        /// <param name="contactID">Unique identifier for the record</param>
        /// <returns>A contact reocrd</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact GetContact(Guid contactID)
        {
            if (contactID == null)
            {
                throw new ArgumentNullException("Missing Contact ID");
            }
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetContactAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, contactID));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact CreateContact(Xero.NetStandard.OAuth2.Model.Accounting.Contact record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact ");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
                header._Contacts = list;

                var task = Task.Run(() => xeroAPI_A.CreateContactsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> CreateContacts(List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contacts List");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
                header._Contacts = records;

                var task = Task.Run(() => xeroAPI_A.CreateContactsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact UpdateContact(Xero.NetStandard.OAuth2.Model.Accounting.Contact record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
                header._Contacts = list;

                var task = Task.Run(() => xeroAPI_A.UpdateContactAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, record.ContactID.Value, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> UpdateContacts(List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contact list");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
                header._Contacts = records;

                var task = Task.Run(() => xeroAPI_A.UpdateContactAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, new Guid(), header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup> ContactGroup(string filter = null, string order = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetContactGroupsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, filter, order));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup ContactGroup(Guid contactGroupID)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetContactGroupAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, contactGroupID));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup CreateContactGroup(Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Contact Group Record ");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.ContactGroups();
                header._ContactGroups = list;

                var task = Task.Run(() => xeroAPI_A.CreateContactGroupAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup> CreateContactGroups(List<Xero.NetStandard.OAuth2.Model.Accounting.ContactGroup> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Contact Group Records ");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.ContactGroups();
                header._ContactGroups = records;

                var task = Task.Run(() => xeroAPI_A.CreateContactGroupAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
                var task = Task.Run(() => xeroAPI_A.DeleteContactGroupContactAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ContactGroupID, ContactID));
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
                var task = Task.Run(() => xeroAPI_A.DeleteContactGroupContactsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ContactGroupID));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote> CreditNotes(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, int? unitdp = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned            // If onlypage is set then the client only wants that page of records so stop processing
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => xeroAPI_A.GetCreditNotesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order, page, unitdp));
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
        /// Return a single Credit Note
        /// </summary>
        /// <param name="creditNoteID">Unique identifier for the record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns>Single Credit Note Record</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.CreditNote CreditNote(Guid creditNoteID, int? unitdp = null)
        {
            if (creditNoteID == null)
            {
                throw new ArgumentNullException("Missing Credit Note ID");
            }
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetCreditNoteAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, creditNoteID, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.CreditNote CreateCreditNote(Xero.NetStandard.OAuth2.Model.Accounting.CreditNote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Credit Note Record");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.CreditNotes();
                header._CreditNotes = list;

                var task = Task.Run(() => xeroAPI_A.CreateCreditNotesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header, null, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.CreditNote UpdateCreditNote(Xero.NetStandard.OAuth2.Model.Accounting.CreditNote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Credit Note");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.CreditNote>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.CreditNotes();
                header._CreditNotes = list;

                var task = Task.Run(() => xeroAPI_A.UpdateCreditNoteAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, record.CreditNoteID.Value, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Currency> Currencies(string filter = null, string order = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetCurrenciesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, filter, order));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Currency CreateCurrency(Xero.NetStandard.OAuth2.Model.Accounting.Currency record)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.CreateCurrencyAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, record));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> Invoices(string filter = null, string order = null, int? onlypage = null, DateTime? ModifiedSince = null, List<Guid> iDs = null, List<string> invoiceNumbers = null,
            List<Guid> contactIDs = null, List<string> statuses = null, bool? includeArchived = null, bool? createdByMyApp = null, int? unitdp = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned      
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => xeroAPI_A.GetInvoicesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order, iDs, invoiceNumbers, contactIDs, statuses, page, includeArchived, createdByMyApp, unitdp));
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
        /// Return a single Invoice Record 
        /// </summary>
        /// <param name="invoiceID">Unique identifier for the record</param>
        /// <param name="unitdp">(Unit Decimal Places) You can opt in to use four decimal places for unit amounts (optional)</param>
        /// <returns></returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice Invoice(Guid invoiceID, int? unitdp = null)
        {
            if (invoiceID == null)
            {
                throw new ArgumentNullException("Missing InvoiceID");
            }
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetInvoiceAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, invoiceID, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice CreateInvoice(Xero.NetStandard.OAuth2.Model.Accounting.Invoice record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Invoice Record");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
                header._Invoices = list;

                var task = Task.Run(() => xeroAPI_A.CreateInvoicesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header, null, unitdp));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> CreateInvoices(List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> records, int? unitdp = null)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing Invoice Records ");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
                header._Invoices = records;

                var task = Task.Run(() => xeroAPI_A.CreateInvoicesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header, null, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice UpdateInvoice(Xero.NetStandard.OAuth2.Model.Accounting.Invoice record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Invoice");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
                header._Invoices = list;

                var task = Task.Run(() => xeroAPI_A.UpdateInvoiceAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, record.InvoiceID.Value, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Item> Items(string filter = null, string order = null, int? unitdp = null, DateTime? ModifiedSince = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetItemsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Item Item(Guid itemID, int? unitdp = null)
        {
            if (itemID == null)
            {
                throw new ArgumentNullException("Missing Item ID");
            }
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetItemAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, itemID, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Item UpdateItem(Xero.NetStandard.OAuth2.Model.Accounting.Item record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Item");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Item>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Items();
                header._Items = list;

                var task = Task.Run(() => xeroAPI_A.UpdateItemAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, record.ItemID.Value, header, unitdp));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Item CreateItem(Xero.NetStandard.OAuth2.Model.Accounting.Item record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Item record");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Item>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Items();
                header._Items = list;

                var task = Task.Run(() => xeroAPI_A.CreateItemsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header, null, unitdp));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Item> CreateItems(List<Xero.NetStandard.OAuth2.Model.Accounting.Item> records, int? unitdp = null)
        {
            if (records == null)
            {
                throw new ArgumentNullException("Missing Item records");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Items();
                header._Items = records;

                var task = Task.Run(() => xeroAPI_A.CreateItemsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header, null, unitdp));
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
                var task = Task.Run(() => xeroAPI_A.DeleteItemAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, itemID));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Journal> Journals(DateTime? ModifiedSince = null, int? offset = null, bool? paymentsOnly = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetJournalsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, offset, paymentsOnly));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Organisation> Organisations()
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetOrganisationsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Quote> Quotes(string order = null, int? onlypage = null, DateTime? ModifiedSince = null, DateTime? dateFrom = null, DateTime? dateTo = null, DateTime? expiryDateFrom = null, DateTime? expiryDateTo = null, Guid? contactID = null, string status = null, string quoteNumber = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.Quote>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned             
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => xeroAPI_A.GetQuotesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, dateFrom, dateTo, expiryDateFrom, expiryDateTo, contactID, status, page, order, quoteNumber));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Quote Quote(Guid quoteID)
        {
            if (quoteID == null)
            {
                throw new ArgumentNullException("Missing QuoteID");
            }
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetQuoteAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, quoteID));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.Quote CreateQuote(Xero.NetStandard.OAuth2.Model.Accounting.Quote record, int? unitdp = null)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing Quote Record");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.Quote>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.Quotes();
                header._Quotes = list;

                var task = Task.Run(() => xeroAPI_A.CreateQuotesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate> TaxRates(string filter = null, string order = null, string taxType = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetTaxRatesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, filter, order, taxType));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.TaxRate TaxRate(string name)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetTaxRatesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, $"Name =\"{name}\""));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.TaxRate CreateTaxRate(Xero.NetStandard.OAuth2.Model.Accounting.TaxRate record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing TaxRate");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.TaxRates();
                header._TaxRates = list;

                var task = Task.Run(() => xeroAPI_A.CreateTaxRatesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate> CreateTaxRates(List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing TaxRate Records");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.TaxRates();
                header._TaxRates = records;

                var task = Task.Run(() => xeroAPI_A.CreateTaxRatesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.TaxRate UpdateTaxRate(Xero.NetStandard.OAuth2.Model.Accounting.TaxRate record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Missing TaxRate");
            }
            try
            {
                var list = new List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate>();
                list.Add(record);

                var header = new Xero.NetStandard.OAuth2.Model.Accounting.TaxRates();
                header._TaxRates = list;

                var task = Task.Run(() => xeroAPI_A.UpdateTaxRateAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate> UpdateTaxRates(List<Xero.NetStandard.OAuth2.Model.Accounting.TaxRate> records)
        {
            if (records == null || records.Count == 0)
            {
                throw new ArgumentNullException("Missing TaxRate Records");
            }
            try
            {
                var header = new Xero.NetStandard.OAuth2.Model.Accounting.TaxRates();
                header._TaxRates = records;

                var task = Task.Run(() => xeroAPI_A.UpdateTaxRateAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, header));
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
        /// <param name="filter">a filter to limit the returned records (leave empty for all records)</param>
        /// <param name="order">Order by an any element (optional)</param>
        /// <param name="includeArchived"></param>
        /// <returns>List of TrackingCategory Records</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.TrackingCategory> TrackingCategories(string filter = null, string order = null, bool? includeArchived = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetTrackingCategoriesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, filter, order, includeArchived));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.TrackingCategory TrackingCategory(Guid trackingCategoryID)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetTrackingCategoryAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, trackingCategoryID));
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.User> Users(string filter = null, DateTime? ModifiedSince = null, string order = null)
        {
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetUsersAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, filter, order));
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
        /// <returns>List of Purchase Orders</returns>
        public List<Xero.NetStandard.OAuth2.Model.Accounting.PurchaseOrder> PurchaseOrders(int? onlypage = null, string status = null, DateTime? ModifiedSince = null, string dateFrom = null, string dateTo = null, string order = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Xero.NetStandard.OAuth2.Model.Accounting.PurchaseOrder>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned  
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => xeroAPI_A.GetPurchaseOrdersAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, ModifiedSince, status, dateFrom, dateTo, order, page));
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
        public Xero.NetStandard.OAuth2.Model.Accounting.PurchaseOrder PurchaseOrder(Guid purchaseOrderID)
        {
            if (purchaseOrderID == null)
            {
                throw new ArgumentNullException("Missing PurchaseOrder ID");
            }
            try
            {
                var task = Task.Run(() => xeroAPI_A.GetPurchaseOrderAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, purchaseOrderID));
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











        #region Assets
        public List<Xero.NetStandard.OAuth2.Model.Asset.Asset> Assets(Xero.NetStandard.OAuth2.Model.Asset.AssetStatusQueryParam status, int? onlypage = null,
            string orderBy = null, string sortDirection = null, string filterBy = null)
        {
            int? page = 1;
            if (onlypage.HasValue)
            {
                page = onlypage.Value;
            }
            try
            {
                var records = new List<Xero.NetStandard.OAuth2.Model.Asset.Asset>(); // Hold the records
                int count = 100; // This is how many per page - setting this will ensure we check for the first page is a full 100 and loop until all returned  
                while (count == 100)
                {
                    if (page == -1) page = null; // This allows a quick first page of records
                    var task = Task.Run(() => xeroAPI_Assets.GetAssetsAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID, status, page, 100, orderBy, sortDirection, filterBy));
                    task.Wait();
                    records.AddRange(task.Result.Items); // Add the next page records returned
                    count = task.Result.Items.Count; // Record the number of records returned in this page. if less than 100 then the loop will exit otherwise get the next page full
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
        public List<Xero.NetStandard.OAuth2.Model.Asset.AssetType> AssetTypes()
        {
            try
            {
                var task = Task.Run(() => xeroAPI_Assets.GetAssetTypesAsync(XeroConfig.XeroAPIToken.AccessToken, XeroConfig.SelectedTenantID));
                task.Wait();
                if (task.Result.Count > 0)
                {
                    return task.Result;
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
