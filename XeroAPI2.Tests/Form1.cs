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
        string XeroClientID = "ClientID";
        Uri XeroCallbackUri = new Uri("http://localhost:8888/callback");
        string XeroScope = "openid profile email files accounting.transactions accounting.reports.read accounting.journals.read accounting.settings.read accounting.contacts assets";
        string XeroState = "123456";

        public static string ApplicationPath = System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;
        XeroAuth2API.Model.XeroOAuthToken accessToken = new XeroAuth2API.Model.XeroOAuthToken();

        XeroAuth2API.Model.XeroConfiguration XeroConfig = null;
        XeroAuth2API.API xeroAPI = null;

        static FileSystemWatcher LogWatcher = null; // Used to watch a file system folder.

        public Form1()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            XeroConfig = new XeroAuth2API.Model.XeroConfiguration
            {
                ClientID = XeroClientID,
                CallbackUri = XeroCallbackUri,
                Scope = XeroScope,
                // State = XeroState, // Not needed for a desktop app
                codeVerifier = null // Code verifier will be generated if empty
            };

            xeroAPI = new XeroAuth2API.API(XeroConfig, accessToken);
            xeroAPI.StatusUpdates += StatusUpdates; // Bind to the status update event 
            xeroAPI.InitializeAPI(); // Init the API and pass in the old token - this will be refreshed if required

            // Write the AccessToken to storage
            string tokendata = SerializeObject(xeroAPI.XeroToken);
            WriteTextFile("tokendata.txt", tokendata);

            // Find the Demo Company TenantID
            XeroAuth2API.Model.Tenant Tenant = xeroAPI.Tenants.Find(x => x.TenantName.ToLower() == "demo company (uk)");
            //  XeroAuth2API.Model.Tenant Tenant = xeroAPI.Tenants[1];
            xeroAPI.TenantID = Tenant.TenantId.ToString(); // Ensure its selected

            // var assettypes = xeroAPI.AssetTypes();

            var contacts = xeroAPI.Contacts();

            try
            {
                var assets = xeroAPI.Assets(Xero.NetStandard.OAuth2.Model.Asset.AssetStatusQueryParam.REGISTERED);
            }
            catch (Exception ex)
            {
                // Deal with the error
                int stop = 0;
            }
            


            var POrders = xeroAPI.PurchaseOrders();
            if (POrders != null && POrders.Count > 0)
            {
                // var singlePO = xeroAPI.TrackingCategory(POrders[0].TrackingCategoryID.Value);
            }

            var users = xeroAPI.Users();


            var trackingcats = xeroAPI.TrackingCategories();
            if (trackingcats != null && trackingcats.Count > 0)
            {
                var singlecat = xeroAPI.TrackingCategory(trackingcats[0].TrackingCategoryID.Value);
            }


            var banktrans = xeroAPI.BankTransactions();
            var singleptran = xeroAPI.BankTransaction(banktrans[3].BankTransactionID.Value);

            var banktransfers = xeroAPI.BankTransfers();
            if (banktransfers != null && banktransfers.Count > 0)
            {
                var singlebanktransfer = xeroAPI.BankTransaction(banktransfers[3].BankTransferID.Value);
            }

            var taxrates = xeroAPI.TaxRates();
            if (taxrates != null && taxrates.Count > 0)
            {
                var singletaxrate = xeroAPI.TaxRate(taxrates[3].Name);
            }


            var products = xeroAPI.Items();
            var singleproduct = xeroAPI.Item(products[3].ItemID.Value);


            var invoices = xeroAPI.Invoices();
            var singleinvoice = xeroAPI.Invoice(invoices[5].InvoiceID.Value);

            var accounts = xeroAPI.Accounts(); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>
            var singleaccount = xeroAPI.Account(accounts[5].AccountID.Value);// return Xero.NetStandard.OAuth2.Model.Accounting.Account

            var quotes = xeroAPI.Quotes();
            if (quotes != null && quotes.Count > 0)
            {
                var singlequote = xeroAPI.Quote(quotes[0].QuoteID.Value);// return Xero.NetStandard.OAuth2.Model.Accounting.Quote
            }


            int h = 0;

        }
        private void StatusUpdates(object sender, XeroAuth2API.API.StatusEventArgs e)
        {
            // Event fired so recored the log 
            System.Diagnostics.Debug.WriteLine(e.MessageText);

            WriteLogFile($"{e.Status.ToString()} - {e.MessageText}", "APILog", true, true);


        }


        public void WriteLogFile(string sText, string sLogFileName, bool bTimeStamp = false, bool appendNewLine = true)
        {
            try
            {
                string sPath = Path.Combine(ApplicationPath, "Logs", $"{sLogFileName}.log");

                //   string sPath = String.Format(@"{0}\logs\{1}.log", ApplicationPath, sLogFileName);

                if (System.IO.File.Exists(sPath).Equals(false))
                {
                    Directory.CreateDirectory(Path.Combine(ApplicationPath, "Logs"));

                    System.IO.File.AppendAllText(sPath, @"-----------------------------" + Environment.NewLine);
                    System.IO.File.AppendAllText(sPath, $"{sLogFileName} Log file{Environment.NewLine}");
                    System.IO.File.AppendAllText(sPath, @"-----------------------------" + Environment.NewLine);
                    System.IO.File.AppendAllText(sPath, $"Created {DateTime.Now}{Environment.NewLine}{Environment.NewLine}");
                }

                if (bTimeStamp)
                {
                    sText = $"({DateTime.Now}) {sText}";
                }
                if (appendNewLine)
                {
                    System.IO.File.AppendAllText(sPath, Environment.NewLine + sText);
                }
                else
                {
                    System.IO.File.AppendAllText(sPath, sText);
                }

                FileInfo fiLog = new FileInfo(sPath);

                if (fiLog.Length > 1000000)
                {
                    System.IO.File.Move(sPath, $@"{ApplicationPath}\logs\Completed\{sLogFileName}_{DateTime.Now.ToString("dd-mm-yyyy HHmmss")}.log");
                }


                //<EhFooter>
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (System.Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

                // Handle exception here
                //Interaction.MsgBox("Error occured in modStuff(modStuff.vb) - at Sub - WriteLogFile at line " + Information.Erl() + Constants.vbCrLf + Err().Description, Constants.vbExclamation + Constants.vbOKOnly, "Application Exception Error in MailSolutionsXML");
            }

            //</EhFooter>
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
