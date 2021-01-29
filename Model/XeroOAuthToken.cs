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
        /// <summary>
        /// List of connected tenants
        /// </summary>
        public List<Tenant> Tenants { get; set; }
        /// <summary>
        /// The AccessToken used for API calls
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// The Refresh token required to refresh the AccessToken
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// ?
        /// </summary>
        public string IdToken { get; set; }
        /// <summary>
        /// When the Access Token will expire
        /// </summary>
        public DateTime ExpiresAtUtc { get; set; }
        /// <summary>
        /// Record the Scope used. If the scope is changed on a refresh then force a re-authentication
        /// </summary>
        public string RequestedScopes { get; set; }

    }
}
