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
        /// <summary>
        /// Holds the Returned Access Code from the Authentication step that the AccessToken exchange needs
        /// </summary>
        public string ReturnedAccessCode { get; set; }
        /// <summary>
        /// Holds the Returned state value provided when Authenticating. this should match the State sctring provided
        /// Not needed for local desktop application
        /// </summary>
        public string ReturnedState { get; set; }
        /// <summary>
        /// Holds the Live AccessToken
        /// </summary>
        public XeroAccessToken XeroAPIToken { get; set; }
        public string codeVerifier { get; set; }
        /// <summary>
        /// Issued when you create your Zero app
        /// </summary>
        public string ClientID { get; set; }
        /// <summary>
        /// The URL on your server to redirect back to - should be http://localhost:port/name
        /// where "port" is any valid port e.g. 8888 and "name" is something like "callback" - http://localhost:8888/callback
        /// </summary>
        public Uri CallbackUri { get; set; }

        private List<XeroScope> _scopes { get; set; } // Hold the list
        /// <summary>
        /// List of Scopes the API would like to use
        /// If a call to any API method does not have the required scope at time of authentication
        /// an APIAception will be thrown
        /// </summary>
        public List<XeroScope> Scopes
        {
            get
            {
                if (_scopes == null)
                {
                    _scopes = new List<XeroScope>();
                }

                return _scopes;
            }
            set
            {
                _scopes = value;

                if (!_scopes.Contains(XeroScope.openid))
                {
                    AddScope(XeroScope.openid); // Ensure its in there
                }
                if (!_scopes.Contains(XeroScope.profile))
                {
                    AddScope(XeroScope.profile); // Ensure its in there
                }
                if (!_scopes.Contains(XeroScope.offline_access))
                {
                    AddScope(XeroScope.offline_access); // Ensure its in there                   
                }
            }
        }
        /// <summary>
        /// Add a scope to the required scopes when authenticating
        /// </summary>
        /// <param name="scope"></param>
        public void AddScope(XeroScope scope)
        {
            switch (scope)
            {
                case XeroScope.all:
                    foreach (XeroScope item in (XeroScope[])Enum.GetValues(typeof(XeroScope)))
                    {
                        string name = Enum.GetName(typeof(XeroScope), item);
                        if (!name.EndsWith("_read") && !name.Contains("all"))
                        {
                            AddScope(item);
                        }
                    }
                    break;
                case XeroScope.all_read:
                    foreach (XeroScope item in (XeroScope[])Enum.GetValues(typeof(XeroScope)))
                    {
                        string name = Enum.GetName(typeof(XeroScope), item);
                        if (item != XeroScope.all_read && name.EndsWith("_read") && !name.Contains("all"))
                        {
                            AddScope(item);
                        }
                    }
                    break;

                case XeroScope.accounting_all:

                    foreach (XeroScope item in (XeroScope[])Enum.GetValues(typeof(XeroScope)))
                    {
                        string name = Enum.GetName(typeof(XeroScope), item);
                        if (name.StartsWith("accounting") && !name.EndsWith("_read") && !name.Contains("all"))
                        {
                            AddScope(item);
                        }
                    }
                    break;
                case XeroScope.accounting_all_read:
                    foreach (XeroScope item in (XeroScope[])Enum.GetValues(typeof(XeroScope)))
                    {
                        string name = Enum.GetName(typeof(XeroScope), item);
                        if (name.StartsWith("accounting") && name.EndsWith("_read") && !name.Contains("all"))
                        {
                            AddScope(item);
                        }
                    }
                    break;



                default:
                    // Add any not already in list
                    if (!Scopes.Contains(scope))
                    {
                        Scopes.Add(scope);
                    }
                    break;
            }


        }
        /// <summary>
        /// String representation of the list of scopes selected
        /// </summary>
        public string Scope
        {
            get
            {
                if (!_scopes.Contains(XeroScope.openid))
                {
                    AddScope(XeroScope.openid); // Ensure its in there
                }
                if (!_scopes.Contains(XeroScope.profile))
                {
                    AddScope(XeroScope.profile); // Ensure its in there
                }
                if (!_scopes.Contains(XeroScope.offline_access))
                {
                    AddScope(XeroScope.offline_access); // Ensure its in there                   
                }

                string scopelist = string.Empty;
                foreach (var item in Scopes)
                {
                    if (!string.IsNullOrEmpty(scopelist))
                    {
                        scopelist += " ";
                    }
                    if (item == XeroScope.offline_access)
                    {
                        // Dont add now                        
                    }
                    else
                    {
                        scopelist += item.ToString().Replace("_", ".");
                    }

                }
                // To ensure offline_access is at the end of the scope list
                scopelist += " offline_access";
                return scopelist;
            }
        }
        /// <summary>
        /// a unique string to be passed back on completion (optional) 
        /// The state parameter should be used to avoid forgery attacks. Pass in a value that's unique to the user you're sending through authorisation. It will be passed back after the user completes authorisation.
        /// Generally not required for a Desktop application
        /// </summary>
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
                    string url = $"{XeroConstants.XERO_AUTH_URL}response_type=code&client_id={ClientID}&redirect_uri={CallbackUri.AbsoluteUri}&scope={Scope.Replace(" ", "%20")}&code_challenge={codeChallenge}&code_challenge_method=S256";
                    if (!string.IsNullOrEmpty(State))
                    {
                        return $"{url}&state={State}";
                    }

                    return url;

                }
                return string.Empty;
            }
        }

        /// <summary>
        /// If provided, the API setup will try and match the name with the correct tenant otherwise the first tenant will be selected
        /// </summary>
        public string SelectedTenantName
        {
            get
            {
                if (SelectedTenant != null)
                {
                    return SelectedTenant.TenantName;
                }
                return null;
            }
        }
        public string SelectedTenantID
        {
            get
            {
                if (SelectedTenant != null)
                {
                    return SelectedTenant.TenantId.ToString();
                }
                return null;
            }
        }// The Tenant ID to use for API calls
        public Tenant SelectedTenant { get; set; }// The Tenant  
        public bool StoreReceivedScope { get; set; }
        public bool? AutoSelectTenant { get; set; }

    }
}
