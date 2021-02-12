using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using XeroAuth2API.Model;

namespace XeroAuth2API
{
    internal class oAuth2
    {
        public XeroConfiguration XeroConfig { get; set; }
        public XeroTokenSet AccessTokenSet { get; set; } // Hold the active Access and Refresh tokens
        public int? Timeout { get; set; }
        public bool HasTimedout { get; set; }

        LocalHttpListener responseListener = null;
        protected internal API ParentAPI { get; set; }
        void onStatusUpdates(string Message, XeroEventStatus status)
        {
            if (this.ParentAPI != null)
            {
                ParentAPI.onStatusUpdates(Message, status);
            }
        }

        public oAuth2()
        {
            // Setup the Listener client
            responseListener = new LocalHttpListener();
            if (!Timeout.HasValue)
            {
                Timeout = 60;
            }
            HasTimedout = false;
        }

        /// <summary>
        /// Set the initial Token record before any processing, if this is a new authentication then no existing data is needed
        /// </summary>
        /// <param name="timeout">The timout duration in seconds to wait for authentication before it gives up</param>
        /// <param name="ForceReAuth">Force a re-authentication regardless of status of the token</param>
        /// <returns>The AccessToken record (refreshed version if it was expired prior)</returns>
        public void InitializeoAuth2(int? timeout = 60, bool ForceReAuth = false)
        {
            bool doAuth = false;
            if (XeroConfig == null)
            {
                throw new ArgumentNullException("Missing XeroConfig");
            }
            if (string.IsNullOrEmpty(XeroConfig.ClientID))
            {
                throw new ArgumentNullException("Missing Client ID");
            }
            if (XeroConfig.AccessTokenSet == null)
            {
                XeroConfig.AccessTokenSet = new XeroTokenSet();
            }

            Timeout = timeout;
            if (ForceReAuth)
            {
                doAuth = true;
            }
            // Check Scope change. If changed then we need to re-authenticate
            if (XeroConfig.AccessTokenSet.RequestedScopes != null && XeroConfig.Scope != XeroConfig.AccessTokenSet.RequestedScopes)
            {
                doAuth = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(XeroConfig.AccessTokenSet.RefreshToken) &&
                    (XeroConfig.AccessTokenSet.ExpiresAtUtc < DateTime.Now ||
                    XeroConfig.AccessTokenSet.ExpiresAtUtc.AddDays(59) < DateTime.Now))
                {
                    // Do a refresh
                    try
                    {
                        RefreshToken();

                        doAuth = false;
                    }
                    catch (Exception)
                    {
                        // If an error happens there was a problem with the token data
                        // Possibly app was disconnected or revoked
                        // Re-authenticate
                        doAuth = true;
                    }
                }

                // Do a new authenticate if expired (over 59 days)
                    if (string.IsNullOrEmpty(XeroConfig.AccessTokenSet.RefreshToken) ||
                    XeroConfig.AccessTokenSet.ExpiresAtUtc.AddDays(59) < DateTime.Now)
                {
                    doAuth = true;
                }
            }
            if (doAuth)
            {
                // First Revoke if token is present 
                if (!string.IsNullOrEmpty(XeroConfig.AccessTokenSet.RefreshToken))
                {
                    RevokeToken();
                }
                var task = Task.Run(() => BeginoAuth2Authentication());
                task.Wait();
            }

