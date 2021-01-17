using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace XeroAuth2API
{
    public class oAuth2
    {
        #region Event
        public enum XeroEventStatus
        {
            Login,
            Success,
            Refreshed,
            Failed,
            Timeout,
            Log,
        }
        public class XeroAuth2EventArgs : EventArgs
        {
            public string MessageText { get; set; }
            public XeroEventStatus Status { get; set; }
            public Model.XeroOAuthToken XeroTokenData { get; set; }
        }
        public virtual void OnMessageReceived(XeroAuth2EventArgs e)
        {
            EventHandler<XeroAuth2EventArgs> handler = StatusUpdate;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<XeroAuth2EventArgs> StatusUpdate;

        #endregion

        public string XeroClientID { get; set; }
        public Uri XeroCallbackUri { get; set; }
        public string XeroScope { get; set; }
        public string XeroState { get; set; }
        public int? Timeout { get; set; }
        public Model.XeroOAuthToken XeroToken { get; set; }

        LocalHttpListener responseListener = null;

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
                Globals.XeroToken = TokenData;
            }
            Timeout = timeout;

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

            return null;
        }
        // Because we need to launch a browser and wait for authentications this needs to be a task so it can wait.
        async Task<Model.XeroOAuthToken> BeginoAuth2Authentication()
        {
            if (string.IsNullOrEmpty(XeroClientID))
            {
                return null;
            }
            //construct the link that the end user will need to visit in order to authorize the app
            Globals.returnedCode = new XeroAccessCode
            {
                xeroClientID = XeroClientID,
                xeroCallbackUri = XeroCallbackUri,
                xeroScope = XeroScope,
                xeroState = XeroState,
                codeVerifier = GenerateCodeVerifier()
            };

            Globals.XeroToken = new Model.XeroOAuthToken(); // Set an empty token ready
            //start webserver to listen for the callback
            responseListener = new LocalHttpListener();
            responseListener.Message += MessageResponse;
            responseListener.callBackUri = XeroCallbackUri;
            responseListener.StartWebServer(Globals.returnedCode);
            //open web browser with the link generated
            System.Diagnostics.Process.Start(Globals.returnedCode.AuthURL);

            // Fire Event so the caller can monitor
            XeroAuth2EventArgs args = new XeroAuth2EventArgs() { MessageText = $"Login Started", XeroTokenData = null, Status = XeroEventStatus.Login };
            OnMessageReceived(args);

            // Basically wait for 60 Seconds (should be long enough)
            int counter = 0;
            do
            {
                await Task.Delay(1000); // Wait 1 second - gives time for response back to listener
                counter++;
            } while (Globals.XeroToken.Tenants == null && counter < Timeout);

            if (counter >= Timeout)
            {
                args = new XeroAuth2EventArgs() { MessageText = $"Timed Out Waiting for Authentication", XeroTokenData = Globals.XeroToken, Status = XeroEventStatus.Timeout };
                OnMessageReceived(args);
            }
            else
            {
                args = new XeroAuth2EventArgs() { MessageText = $"Success", XeroTokenData = Globals.XeroToken, Status = XeroEventStatus.Success };
                OnMessageReceived(args);
            }

            return Globals.XeroToken;
        }
        private void MessageResponse(object sender, LocalHttpListener.LocalHttpListenerEventArgs e)
        {
            // Get The Access token data once the Auth process has finished
            if (e.MessageText == "CODE")
            {
                ExchangeCodeForToken();

                responseListener.StopWebServer();
                // Raise event to the parent caller (your app)
                XeroAuth2EventArgs args = new XeroAuth2EventArgs() { MessageText = $"Success", XeroTokenData = Globals.XeroToken, Status = XeroEventStatus.Success };
                OnMessageReceived(args);
            }
        }
        private void ExchangeCodeForToken()
        {
            //exchange the code for a set of tokens
            const string url = "https://identity.xero.com/connect/token";

            try
            {
                using (var client = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[]
                      {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("client_id", XeroClientID),
                        new KeyValuePair<string, string>("code", Globals.returnedCode.authCode),
                        new KeyValuePair<string, string>("redirect_uri", XeroCallbackUri.AbsoluteUri),
                        new KeyValuePair<string, string>("code_verifier", Globals.returnedCode.codeVerifier),
                      });

                    var responsetask = Task.Run(() => client.PostAsync(url, formContent));
                    responsetask.Wait();
                    var response = responsetask.Result;// await client.PostAsync(url, formContent);

                    var contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
                    contenttask.Wait();
                    var content = contenttask.Result;// await response.Content.ReadAsStringAsync();


                    var tokens = JObject.Parse(content);

                    // Record the token data
                    Globals.XeroToken.Tenants = null;
                    Globals.XeroToken.IdToken = tokens["id_token"]?.ToString();
                    Globals.XeroToken.AccessToken = tokens["access_token"]?.ToString();
                    Globals.XeroToken.ExpiresAtUtc = DateTime.Now.AddSeconds(int.Parse(tokens["expires_in"]?.ToString()));
                    Globals.XeroToken.RefreshToken = tokens["refresh_token"]?.ToString();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.XeroToken.AccessToken);

                    responsetask = Task.Run(() => client.GetAsync("https://api.xero.com/connections"));
                    responsetask.Wait();
                    response = responsetask.Result;// await client.PostAsync(url, formContent);

                    contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
                    contenttask.Wait();
                    content = contenttask.Result;// await response.Content.ReadAsStringAsync();

                    // response = await client.GetAsync("https://api.xero.com/connections");
                    // content = await response.Content.ReadAsStringAsync();

                    // Record the Available Tenants
                    Globals.XeroToken.Tenants = JsonConvert.DeserializeObject<List<Model.Tenant>>(content);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public Model.XeroOAuthToken RefreshToken(Model.XeroOAuthToken TokenData = null)
        {
            if (TokenData == null)
            {
                // Use passed in token object
                TokenData = Globals.XeroToken;
            }
            if (TokenData == null)
            {
                XeroAuth2EventArgs args2 = new XeroAuth2EventArgs() { MessageText = $"Failed", XeroTokenData = null, Status = XeroEventStatus.Failed };
                OnMessageReceived(args2);
                return null;
            }
            const string url = "https://identity.xero.com/connect/token";
            var client = new HttpClient();
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", XeroClientID),
                new KeyValuePair<string, string>("refresh_token", TokenData.RefreshToken),
            });

            var responsetask = Task.Run(() => client.PostAsync(url, formContent));
            responsetask.Wait();
            var response = responsetask.Result;

            var contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
            contenttask.Wait();
            var content = contenttask.Result;

            var tokens = JObject.Parse(content);

            // Store the data in the local copy now
            Globals.XeroToken = new Model.XeroOAuthToken();

            Globals.XeroToken.AccessToken = tokens["access_token"]?.ToString();
            Globals.XeroToken.ExpiresAtUtc = DateTime.Now.AddSeconds(int.Parse(tokens["expires_in"]?.ToString()));
            Globals.XeroToken.RefreshToken = tokens["refresh_token"]?.ToString();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Globals.XeroToken.AccessToken);

            responsetask = Task.Run(() => client.GetAsync("https://api.xero.com/connections"));
            responsetask.Wait();
            response = responsetask.Result;// await client.PostAsync(url, formContent);

            contenttask = Task.Run(() => response.Content.ReadAsStringAsync());
            contenttask.Wait();
            content = contenttask.Result;// await response.Content.ReadAsStringAsync();

            Globals.XeroToken.Tenants = JsonConvert.DeserializeObject<List<Model.Tenant>>(content);

            XeroAuth2EventArgs args = new XeroAuth2EventArgs() { MessageText = $"Success", XeroTokenData = Globals.XeroToken, Status = XeroEventStatus.Refreshed };
            OnMessageReceived(args);

            return Globals.XeroToken;

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

        private string GenerateCodeVerifier()
        {
            //Generate a random string for our code verifier
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);

            var codeVerifier = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return codeVerifier;
        }
    }
}
