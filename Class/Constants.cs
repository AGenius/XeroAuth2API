using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    public static class XeroConstants
    {
        public const string XERO_TOKEN_URL = "https://identity.xero.com/connect/token";
        public const string XERO_TENANTS_URL = "https://api.xero.com/connections";
        public const string XERO_AUTH_URL = "https://login.xero.com/identity/connect/authorize?";

        public const string XERO_AUTH_ACCESS_DENIED = "ACCESS DENIED";
        public const string XERO_AUTH_ACCESS_GRANTED = "ACCESS GRANTED";
        public const string XERO_AUTH_ACCESS_DENIED_HTML = "<h1>ACCESS DENIED</h1><h2>Xero access Authentication is completed.</h2><p>You may now close this window.</p>";
        public const string XERO_AUTH_ACCESS_GRANTED_HTML = "<h1>ACCESS GRANTED</h1><h2>Xero access Authentication is completed.</h2><p>You may now close this window.</p>";


    }
}