            onStatusUpdates("Token OK", XeroEventStatus.Success);
            return;
        }
        // Because we need to launch a browser and wait for authentications this needs to be a task so it can wait.
        async Task BeginoAuth2Authentication()
        {
            if (string.IsNullOrEmpty(XeroConfig.ClientID))
            {
                throw new ArgumentNullException("Missing Client ID");
            }
            // Raise event to the parent caller (your app)
            onStatusUpdates("Begin Authentication", XeroEventStatus.Success);

            XeroConfig.ReturnedAccessCode = null;// Ensure the Return code cleared as we are authenticating and this propery will be monitored for the completion
            XeroConfig.AccessTokenSet = new XeroTokenSet(); // Reset this token as we are authenticating so its all going to be replaced
            //start webserver to listen for the callback
            responseListener = new LocalHttpListener();
            responseListener.Message += MessageResponse;
            responseListener.callBackUri = XeroConfig.CallbackUri;
            responseListener.config = XeroConfig;
            responseListener.StartWebServer();

            //open web browser with the link generated
            System.Diagnostics.Process.Start(XeroConfig.AuthURL);

            // Fire Event so the caller can monitor
            onStatusUpdates("Login URL Opened", XeroEventStatus.Log);

            // Basically wait for 60 Seconds (should be long enough, possibly not for first time if using 2FA)
            HasTimedout = false;
            int counter = 0;
            do
            {
                await Task.Delay(1000); // Wait 1 second - gives time for response back to listener
                counter++;
            } while (responseListener.config.ReturnedAccessCode == null && counter < Timeout); // Keep waiting until a code is returned or a timeout happens

            if (counter >= Timeout)
            {
                // Raise event to the parent caller (your app)
                onStatusUpdates("Timed Out Waiting for Authentication", XeroEventStatus.Timeout);
                HasTimedout = true;
            }
            else
            {
                // Test if access was not granted
                // ReturnedAccessCode will be either a valid code or "ACCESS DENIED"
                if (responseListener.config.ReturnedAccessCode != XeroConstants.XERO_AUTH_ACCESS_DENIED)
                {
                    // Raise event to the parent caller (your app)
                    onStatusUpdates("Success", XeroEventStatus.Success);

                    //       XeroConfig = responseListener.config;// update the config with the retrieved access code data
                    ExchangeCodeForToken();
                    // Raise event to the parent caller (your app)
                    onStatusUpdates("Authentication Completed", XeroEventStatus.Success);
                }
            }
            responseListener.StopWebServer();
            // Raise event to the parent caller (your app)
            onStatusUpdates("Authentication Failed", XeroEventStatus.Failed);
        }
        private void MessageResponse(object sender, LocalHttpListener.LocalHttpListenerEventArgs e)
        {
            // Raise event to the parent caller (your app)
            onStatusUpdates(e.MessageText, XeroEventStatus.Success);
        }
        /// <summary>
        /// exchange the code for a set of tokens
        /// </summary>
        private void ExchangeCodeForToken()
        {
            try
            {
                // Raise event to the parent caller (your app)
                onStatusUpdates("Begin Code Exchange", XeroEventStatus.Success);

                using (var client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                      {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("client_id", XeroConfig.ClientID),
                        new KeyValuePair<string, string>("code", XeroConfig.ReturnedAccessCode),
                        new KeyValuePair<string, string>("redirect_uri", XeroConfig.CallbackUri.AbsoluteUri),
                        new KeyValuePair<string, string>("code_verifier", XeroConfig.codeVerifier),
                      });

                    var response = Task.Run(() => client.PostAsync(XeroConstants.XERO_TOKEN_URL, formContent)).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = Task.Run(() => response.Content.ReadAsStringAsync()).ConfigureAwait(false).GetAwaiter().GetResult();

                        // Record the token data
                        XeroConfig.AccessTokenSet = UnpackToken(content, false);
                        XeroConfig.AccessTokenSet.Tenants = null;

                        ScopesFromScopeString(); // Fix the internal Scope collection

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", XeroConfig.AccessTokenSet.AccessToken);

                        // Refresh the Authorised Tenants collection
                        var tenantsresponse = Task.Run(() => client.GetAsync(XeroConstants.XERO_TENANTS_URL)).ConfigureAwait(false).GetAwaiter().GetResult();
                        var tenantscontent = Task.Run(() => tenantsresponse.Content.ReadAsStringAsync()).ConfigureAwait(false).GetAwaiter().GetResult();

                        // Record the Available Tenants
                        XeroConfig.AccessTokenSet.Tenants = Common.DeSerializeObject<List<Tenant>>(tenantscontent);

                        // Raise event to the parent caller (your app) 
                        onStatusUpdates("Code Exchange Completed", XeroEventStatus.Success);
                    }
                    else
                    {
                        onStatusUpdates($"Begin Code Failed - {response.StatusCode}-{response.ReasonPhrase}", XeroEventStatus.Failed);
                        throw new InvalidDataException($"Code Exchange Failed - {response.StatusCode}-{response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Raise event to the parent caller (your app)
                onStatusUpdates($"Begin Code Failed - {ex.Message}", XeroEventStatus.Failed);
                throw new InvalidDataException($"Code Exchange Failed - {ex.Message}");
            }
        }
        /// <summary>
        /// Force the Config Scope list to match the returned scopes list if the <see cref="XeroConfiguration.StoreReceivedScope"/> is true
        /// </summary>
        public void ScopesFromScopeString()
        {
            if (XeroConfig.StoreReceivedScope && !string.IsNullOrEmpty(XeroConfig.AccessTokenSet.RequestedScopes))
            {
                string[] scopes = XeroConfig.AccessTokenSet.RequestedScopes.Split(' ');
                XeroConfig.Scopes = new List<XeroScope>();

                foreach (var scopeItem in scopes)
                {
                    string scopename = scopeItem;
                    scopename = scopeItem.Replace(".", "_"); // Replace . with _ to match the scopes
                    // Find the Scope Enum that matches the scopename string
                    foreach (XeroScope item in (XeroScope[])Enum.GetValues(typeof(XeroScope)))
                    {
                        string name = Enum.GetName(typeof(XeroScope), item);
                        if (scopename == name)
                        {
                            XeroConfig.AddScope(item);
                        }
                    }

                }
                // Now set the Requested string list to match the new order
                XeroConfig.AccessTokenSet.RequestedScopes = XeroConfig.Scope;
            }
        }
        /// <summary>
        /// Revoke the Access Token and disconnect the tenants from the user
        /// </summary>        
        public void RevokeToken()
        {
            var client = new HttpClient();

            var response = Task.Run(() => client.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = "https://identity.xero.com/connect/revocation",
                ClientId = XeroConfig.ClientID,
                //ClientSecret = XeroConfig.ClientSecret,
                Token = XeroConfig.AccessTokenSet.RefreshToken
            }));
            response.Wait();

