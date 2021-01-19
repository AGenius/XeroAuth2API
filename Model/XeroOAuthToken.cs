using System;
using System.Collections.Generic;
using System.Text;

namespace XeroAuth2API.Model
{
    /// <summary>
    /// Hold the Auth token
    /// </summary>
    public class XeroAccessToken
    {
        public List<Tenant> Tenants { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        /// <summary>
        /// Record the Scope used. If the scope is changed on a refresh then force a re-authentication
        /// </summary>
        public string RequestedScopes { get; set; }

    }
}
