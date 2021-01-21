using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using XeroAuth2API.Model;


namespace XeroAPI2Tests
{
    public partial class Form1 : Form
    {
        string XeroClientID = "Your Client ID";
        Uri XeroCallbackUri = new Uri("http://localhost:8888/callback");
        string XeroState = "123456";

        public static string ApplicationPath = System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName;

        XeroConfiguration XeroConfig = null;
        XeroAuth2API.API xeroAPI = null;

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // We can ether save/restore the entire config or just the AccessToken element
            string tokendata = ReadTextFile("tokendata.txt");
            if (!string.IsNullOrEmpty(tokendata))
            {
                XeroConfig = DeSerializeObject<XeroConfiguration>(tokendata);
            }
            else
            {
                // Setup New Config
                XeroConfig = new XeroConfiguration
                {
                    ClientID = XeroClientID,
                    CallbackUri = XeroCallbackUri,
                    // Add them this way or see below
                    //Scopes = new List<XeroAuth2API.XeroScope> { XeroAuth2API.XeroScope.accounting_contacts, XeroAuth2API.XeroScope.accounting_transactions },
                    State = XeroState, // Optional - Not needed for a desktop app
                    codeVerifier = null // Code verifier will be generated if empty
                };
                XeroConfig.AddScope(XeroAuth2API.XeroScope.all);
                // Or add idividualy
                //XeroConfig.AddScope(XeroAuth2API.XeroScope.files);
                //XeroConfig.AddScope(XeroAuth2API.XeroScope.accounting_transactions);
                //XeroConfig.AddScope(XeroAuth2API.XeroScope.accounting_reports_read);
                //XeroConfig.AddScope(XeroAuth2API.XeroScope.accounting_journals_read);
                //XeroConfig.AddScope(XeroAuth2API.XeroScope.accounting_settings_read);
                //XeroConfig.AddScope(XeroAuth2API.XeroScope.accounting_contacts);
                //XeroConfig.AddScope(XeroAuth2API.XeroScope.assets);               
            }

            // Restore saved config
            xeroAPI = new XeroAuth2API.API(XeroConfig);
            //xeroAPI.XeroConfig = XeroConfig;
            // Save this
            tokendata = SerializeObject(XeroConfig);
            WriteTextFile("tokendata.txt", tokendata);
            xeroAPI.StatusUpdates += StatusUpdates; // Bind to the status update event 

        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            xeroAPI.InitializeAPI(); // Init the API and pass in the old token - this will be refreshed if required

            // Write the AccessToken to storage
            string tokendata = SerializeObject(XeroConfig);
            WriteTextFile("tokendata.txt", tokendata);

            // Find the Demo Company TenantID
            Tenant Tenant = xeroAPI.Tenants.Find(x => x.TenantName.ToLower() == "demo company (uk)");
            //  XeroAuth2API.Model.Tenant Tenant = xeroAPI.Tenants[1];
            xeroAPI.SelectedTenant = Tenant; // Ensure its selected

            // var assettypes = xeroAPI.AssetTypes();

            //var contacts = xeroAPI.Contacts();
            List<Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum> status = new List<Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum>();
            status.Add(Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum.ACTIVE);
            status.Add(Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum.ARCHIVED);

            List<Xero.NetStandard.OAuth2.Model.Accounting.AccountType> atypes = new List<Xero.NetStandard.OAuth2.Model.Accounting.AccountType>();
            atypes.Add(Xero.NetStandard.OAuth2.Model.Accounting.AccountType.OVERHEADS);
            atypes.Add(Xero.NetStandard.OAuth2.Model.Accounting.AccountType.BANK);

            //var accounts = xeroAPI.AccountingApi.Accounts(status, "Name", atypes); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>
           // var accounts = xeroAPI.AccountingApi.Accounts(Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum.ACTIVE, "Name"); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>

            //var  accounts = xeroAPI.AccountingApi.Accounts(); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>

            //var singleaccount = xeroAPI.AccountingApi.Account(accounts[5].AccountID.Value);// return Xero.NetStandard.OAuth2.Model.Accounting.Account

            try
            {

               // var assets = xeroAPI.AssetApi.Assets(Xero.NetStandard.OAuth2.Model.Asset.AssetStatusQueryParam.REGISTERED);
            }
            catch (Exception ex)
            {
                // Deal with the error
                int stop = 0;
            }



            //var POrders = xeroAPI.AccountingApi.PurchaseOrders();
            //if (POrders != null && POrders.Count > 0)
            //{
            //     var singlePO = xeroAPI.AccountingApi.PurchaseOrder(POrders[0].PurchaseOrderID.Value);
            //}

