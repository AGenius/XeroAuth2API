using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace XeroAuth2API.Model
{
    /// <summary>
    /// Hold the Auth token
    /// </summary>
    public class XeroTokenSet
    {

        /// <summary>
        /// The AccessToken used for API calls
        /// </summary>
        [JsonProperty]
        public string AccessToken { get; internal set; }
        /// <summary>
        /// The Refresh token required to refresh the AccessToken
        /// </summary>
        [JsonProperty]
        public string RefreshToken { get; internal set; }
        /// <summary>
        /// User ID
        /// </summary>
        [JsonProperty]
        public string IdToken { get; internal set; }
        /// <summary>
        /// When the Access Token will expire
        /// </summary>
        [JsonProperty]
        public DateTime ExpiresAtUtc { get; internal set; }
        /// <summary>
        /// Record the Scope used. If the scope is changed on a refresh then force a re-authentication
        /// </summary>
        [JsonProperty]
        public string RequestedScopes { get; internal set; }
        /// <summary>
        /// JWT Decoded object holding the Access Token record
        /// </summary>
        [JsonProperty]
        public JWTAccessToken AccessTokenRecord
        {
            get
            {
                // Unpack the JWT Token
                if (!string.IsNullOrEmpty(AccessToken))
                {
                    return Common.DeSerializeObject<JWTAccessToken>(Common.JWTtoJSON(AccessToken));
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// JWT Decoded object holding the ID Token record
        /// </summary>
        [JsonProperty]
        public JWTIDToken IDTokenRecord
        {
            get
            {
                // Unpack the JWT Token
                if (!string.IsNullOrEmpty(IdToken))
                {
                    return Common.DeSerializeObject<JWTIDToken>(Common.JWTtoJSON(IdToken));
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// List of authorised tenants
        /// </summary>
        [JsonProperty]
        public List<Tenant> Tenants { get; internal set; }

    }
}
