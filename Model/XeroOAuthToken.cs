using System;
using System.Collections.Generic;
using System.Text;

namespace XeroAuth2API.Model
{
    public class XeroOAuthToken 
    {
        public List<Tenant> Tenants { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

    }
}
