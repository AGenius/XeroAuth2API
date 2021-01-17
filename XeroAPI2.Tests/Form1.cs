using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XeroAPI2Tests
{
    public partial class Form1 : Form
    {     
        string XeroClientID = "Client ID";
        Uri XeroCallbackUri = new Uri("http://localhost:8888/callback");
        string XeroScope = "openid profile email files accounting.transactions accounting.reports.read accounting.journals.read accounting.settings.read accounting.contacts offline_access";
        string XeroState = "123456";

        public static string ApplicationPath = System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
        XeroAuth2API.Model.XeroOAuthToken accessToken = new XeroAuth2API.Model.XeroOAuthToken();
        public Form1()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            XeroAuth2API.API xeroAPI = new XeroAuth2API.API(XeroClientID, XeroCallbackUri, XeroScope, XeroState, accessToken);
            xeroAPI.StatusUpdate += StatusUpdate; // Bind to the status update event 

            // Write the AccessToken to storage
            string tokendata = SerializeObject(xeroAPI.XeroToken);
            WriteTextFile("tokendata.txt", tokendata);

            // Find the Demo Company TenantID
            XeroAuth2API.Model.Tenant Tenant = xeroAPI.Tenants.Find(x => x.TenantName.ToLower() == "demo company (uk)");
            xeroAPI.TenantID = Tenant.TenantId.ToString(); // Ensure its selected

            var invoices = xeroAPI.Invoices();
            var singleinvoice = xeroAPI.Invoice(invoices[5].InvoiceID.Value);

            var accounts = xeroAPI.Accounts(); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>
            var single = xeroAPI.GetAccount(accounts[5].AccountID.Value);// return Xero.NetStandard.OAuth2.Model.Accounting.Account

        }
        private void StatusUpdate(object sender, XeroAuth2API.oAuth2.XeroAuth2EventArgs e)
        {
            switch (e.Status)
            {
                case XeroAuth2API.oAuth2.XeroEventStatus.Login:
                    System.Diagnostics.Debug.WriteLine("Begin Login");
                    break;
                case XeroAuth2API.oAuth2.XeroEventStatus.Success:
                    System.Diagnostics.Debug.WriteLine("Authenticated");
                    break;
                case XeroAuth2API.oAuth2.XeroEventStatus.Refreshed:
                    System.Diagnostics.Debug.WriteLine("Refreshed Token");
                    // accessToken = e.XeroTokenData;
                    break;
                case XeroAuth2API.oAuth2.XeroEventStatus.Failed:
                    System.Diagnostics.Debug.WriteLine("Something went wrong");
                    break;
                default:
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string tokendata = ReadTextFile("tokendata.txt");
            if (!string.IsNullOrEmpty(tokendata))
            {
                accessToken = DeSerializeObject<XeroAuth2API.Model.XeroOAuthToken>(tokendata);
            }
            else
            {
                accessToken = new XeroAuth2API.Model.XeroOAuthToken();
            }

            //api2 = new XeroAuth2API.oAuth2();
            //api2.XeroClientID = XeroClientID;
            //api2.XeroCallbackUri = XeroCallbackUri;
            //api2.XeroScope = XeroScope;
            //api2.XeroState = XeroState;

            //api2.StatusUpdate += StatusUpdate;
        }


        /// <summary>Read the contents of a text file into a string </summary>
        /// <param name="filepath">File to read</param>
        /// <returns>files contents</returns>
        public static string ReadTextFile(string filepath)
        {
            try
            {
                string test = Path.GetPathRoot(filepath);

                if (String.IsNullOrEmpty(test) || (test.StartsWith(@"\") && !test.StartsWith(@"\\")))
                {

                    // No Full path supplied so start from Application root
                    if (test.StartsWith(@"\"))
                    {
                        filepath = ApplicationPath + filepath;
                    }
                    else
                    {
                        filepath = $"{ApplicationPath}\\{filepath}";
                    }
                }

                if (File.Exists(filepath).Equals(true))
                {
                    using (StreamReader reader = new StreamReader(filepath))
                    {
                        string contents = reader.ReadToEnd();
                        return contents;
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
        /// <summary>Write the contents of a string to a file </summary>
        /// <param name="filepath">File to write to</param>        
        public static void WriteTextFile(string filepath, string contents)
        {
            try
            {
                string test = Path.GetPathRoot(filepath);

                if (String.IsNullOrEmpty(test) || (test.StartsWith(@"\") && test.Substring(1, 1) != @"\"))
                {

                    // No Full path supplied so start from Application root
                    if (test.StartsWith(@"\"))
                    {
                        filepath = ApplicationPath + filepath;
                    }
                    else
                    {
                        filepath = $"{ApplicationPath}\\{filepath}";
                    }
                }
                using (StreamWriter sw = new StreamWriter(filepath))
                {
                    sw.WriteLine(contents);
                }
            }
            catch (Exception)
            {
            }
        }
        #region JSON Serialization methods
        public static string SerializeObject<TENTITY>(TENTITY objectRecord)
        {
            string serialVersion = Newtonsoft.Json.JsonConvert.SerializeObject(objectRecord, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error
            });
            return serialVersion;
        }
        public static TENTITY DeSerializeObject<TENTITY>(string serializedString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TENTITY>(serializedString);
        }
        #endregion
    }
}
