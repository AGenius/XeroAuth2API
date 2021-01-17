using System;
using System.Collections.Generic;
using System.Text;

namespace XeroAuth2API.Model
{
    public interface IXeroToken
    {
        List<Tenant> Tenants { get; set; }
        string AccessToken { get; set; }
        string RefreshToken { get; set; }
        string IdToken { get; set; }
        DateTime ExpiresAtUtc { get; set; }
    }
}
