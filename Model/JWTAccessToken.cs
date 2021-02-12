using System;
using System.Collections.Generic;

namespace XeroAuth2API.Model
{
    /// <summary>
    /// Holds the Decoded AccessToken JWT set
    /// </summary>
    public class JWTAccessToken
    {
        /// <summary>
        /// Unix Epoch Timestamp - 
        /// </summary>
        public long nbf { get; set; }
        /// <summary>
        /// Unix Epoch Timestamp converted to DateTime
        /// </summary>
        public DateTime? nbf_DateTime
        {
            get
            {
                return Common.DateTimeFromUnixTime(nbf);
            }
        }
        /// <summary>
        /// Unix Epoch TimeStamp - Expiry?
        /// </summary>
        public long exp { get; set; }
        /// <summary>
        /// Unix Epoch Timestamp converted to DateTime
        /// </summary>
        public DateTime? exp_DateTime
        {
            get
            {
                return Common.DateTimeFromUnixTime(exp);
            }
        }
        /// <summary>
        /// Issued from 
        /// </summary>
        public string iss { get; set; }
        /// <summary>
        /// ?
        /// </summary>
        public string aud { get; set; }
        /// <summary>
        /// Issued when you create your Zero app and should match your client ID
        /// </summary>
        public string client_id { get; set; }
        /// <summary>
        /// ?
        /// </summary>
        public string sub { get; set; }
        /// <summary>
        /// Unix Epoch TimeStamp - The first time Authentication was completed.
        /// </summary>
        public long auth_time { get; set; }
        /// <summary>
        /// Unix Epoch TimeStamp converted to DateTime
        /// </summary>
        public DateTime? auth_time_DateTime
        {
            get
            {
                return Common.DateTimeFromUnixTime(auth_time);
            }
        }
        /// <summary>
        /// The Xero UserId of the Authenticated User
        /// </summary>
        public Guid? xero_userid { get; set; }
        /// <summary>
        /// ?
        /// </summary>
        public string global_session_id { get; set; }
        /// <summary>
        /// ?
        /// </summary>
        public string jti { get; set; }
        /// <summary>
        /// The ID should match a tenant in the list of Authenticated tenants
        /// </summary>
        /// <remarks>Does not appear to after a refresh</remarks>
        public Guid authentication_event_id { get; set; }
        /// <summary>
        /// List of Scopes the user has
        /// </summary>
        public List<string> scope { get; set; }
    }
}
