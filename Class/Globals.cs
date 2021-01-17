using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

/// <summary>
///  This is used so the internal listener has somewhere to place the data for the main process to access
/// </summary>
namespace XeroAuth2API
{
    static class Globals
    {
        public static XeroAccessCode returnedCode { get; set; } // Hold the First leg of the oAuth2 process
        public static Model.XeroOAuthToken XeroToken { get; set; } // Hold the Final Access and Refresh tokens
    }
    /// <summary>
    /// The Access Code object - this object will hold the initial code sent from Xero when authenticating
    /// </summary>
    class XeroAccessCode
    {
        public string authCode { get; set; }
        public string stateCode { get; set; }
        public string codeVerifier { get; set; }
        public string xeroClientID { get; set; }
        public Uri xeroCallbackUri { get; set; }
        public string xeroScope { get; set; }
        public string xeroState { get; set; }
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

                    return "https://login.xero.com/identity/connect/authorize?" + $"response_type=code&client_id={xeroClientID}&redirect_uri={xeroCallbackUri.AbsoluteUri}&scope={xeroScope}&state={xeroState}&code_challenge={codeChallenge}&code_challenge_method=S256";
                }
                return string.Empty;
            }
        }
    }
}