            if (response.Result.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(response.Result.Exception.Message);
            }
            XeroConfig.AccessTokenSet = new XeroTokenSet(); // Remove it as its no longer valid

        }
        public void RefreshToken()
        {
            onStatusUpdates("Begin Token Refresh", XeroEventStatus.Success);
            try
            {
                var client = new HttpClient();
                var formContent = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", XeroConfig.ClientID),
                new KeyValuePair<string, string>("refresh_token", XeroConfig.AccessTokenSet.RefreshToken),
            });

                var response = Task.Run(() => client.PostAsync(XeroConstants.XERO_TOKEN_URL, formContent)).ConfigureAwait(false).GetAwaiter().GetResult();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = Task.Run(() => response.Content.ReadAsStringAsync()).ConfigureAwait(false).GetAwaiter().GetResult();

                    // Unpack the response tokens
                    if (content.Contains("error"))
                    {
                        throw new Exception(content);
                    }

                    XeroConfig.AccessTokenSet = UnpackToken(content, true);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", XeroConfig.AccessTokenSet.AccessToken);

                    // Refresh the Authorised Tenants collection
                    var tenantsresponse = Task.Run(() => client.GetAsync(XeroConstants.XERO_TENANTS_URL)).ConfigureAwait(false).GetAwaiter().GetResult();
                    var tenantscontent = Task.Run(() => tenantsresponse.Content.ReadAsStringAsync()).ConfigureAwait(false).GetAwaiter().GetResult();

                    // Record the Available Tenants
                    XeroConfig.AccessTokenSet.Tenants = Common.DeSerializeObject<List<Tenant>>(tenantscontent);

                    onStatusUpdates("Token Refresh Success", XeroEventStatus.Refreshed);
                }
                else
                {
                    // Something didnt work - disconnected/revoked?
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                // Raise event to the parent caller (your app)
                onStatusUpdates($"Refresh Code Failed - {ex.Message}", XeroEventStatus.Failed);
                throw new InvalidDataException($"Refresh Exchange Failed - {ex.Message}");
            }
        }
        /// <summary>
        /// Unpack the token data from the API Authentication or Refresh calls
        /// </summary>
        /// <param name="content">reponse string containing the data </param>
        /// <param name="isRefresh">Property used to know if the Unpack is from the refresh</param>
        /// <returns></returns>
        private XeroTokenSet UnpackToken(string content, bool isRefresh)
        {
            // Record the token data
            var tokens = JObject.Parse(content);

            XeroTokenSet newToken = new XeroTokenSet();

            newToken.IdToken = tokens["id_token"]?.ToString();
            newToken.AccessToken = tokens["access_token"]?.ToString();
            newToken.ExpiresAtUtc = DateTime.Now.AddSeconds(int.Parse(tokens["expires_in"]?.ToString()));
            newToken.RefreshToken = tokens["refresh_token"]?.ToString();
            if (!isRefresh)
            {
                // Only bother with this if its not a refresh
                if (XeroConfig.StoreReceivedScope)
                {
                    newToken.RequestedScopes = tokens["scope"]?.ToString(); // Ensure we record the scope used
                }
                else
                {
                    newToken.RequestedScopes = XeroConfig.Scope;
                }
            }
            else
            {
                // Ensure the scopes list is left intact!
                newToken.RequestedScopes = XeroConfig.AccessTokenSet.RequestedScopes;
            }
            return newToken;
        }

    }
}
