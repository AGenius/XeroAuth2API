﻿using System;
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
        /// <summary>
        /// Returns the URL to authenticate with Xero
        /// </summary>
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
                    string url = $"{XeroURLS.XERO_AUTH_URL}response_type=code&client_id={ClientID}&redirect_uri={CallbackUri.AbsoluteUri}&scope={Scope}&code_challenge={codeChallenge}&code_challenge_method=S256";
                    if (!string.IsNullOrEmpty(State))
                    {
                        return $"{url}&state={State}";
                    }

                    return url;

                }
                return string.Empty;
            }
        }
    }
}
