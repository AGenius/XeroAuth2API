using System;
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
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Account> Accounts(string filter = null, DateTime? ifModifiedSince = null, string order = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetAccountsAsync(AccessToken, TenantID, ifModifiedSince, filter, order));
            task.Wait();
            return task.Result._Accounts;
        }
        public Xero.NetStandard.OAuth2.Model.Accounting.Account GetAccount(Guid accountID)
        {
            var task = Task.Run(() => xeroAPI_A.GetAccountAsync(AccessToken, TenantID, accountID));
            task.Wait();
            if (task.Result._Accounts.Count > 0)
            {
                return task.Result._Accounts[0];
            }
            return null;
        }

        #endregion
        #region Invoices
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> Invoices(string filter = null, DateTime? ifModifiedSince = null, string order = null, List<Guid> iDs = null, List<string> invoiceNumbers = null,
            List<Guid> contactIDs = null, List<string> statuses = null, int? page = null, bool? includeArchived = null, bool? createdByMyApp = null, int? unitdp = null)
        {
            var task = Task.Run(() => xeroAPI_A.GetInvoicesAsync(AccessToken, TenantID, ifModifiedSince, filter, order, iDs, invoiceNumbers, contactIDs, statuses, page, includeArchived, createdByMyApp, unitdp));
            task.Wait();

            return task.Result._Invoices;
        }
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice Invoice(Guid invoiceID)
        {
            var task = Task.Run(() => xeroAPI_A.GetInvoiceAsync(AccessToken, TenantID, invoiceID));
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
        /// <param name="invoice"></param>
        /// <returns>Created Invoice</returns>
        public Xero.NetStandard.OAuth2.Model.Accounting.Invoice CreateInvoice(Xero.NetStandard.OAuth2.Model.Accounting.Invoice invoice)
        {
            var invoiceList = new List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice>();
            invoiceList.Add(invoice);

            var invoices = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
            invoices._Invoices = invoiceList;

            var task = Task.Run(() => xeroAPI_A.CreateInvoicesAsync(AccessToken, TenantID, invoices));
            task.Wait();
            if (task.Result._Invoices.Count > 0)
            {
                return task.Result._Invoices[0];
            }
            return null;
        }
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> CreateInvoice(List<Xero.NetStandard.OAuth2.Model.Accounting.Invoice> invoices)
        {
            var invoicesList = new Xero.NetStandard.OAuth2.Model.Accounting.Invoices();
            invoicesList._Invoices = invoices;

            var task = Task.Run(() => xeroAPI_A.CreateInvoicesAsync(AccessToken, TenantID, invoicesList));
            task.Wait();
            if (task.Result._Invoices.Count > 0)
            {
                return task.Result._Invoices;
            }
            return null;
        }
        #endregion

        #region BrandingThemes
        public List<Xero.NetStandard.OAuth2.Model.Accounting.BrandingTheme> GetBrandingThemes()
        {
            var task = Task.Run(() => xeroAPI_A.GetBrandingThemesAsync(AccessToken, TenantID));
            task.Wait();
            if (task.Result._BrandingThemes.Count > 0)
            {
                return task.Result._BrandingThemes;
            }
            return null;
        }
        public Xero.NetStandard.OAuth2.Model.Accounting.BrandingTheme GetBrandingTheme(Guid brandingThemeID)
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
        #region Contacts
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> GetContacts()
        {
            var task = Task.Run(() => xeroAPI_A.GetContactsAsync(AccessToken, TenantID));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts;
            }
            return null;
        }
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact GetContact(Guid contactID)
        {
            var task = Task.Run(() => xeroAPI_A.GetContactAsync(AccessToken, TenantID, contactID));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts[0];
            }
            return null;
        }
        public Xero.NetStandard.OAuth2.Model.Accounting.Contact CreateContact(Xero.NetStandard.OAuth2.Model.Accounting.Contact contact)
        {
            var contactsList = new List<Xero.NetStandard.OAuth2.Model.Accounting.Contact>();
            contactsList.Add(contact);

            var contacts = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
            contacts._Contacts = contactsList;

            var task = Task.Run(() => xeroAPI_A.CreateContactsAsync(AccessToken, TenantID, contacts));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts[0];
            }
            return null;
        }
        public List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> CreateContacts(List<Xero.NetStandard.OAuth2.Model.Accounting.Contact> contact)
        {
            var contactsList = new Xero.NetStandard.OAuth2.Model.Accounting.Contacts();
            contactsList._Contacts = contact;

            var task = Task.Run(() => xeroAPI_A.CreateContactsAsync(AccessToken, TenantID, contactsList));
            task.Wait();
            if (task.Result._Contacts.Count > 0)
            {
                return task.Result._Contacts;
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
