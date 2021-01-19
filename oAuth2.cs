using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    public class oAuth2
    {
        public Model.XeroConfiguration XeroConfig { get; set; }// Hold the First leg of the oAuth2 process
        public Model.XeroOAuthToken XeroToken { get; set; } // Hold the Final Access and Refresh tokens
        public int? Timeout { get; set; }

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
        }

        /// <summary>
        /// Set the initial Token record before any processing, if this is a new authentication then no existing data is needed
        /// </summary>
        /// <param name="TokenData"></param>
        /// <returns>The AccessToken record (refreshed version if it was expired prior)</returns>
        public Model.XeroOAuthToken InitializeoAuth2(Model.XeroOAuthToken TokenData = null, int? timeout = 60)
        {
            if (TokenData != null)
            {
                XeroToken = TokenData;
            }
            Timeout = timeout;

            // Check Scope change. If changed then we need to re-authenticate
            if (XeroConfig.Scope != XeroToken.Scope)
            {
                var task = Task.Run(() => BeginoAuth2Authentication());
                task.Wait();
                XeroToken = task.Result; // Set the internal copy of the Token

                return XeroToken; // Return the resulting token
            }
            else
            {
                if (!string.IsNullOrEmpty(TokenData.RefreshToken) && (TokenData.ExpiresAtUtc < DateTime.Now || TokenData.ExpiresAtUtc.AddDays(59) < DateTime.Now))
                {
                    // Do a refresh
                    return RefreshToken(TokenData);
                }

                // Do a new authenticate if expired (over 59 days)
                if (string.IsNullOrEmpty(TokenData.RefreshToken) || TokenData.ExpiresAtUtc.AddDays(59) < DateTime.Now)
                {
                    var task = Task.Run(() => BeginoAuth2Authentication());
                    task.Wait();
                    XeroToken = task.Result; // Set the internal copy of the Token

                    return XeroToken; // Return the resulting token
                }
            }
            onStatusUpdates("Token OK", XeroEventStatus.Success);
            return XeroToken;
        }
        // Because we need to launch a browser and wait for authentications this needs to be a task so it can wait.
        async Task<Model.XeroOAuthToken> BeginoAuth2Authentication()
        {
            if (string.IsNullOrEmpty(XeroConfig.ClientID))
            {
                return null;
            }
            // Raise event to the parent caller (your app)
            onStatusUpdates("Begin Authentication", XeroEventStatus.Success);

            XeroConfig.ReturnedAccessCode = null;// Ensure the Return code cleared as we are authenticating and this propery will be monitored for the completion
            XeroToken = new Model.XeroOAuthToken(); // Set an empty token ready
            //start webserver to listen for the callback
            responseListener = new LocalHttpListener();
            responseListener.Message += MessageResponse;
            responseListener.callBackUri = XeroConfig.CallbackUri;
            responseListener.config = XeroConfig;
            responseListener.StartWebServer();
            XeroToken.AccessToken = null; // Ensure the Access Token is cleared as we are authenticating  

            //open web browser with the link generated
            System.Diagnostics.Process.Start(XeroConfig.AuthURL);

            // Fire Event so the caller can monitor
            onStatusUpdates("Login URL Opened", XeroEventStatus.Log);

            // Basically wait for 60 Seconds (should be long enough)
            int counter = 0;
            do
            {
                await Task.Delay(1000); // Wait 1 second - gives time for response back to listener
                counter++;
            } while (responseListener.config.ReturnedAccessCode == null && counter < Timeout);

            if (counter >= Timeout)
            {
                // Raise event to the parent caller (your app)
                onStatusUpdates("Timed Out Waiting for Authentication", XeroEventStatus.Timeout);
            }
            else
            {
                // Raise event to the parent caller (your app)
                onStatusUpdates("Success", XeroEventStatus.Success);

                XeroConfig = responseListener.config;// update the config with the retrieved access code data
                ExchangeCodeForToken();

                responseListener.StopWebServer();
            }
            // Raise event to the parent caller (your app)
            onStatusUpdates("Authentication Completed", XeroEventStatus.Success);

            return XeroToken;
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

                    var responsetask = Task.Run(() => client.PostAsync(XeroURLS.XERO_TOKEN_URL, formContent));
                    responsetask.Wait();
                    var response = responsetask.Result;// await client.PostAsync(url, formContent);

                    var contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
                    contenttask.Wait();
                    var content = contenttask.Result;// await response.Content.ReadAsStringAsync();
                    var tokens = JObject.Parse(content);

                    // Record the token data
                    XeroToken.Tenants = null;
                    XeroToken.IdToken = tokens["id_token"]?.ToString();
                    XeroToken.AccessToken = tokens["access_token"]?.ToString();
                    XeroToken.ExpiresAtUtc = DateTime.Now.AddSeconds(int.Parse(tokens["expires_in"]?.ToString()));
                    XeroToken.RefreshToken = tokens["refresh_token"]?.ToString();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", XeroToken.AccessToken);

                    responsetask = Task.Run(() => client.GetAsync(XeroURLS.XERO_TENANTS_URL));
                    responsetask.Wait();
                    response = responsetask.Result;

                    contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
                    contenttask.Wait();
                    content = contenttask.Result;

                    // Record the Available Tenants
                    XeroToken.Tenants = JsonConvert.DeserializeObject<List<Model.Tenant>>(content);

                    // Raise event to the parent caller (your app) 
                    onStatusUpdates("Code Exchange Completed", XeroEventStatus.Success);
                }
            }
            catch (Exception ex)
            {
                // Raise event to the parent caller (your app)
                onStatusUpdates("Begin Code Failed", XeroEventStatus.Failed);
                throw new InvalidDataException("Code Exchange Failed");
            }
        }

        /// <summary>
        /// REvoke the Access Token and disconnect the tenants from the user
        /// </summary>
        /// <param name="xeroToken"></param>
        public void RevokeToken(Model.XeroOAuthToken XeroToken)
        {
            if (XeroToken == null)
            {
                throw new ArgumentNullException("XeroToken");
            }

            //TODO Implement Revoke (when found out how!
            //var client = new HttpClient();

            //var response = client.RevokeTokenAsync(new TokenRevocationRequest
            //{
            //    Address = "~https~://identity.xero.com/connect/revocation",
            //    ClientId = xeroConfiguration.ClientId,
            //    ClientSecret = xeroConfiguration.ClientSecret,
            //    Token = xeroToken.RefreshToken
            //});

            //if (response.IsError)
            //{
            //    throw new Exception(response.Error);
            //}


        }
        public Model.XeroOAuthToken RefreshToken(Model.XeroOAuthToken TokenData = null)
        {
            if (TokenData == null)
            {
                // Use passed in token object
                TokenData = XeroToken;
            }
            onStatusUpdates("Begin Token Refresh", XeroEventStatus.Success);

            if (TokenData == null)
            {
                onStatusUpdates("Failed - Missing Token Data", XeroEventStatus.Failed);
                return null;
            }

            var client = new HttpClient();
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", XeroConfig.ClientID),
                new KeyValuePair<string, string>("refresh_token", TokenData.RefreshToken),
            });

            var responsetask = Task.Run(() => client.PostAsync(XeroURLS.XERO_TOKEN_URL, formContent));
            responsetask.Wait();
            var response = responsetask.Result;

            var contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
            contenttask.Wait();
            var content = contenttask.Result;

            var tokens = JObject.Parse(content);

            // Store the data in the local copy now
            XeroToken = new Model.XeroOAuthToken();

            XeroToken.AccessToken = tokens["access_token"]?.ToString();
            XeroToken.ExpiresAtUtc = DateTime.Now.AddSeconds(int.Parse(tokens["expires_in"]?.ToString()));
            XeroToken.RefreshToken = tokens["refresh_token"]?.ToString();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", XeroToken.AccessToken);

            responsetask = Task.Run(() => client.GetAsync(XeroURLS.XERO_TENANTS_URL));
            responsetask.Wait();
            response = responsetask.Result;// await client.PostAsync(url, formContent);

            contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
            contenttask.Wait();
            content = contenttask.Result;// await response.Content.ReadAsStringAsync();

            XeroToken.Tenants = JsonConvert.DeserializeObject<List<Model.Tenant>>(content);

            onStatusUpdates("Token Refresh Success", XeroEventStatus.Refreshed);

            return XeroToken;

        }

        #region JSON Serialization methods
        public string SerializeObject<TENTITY>(TENTITY objectRecord)
        {
            string serialVersion = Newtonsoft.Json.JsonConvert.SerializeObject(objectRecord, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error
            });
            return serialVersion;
        }
        public TENTITY DeSerializeObject<TENTITY>(string serializedString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TENTITY>(serializedString);
        }
        #endregion
    }
}