            //var users = xeroAPI.AccountingApi.Users();


            //var trackingcats = xeroAPI.AccountingApi.TrackingCategories();
            //if (trackingcats != null && trackingcats.Count > 0)
            //{
            //    var singlecat = xeroAPI.AccountingApi.TrackingCategory(trackingcats[0].TrackingCategoryID.Value);
            //}


            //var banktrans = xeroAPI.AccountingApi.BankTransactions();
            //var singleptran = xeroAPI.AccountingApi.BankTransaction(banktrans[3].BankTransactionID.Value);

            //var banktransfers = xeroAPI.AccountingApi.BankTransfers();
            //if (banktransfers != null && banktransfers.Count > 0)
            //{
            //    var singlebanktransfer = xeroAPI.AccountingApi.BankTransaction(banktransfers[3].BankTransferID.Value);
            //}

            //var taxrates = xeroAPI.AccountingApi.TaxRates();
            //if (taxrates != null && taxrates.Count > 0)
            //{
            //    var singletaxrate = xeroAPI.AccountingApi.TaxRate(taxrates[3].Name);
            //}


            //var products = xeroAPI.AccountingApi.Items();
            //var singleproduct = xeroAPI.AccountingApi.Item(products[3].ItemID.Value);


            //var invoices = xeroAPI.AccountingApi.Invoices(null, null, -1);
            var invoices = xeroAPI.AccountingApi.Invoices( Xero.NetStandard.OAuth2.Model.Accounting.Invoice.StatusEnum.AUTHORISED,new DateTime(2021,1,1));
            var singleinvoice = xeroAPI.AccountingApi.Invoice(invoices[5].InvoiceID.Value);



            //var quotes = xeroAPI.AccountingApi.Quotes();
            //if (quotes != null && quotes.Count > 0)
            //{
            //    var singlequote = xeroAPI.AccountingApi.Quote(quotes[0].QuoteID.Value);// return Xero.NetStandard.OAuth2.Model.Accounting.Quote
            //}


