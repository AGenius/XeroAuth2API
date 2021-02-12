using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using XeroAuth2API.Model;
using System.Linq;

namespace XeroAuth2API
{
    /// <summary>
    /// This is the Core of the API Wrapper. this class holds the configurations, the sub Api endpoints and events
    /// </summary>
    public class API
    {
        /// <summary>
        /// The API Wrapper version
        /// </summary>
        public string Version
        {
            get
            {
                return "1.2021.0208,Xero-Standard: 3.14.2";
            }
        }
        oAuth2 _authClient = null;

        /// <summary>
        /// The Configuration object that holds all the magic info needed!
        /// </summary>
        public XeroConfiguration XeroConfig { get; set; }
        /// <summary>
        /// When the API is initialized and the Token is refreshed or the API is authenticated 
        /// this will be set to true.
        /// Use this property to determine if the auth/refresh is all ok
        /// </summary>
        public bool isConnected { get; set; }
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
        public List<Tenant> Tenants
        {
            get
            {
                if (XeroConfig != null && XeroConfig.AccessTokenSet != null)
                {
                    return XeroConfig.AccessTokenSet.Tenants;
                }
                return null;
            }
        }

        /// <summary>
        /// Provide access to the currently selected Tenant , selected by TenantID
        /// </summary>
        public Tenant SelectedTenant
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



        // Setup the sub API objects
        /// <summary>Exposes the AccountingApi Object</summary>
        public Api.AccountingApi AccountingApi;
        /// <summary>Exposes the AssetApi Object</summary>
        public Api.AssetApi AssetApi;
        /// <summary>Exposes the BankFeedsApi Object</summary>
        public Api.BankFeedsApi BankFeedsApi;
        /// <summary>Exposes the PayrollAuApi Object</summary>
        public Api.PayrollAuApi PayrollAuApi;
        /// <summary>Exposes the PayrollNzApi Object</summary>
        public Api.PayrollNzApi PayrollNzApi;
        /// <summary>Exposes the PayrollUkApi Object</summary>
        public Api.PayrollUkApi PayrollUkApi;
        /// <summary>Exposes the IdentityApi Object</summary>
        public Api.IdentityApi IdentityApi;
        /// <summary>Exposes the ProjectApi Object</summary>
        public Api.ProjectApi ProjectApi;


        #region Event
        /// <summary>
        /// Loggin message Event
        /// </summary>
        public class LogMessage
        {
            /// <summary>
            /// The message
            /// </summary>
            public string MessageText { get; set; }
            /// <summary>
            /// The status
            /// </summary>            
            public XeroEventStatus Status { get; set; }
        }
        /// <summary></summary>
        public event EventHandler<StatusEventArgs> StatusUpdates;
        /// <summary></summary>
        public class StatusEventArgs : EventArgs
        {
            public string MessageText { get; set; }
            public XeroEventStatus Status { get; set; }
        }
        /// <summary>Fire the Status update Event</summary>
        internal void onStatusUpdates(string message, XeroEventStatus status)
        {
            StatusEventArgs args = new StatusEventArgs() { MessageText = message, Status = status };
            StatusUpdates.SafeInvoke(this, args);
        }

        #endregion
        /// <summary>Default constructor, will setup the defaults required.</summary>
        public API()
        {
            _authClient = new oAuth2();
            _authClient.ParentAPI = this;
            //
            AccountingApi = new Api.AccountingApi(this);
            AssetApi = new Api.AssetApi(this);
            BankFeedsApi = new Api.BankFeedsApi(this);
            PayrollAuApi = new Api.PayrollAuApi(this);
            PayrollNzApi = new Api.PayrollNzApi(this);
            PayrollUkApi = new Api.PayrollUkApi(this);
            IdentityApi = new Api.IdentityApi(this);
            ProjectApi = new Api.ProjectApi(this);
            isConnected = false;
        }
        /// <summary>Instantiate the API with a Configuration record already setup</summary>
        /// <param name="config">The configuration record to use <see cref="XeroConfiguration"/></param>
        public API(XeroConfiguration config = null)
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
            // Setup the reference to the core wrapper
            AccountingApi = new Api.AccountingApi(this);
            AssetApi = new Api.AssetApi(this);
            BankFeedsApi = new Api.BankFeedsApi(this);
            PayrollAuApi = new Api.PayrollAuApi(this);
            PayrollNzApi = new Api.PayrollNzApi(this);
            PayrollUkApi = new Api.PayrollUkApi(this);
            IdentityApi = new Api.IdentityApi(this);
            ProjectApi = new Api.ProjectApi(this);
            isConnected = false;
        }
        /// <summary>
        /// Setup the API and refresh token or re-authorise if needed/requested
        /// </summary>
        /// <param name="timeout">set a timeout to wait for authentication (default is 60 seconds)</param>
        /// <param name="ForceReAuth">This will force the Auth login again-Needed if you want to add a new tenant</param>
        /// <exception cref="TimeoutException">If the Auth process times out an exception will be raised.</exception>
        public void InitializeAPI(int? timeout = 60, bool ForceReAuth = false)
        {
            isConnected = false;
            if (timeout == null)
            {
                // Ensure if null passed then default to 60 seconds
                timeout = 60;
            }
            if (XeroConfig == null)
            {
                throw new ArgumentNullException("Missing XeroConfig");
            }

            // Always ensure the stored Returned scope string is in sorted order 
            if (XeroConfig.AccessTokenSet != null && !string.IsNullOrEmpty(XeroConfig.AccessTokenSet.RequestedScopes))
            {
                XeroConfig.AccessTokenSet.RequestedScopes = string.Join(" ", XeroConfig.AccessTokenSet.RequestedScopes.Split(' ').OrderBy(x => x).ToArray());
            }

            _authClient.XeroConfig = XeroConfig; // Always ensure the auth client has the XeroConfig             
            try
            {
                var task = Task.Run(() => _authClient.InitializeoAuth2(timeout, ForceReAuth));
                task.Wait();
                if (_authClient.HasTimedout)
                {
                    onStatusUpdates("Timed Out Waiting for Authentication", XeroEventStatus.Timeout);
                    throw new TimeoutException("Timed Out Waiting for Authentication");
                }
                onStatusUpdates("Checking Token", XeroEventStatus.Success);

                if (XeroConfig.SelectedTenant == null)
                {
                    if ((XeroConfig.AutoSelectTenant.HasValue && XeroConfig.AutoSelectTenant.Value == true) || !XeroConfig.AutoSelectTenant.HasValue)
                    {
                        XeroConfig.SelectedTenant = XeroConfig.AccessTokenSet.Tenants[0];
                    }
                }
                onStatusUpdates("Ready", XeroEventStatus.Success);
                isConnected = true;
            }
            catch (Exception ex)
            {
                var er = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
                if (er != null)
                {
                    throw new Xero.NetStandard.OAuth2.Client.ApiException(er.ErrorCode, er.Message, er.ErrorContent);
                }
                if (er == null && ex.InnerException != null)
                {
                    throw new Exception(ex.InnerException.Message);
                }
                throw;
            }
        }
        /// <summary>
        /// Revoke the Access Token to invalidate the token used 
        /// </summary>
        public void RevokeAuth()
        {
            _authClient.RevokeToken();
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

    }
}
