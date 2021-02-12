using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    /// <summary>
    /// Constants for the API wrapper
    /// </summary>
    public static class XeroConstants
    {
        /// <summary>The endpoint URL of the Xero Token exchange</summary>
        public const string XERO_TOKEN_URL = "https://identity.xero.com/connect/token";
        /// <summary>The endpoint URL of the Xero Tenants access</summary>
        public const string XERO_TENANTS_URL = "https://api.xero.com/connections";
        /// <summary>The endpoint for the OAuth2 authentication process</summary>
        public const string XERO_AUTH_URL = "https://login.xero.com/identity/connect/authorize?";

        /// <summary>Return value for Access Denied</summary>
        public const string XERO_AUTH_ACCESS_DENIED = "ACCESS DENIED";
        /// <summary>Return value for Access Granted</summary>
        public const string XERO_AUTH_ACCESS_GRANTED = "ACCESS GRANTED";
        /// <summary>XML Content for Access Denied</summary>
        public const string XERO_AUTH_ACCESS_DENIED_HTML = "<h1>ACCESS DENIED</h1><h2>Xero access Authentication is completed.</h2><p>You may now close this window.</p>";
        /// <summary>XML Content for Access Denied</summary>
        public const string XERO_AUTH_ACCESS_GRANTED_HTML = "<h1>ACCESS GRANTED</h1><h2>Xero access Authentication is completed.</h2><p>You may now close this window.</p>";


    }
}