            int h = 0;

        }
        private void StatusUpdates(object sender, XeroAuth2API.API.StatusEventArgs e)
        {
            // Event fired so recored the log 
            System.Diagnostics.Debug.WriteLine(e.MessageText);

            WriteLogFile($"{e.Status.ToString()} - {e.MessageText}", "APILog", true, true);
            // UpdateStatus($"{e.Status.ToString()} - {e.MessageText}", lstResults); gets stuck due to invoke?!?!?!!?!?
        }
        public static void UpdateStatus(string sText, ListBox lstResults = null, bool bSameLine = false, bool bAdd = false)
        {
            if (lstResults.InvokeRequired)
            {
                lstResults.Invoke((MethodInvoker)delegate
                {
                    UpdateStatus(sText, lstResults);
                });
            }
            else
            {
                if (lstResults != null)
                {
                    // Passed in a ref to a list box so we can update this  s
                    if (bSameLine && bAdd)
                    {
                        string PrevLine = lstResults.Items[lstResults.Items.Count - 1].ToString();
                        lstResults.Items[lstResults.Items.Count - 1] = PrevLine + sText;
                    }
                    else if (bSameLine && !bAdd)
                    {
                        lstResults.Items[lstResults.Items.Count - 1] = sText;
                    }
                    else
                    {
                        // sText = String.Format("({0}) {1}", DateTime.Parse(DateTime.Now.ToString(), ciUKCulture.DateTimeFormat).ToString(), sText);
                        lstResults.Items.Add(sText);
                    }
                    if (lstResults.Items.Count > 65000)
                    {
                        lstResults.Items.RemoveAt(1);
                    }
                    lstResults.TopIndex = lstResults.Items.Count - 10;
                    lstResults.Refresh();

                }

            }
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

        private void button1_Click(object sender, EventArgs e)
        {
            xeroAPI.InitializeAPI(); // Init the API and pass in the old token - this will be refreshed if required

            // Write the AccessToken to storage
            string tokendata = SerializeObject(XeroConfig);
            WriteTextFile("tokendata.txt", tokendata);

            // Find the Demo Company TenantID
            Tenant Tenant = xeroAPI.Tenants.Find(x => x.TenantName.ToLower() == "demo company (uk)");
            //  XeroAuth2API.Model.Tenant Tenant = xeroAPI.Tenants[1];
            xeroAPI.SelectedTenant = Tenant; // Ensure its selected


            // Create a test invoice
            var xeroContact = new Xero.NetStandard.OAuth2.Model.Accounting.Contact
            {
                Name = "Client Name",
                FirstName = "Client",
                LastName = "Name",
                EmailAddress = "emailaddress@na.com",
                IsCustomer = true,

                AccountNumber = $"NEW-ACC",
                // Website = "http://google.com"; // Currently the Zero API has this read only!!

                Addresses = new List<Xero.NetStandard.OAuth2.Model.Accounting.Address>()
            };

            var address = new Xero.NetStandard.OAuth2.Model.Accounting.Address
            {
                AddressType = Xero.NetStandard.OAuth2.Model.Accounting.Address.AddressTypeEnum.STREET,
                AddressLine1 = "Address1_Line1",
                AddressLine2 = "Address1_Line2",
                AddressLine3 = "Address1_Line3",
                City = "Address1_City",
                Region = "Address1_County",
                PostalCode = "Address1_PostalCode",
                Country = "Address1_Country"
            };

            xeroContact.Addresses.Add(address);

            xeroContact.Phones = new List<Xero.NetStandard.OAuth2.Model.Accounting.Phone>();

            var phone = new Xero.NetStandard.OAuth2.Model.Accounting.Phone();
            phone.PhoneType = Xero.NetStandard.OAuth2.Model.Accounting.Phone.PhoneTypeEnum.DEFAULT;
            phone.PhoneNumber = "Telephone1";

            xeroContact.Phones.Add(phone);

            var fax = new Xero.NetStandard.OAuth2.Model.Accounting.Phone();
            fax.PhoneType = Xero.NetStandard.OAuth2.Model.Accounting.Phone.PhoneTypeEnum.FAX;
            fax.PhoneNumber = "Fax";
            xeroContact.Phones.Add(fax);

            var mobile = new Xero.NetStandard.OAuth2.Model.Accounting.Phone();
            mobile.PhoneType = Xero.NetStandard.OAuth2.Model.Accounting.Phone.PhoneTypeEnum.MOBILE;
            mobile.PhoneNumber = "MobilePhone";
            xeroContact.Phones.Add(mobile);

            // Build the Invoice Body
            var invoiceRecord = new Xero.NetStandard.OAuth2.Model.Accounting.Invoice();
            invoiceRecord.Contact = xeroContact;
            invoiceRecord.Date = DateTime.Now;
            invoiceRecord.DueDate = DateTime.Now.AddDays(30);
            invoiceRecord.Status = Xero.NetStandard.OAuth2.Model.Accounting.Invoice.StatusEnum.DRAFT;
            invoiceRecord.LineAmountTypes = Xero.NetStandard.OAuth2.Model.Accounting.LineAmountTypes.Exclusive;

            invoiceRecord.Type = Xero.NetStandard.OAuth2.Model.Accounting.Invoice.TypeEnum.ACCREC;
            invoiceRecord.Reference = $"oAuth2/Testing";
            invoiceRecord.LineItems = new List<Xero.NetStandard.OAuth2.Model.Accounting.LineItem>();

            // Line Item 1
            // Create the Tracking Item
            var tracking = new List<Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking>();
            tracking.Add(new Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking { Name = "Region", Option = "Eastside" });

            Xero.NetStandard.OAuth2.Model.Accounting.LineItem lineItem = new Xero.NetStandard.OAuth2.Model.Accounting.LineItem
            {
                Description = $"Product Item 1{Environment.NewLine}Additional Description text",
                Quantity = 1,
                UnitAmount = 123m,
                LineAmount = 123m,
                TaxAmount = 123m * .20m,
                AccountCode = "200",
                Tracking = tracking
            };

            invoiceRecord.LineItems.Add(lineItem); // Add the line item to the invoice object

            // Line Item 2
            // Create the Tracking Item
            tracking = new List<Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking>();
            tracking.Add(new Xero.NetStandard.OAuth2.Model.Accounting.LineItemTracking { Name = "Region", Option = "South" });

            Xero.NetStandard.OAuth2.Model.Accounting.LineItem lineItem2 = new Xero.NetStandard.OAuth2.Model.Accounting.LineItem
            {
                Description = $"Product Item 2{Environment.NewLine}Additional Description text2",
                Quantity = 2,
                UnitAmount = 456m,
                LineAmount = 456m * 2,
                TaxAmount = (456m * 2) * .20m,
                AccountCode = "200",
                Tracking = tracking
            };

            invoiceRecord.LineItems.Add(lineItem2); // Add the line item to the invoice object             

            if (invoiceRecord.ValidationErrors == null || invoiceRecord.ValidationErrors.Count == 0)
            {
                var createdInv = xeroAPI.AccountingApi.CreateInvoice(invoiceRecord);
                if (createdInv.InvoiceID != Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine("Created New Invoice");
                }
            }
        }
    }
}
