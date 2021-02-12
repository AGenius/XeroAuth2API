using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace XeroAuth2API.Model
{
    /// <summary>
    /// Holds the Decoded IdToken JWT set
    /// </summary>
    public class JWTIDToken
    {
        /// <summary></summary>
        public long nbf { get; set; }
        /// <summary></summary>
        public DateTime? nbf_DateTime
        {
            get
            {
                return Common.DateTimeFromUnixTime(nbf);
            }
        }
        /// <summary></summary>
        public long exp { get; set; }
        /// <summary></summary>
        public DateTime exp_DateTime
        {
            get
            {
                return Common.DateTimeFromUnixTime(exp);
            }
        }
        /// <summary></summary>
        public string iss { get; set; }
        /// <summary></summary>
        public string aud { get; set; }
        /// <summary></summary>
        public long iat { get; set; }
        /// <summary></summary>
        public string at_hash { get; set; }
        /// <summary></summary>
        public string sub { get; set; }
        /// <summary></summary>
        public long auth_time { get; set; }
        /// <summary></summary>
        public DateTime auth_time_DateTime
        {
            get
            {
                return Common.DateTimeFromUnixTime(auth_time);
            }
        }
        /// <summary>
        /// The Xero User ID
        /// </summary>
        public Guid? xero_userid { get; set; }
        /// <summary>
        /// Global Session ID
        /// </summary>
        public string global_session_id { get; set; }
        /// <summary>
        /// The Xero Username used
        /// </summary>
        [JsonProperty("preferred_username")]
        public string Username { get; set; }
        /// <summary>
        /// The users Email Address
        /// </summary>
        [JsonProperty("Email")]
        public string EmailAddress { get; set; }
        /// <summary>
        /// The users First Name
        /// </summary>
        [JsonProperty("given_name")]
        public string FirstName { get; set; }
        /// <summary>
        /// The users Surname
        /// </summary>
        [JsonProperty("family_name")]
        public string Surname { get; set; }
    }
}
