using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XeroAuth2API.Model
{
    public class XeroConfiguration
    {
        public string ReturnedAccessCode { get; set; }
        public string ReturnedState { get; set; }
        public string codeVerifier { get; set; }
        public string ClientID { get; set; }
        public Uri CallbackUri { get; set; }
        public string Scope { get; set; }
        public string State { get; set; }
        public string AuthURL
        {
            get
            {
                if (!string.IsNullOrEmpty(codeVerifier))
                {
                    string codeChallenge;
                    using (var sha256 = SHA256.Create())
                    {
                        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                        codeChallenge = Convert.ToBase64String(challengeBytes)
                            .TrimEnd('=')
                            .Replace('+', '-')
                            .Replace('/', '_');
                    }

                    return "https://login.xero.com/identity/connect/authorize?" + $"response_type=code&client_id={ClientID}&redirect_uri={CallbackUri.AbsoluteUri}&scope={Scope}&state={State}&code_challenge={codeChallenge}&code_challenge_method=S256";
                }
                return string.Empty;
            }
        }
    }
}
