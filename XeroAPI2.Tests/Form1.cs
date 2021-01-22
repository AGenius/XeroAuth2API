using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using XeroAuth2API.Model;

namespace XeroAPI2Tests
{
    public partial class Form1 : Form
    {
        Uri XeroCallbackUri = new Uri("http://localhost:8888/callback");
        string XeroState = "123456";

        string XeroClientID = "";//"Your Client ID";
        string tenantName = "demo company (uk)";// "your company";

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
            UpdateStatus($"Loaded Token");
            if (!string.IsNullOrEmpty(tokendata))
            {
                XeroConfig = DeSerializeObject<XeroConfiguration>(tokendata);
                if (XeroConfig.ClientID != XeroClientID)
                {
                    XeroConfig.XeroAPIToken = null; // force re-auth as ID changed
                    UpdateStatus($"Client ID Changed");
                }
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

            SaveConfig();

            if (!SetupApi(tenantName))
            {
                UpdateStatus($"Failed to Connect");
                simpleButton1.Enabled = false;
                button1.Enabled = false;
                return; /// Stop doing anything else
            }
            else
            {
                UpdateStatus($"Ready - Refresh required : {XeroConfig.XeroAPIToken.ExpiresAtUtc.ToString()}");
                simpleButton1.Enabled = true;
                button1.Enabled = true;
            }

            xeroAPI.StatusUpdates += StatusUpdates; // Bind to the status update event 
        }
        private void SaveConfig()
        {
            string tokendata = SerializeObject(XeroConfig);
            WriteTextFile("tokendata.txt", tokendata);
            UpdateStatus($"Config Saved");
        }
        private bool SetupApi(string tName)
        {
            bool done = false;
            do
            {
                try
                {
                    xeroAPI.InitializeAPI();
                    UpdateStatus($"Initialized");
                    done = true;
                    SaveConfig(); // Ensure the new config (with new tokens are saved)
                }
                catch (Exception ex)
                {
                    UpdateStatus(ex.Message);
                    DialogResult rslt = MessageBox.Show(ex.Message + " Try Again?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (rslt == DialogResult.No)
                    {
                        done = true;
                    }
                }
            } while (!done);
            // Initial startup/auth performed check tenant
            if (xeroAPI.isConnected)
            {
                done = false;
                Tenant tenant = xeroAPI.Tenants.Find(x => x.TenantName.ToLower() == tName.ToLower());
                xeroAPI.SelectedTenant = tenant; // Ensure its selected
                do
                {
                    if (xeroAPI.SelectedTenant == null) // Not selected
                    {
                        try
                        {
                            xeroAPI.InitializeAPI(60, true); // Force a re-auth so any missing tenants can be selected 
                            SaveConfig(); // Ensure the new config (with new tokens are saved)
                            done = true;
                            // Check tenant again
                            tenant = xeroAPI.Tenants.Find(x => x.TenantName.ToLower() == tName.ToString());
                            xeroAPI.SelectedTenant = tenant; // Ensure its selected
                            if (xeroAPI.SelectedTenant == null)
                            {
                                done = false;
                                DialogResult rslt = MessageBox.Show(this, "Invalid Tenant Selected, Please try again", "Error", MessageBoxButtons.YesNo);
                                if (rslt == DialogResult.No)
                                {
                                    done = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            DialogResult rslt = MessageBox.Show(ex.Message + " Try Again?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (rslt == DialogResult.No)
                            {
                                done = true;
                            }
                        }

                    }
                    else
                    {
                        done = true;
                    }
                } while (!done);
                if (xeroAPI.SelectedTenant == null)
                {
                    return false;
                }

                return true;
            }
            else
            {
                UpdateStatus($"Failed");
                MessageBox.Show("Failed to connect to Xero");
                return false;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            xeroAPI.InitializeAPI();
                       
            var contacts = xeroAPI.AccountingApi.Contacts();
            UpdateStatus($"Found {contacts.Count} Contacts");

            List<Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum> status = new List<Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum>();
            status.Add(Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum.DELETED);
            status.Add(Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum.ARCHIVED);

            List<Xero.NetStandard.OAuth2.Model.Accounting.AccountType> atypes = new List<Xero.NetStandard.OAuth2.Model.Accounting.AccountType>();
            atypes.Add(Xero.NetStandard.OAuth2.Model.Accounting.AccountType.OVERHEADS);
            atypes.Add(Xero.NetStandard.OAuth2.Model.Accounting.AccountType.BANK);

            var accounts1 = xeroAPI.AccountingApi.Accounts(status, "Name", atypes); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>
            if (accounts1 != null) UpdateStatus($"Found {accounts1.Count} Archived and Deleted Accounts with Type = Bank and Overheads ");

            var accounts2 = xeroAPI.AccountingApi.Accounts(Xero.NetStandard.OAuth2.Model.Accounting.Account.StatusEnum.ACTIVE, "Name"); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>
            UpdateStatus($"Found {accounts2.Count} Active Accounts");

            var accounts3 = xeroAPI.AccountingApi.Accounts(); // Return List<Xero.NetStandard.OAuth2.Model.Accounting.Account>
            UpdateStatus($"Found {accounts3.Count} Accounts (No Filter)");

            var singleaccount = xeroAPI.AccountingApi.Account(accounts2[5].AccountID.Value);// return Xero.NetStandard.OAuth2.Model.Accounting.Account
            UpdateStatus($"Single Account Name :{singleaccount}");
            try
            {
                var assettypes = xeroAPI.AssetApi.AssetTypes();
                if (assettypes != null) UpdateStatus($"Found {assettypes.Count} Asset Types");

                var assets = xeroAPI.AssetApi.Assets(Xero.NetStandard.OAuth2.Model.Asset.AssetStatusQueryParam.REGISTERED);
                if (assets != null) UpdateStatus($"Found {assets.Count} Registered Assests");
            }
            catch (Exception ex)
            {
                // Deal with the error
                UpdateStatus($"Error fetching Assets {ex.Message}");
            }

            var POrders = xeroAPI.AccountingApi.PurchaseOrders();
            if (POrders != null) UpdateStatus($"Found {POrders.Count} Purchase Orders (no filtering)");
            if (POrders != null && POrders.Count > 0)
            {
                var singlePO = xeroAPI.AccountingApi.PurchaseOrder(POrders[0].PurchaseOrderID.Value);
                UpdateStatus($"Single Purchase Order :{singlePO}");
            }

            var users = xeroAPI.AccountingApi.Users();
            UpdateStatus($"Found {users.Count} User Records");

            var trackingcats = xeroAPI.AccountingApi.TrackingCategories();
            if (trackingcats != null) UpdateStatus($"Found {trackingcats.Count} Tracking Categories");
            if (trackingcats != null && trackingcats.Count > 0)
            {
                var singlecat = xeroAPI.AccountingApi.TrackingCategory(trackingcats[0].TrackingCategoryID.Value);
                UpdateStatus($"Single Tracking Category :{singlecat.Name}");
            }

            var banktrans = xeroAPI.AccountingApi.BankTransactions();
            UpdateStatus($"Found {banktrans.Count} Bank Transactions");
            var singlebtran = xeroAPI.AccountingApi.BankTransaction(banktrans[3].BankTransactionID.Value);
            UpdateStatus($"Single Bank Transaction :{singlebtran}");

            var banktransfers = xeroAPI.AccountingApi.BankTransfers();
            if (banktransfers != null) UpdateStatus($"Found {banktransfers.Count} Bank Transfers");
            if (banktransfers != null && banktransfers.Count > 0)
            {
                var singlebanktransfer = xeroAPI.AccountingApi.BankTransaction(banktransfers[3].BankTransferID.Value);
                UpdateStatus($"Single Bank Transfer :{singlebanktransfer} ");
            }

            var taxrates = xeroAPI.AccountingApi.TaxRates();
            UpdateStatus($"Found {taxrates.Count} Tax Rates");
            if (taxrates != null && taxrates.Count > 0)
            {
                var singletaxrate = xeroAPI.AccountingApi.TaxRate(taxrates[3].Name);
                UpdateStatus($"Single Tax Rate :{singletaxrate}");
            }

            var products = xeroAPI.AccountingApi.Items();
            UpdateStatus($"Found {products.Count} Items/Products");
            var singleproduct = xeroAPI.AccountingApi.Item(products[3].ItemID.Value);
            UpdateStatus($"Single Item/Product :{singleproduct}");

            var invoices = xeroAPI.AccountingApi.Invoices(Xero.NetStandard.OAuth2.Model.Accounting.Invoice.StatusEnum.AUTHORISED, new DateTime(2021, 1, 1));
            UpdateStatus($"Found {invoices.Count} Authorised Invoices since 01/01/2021");
            var singleinvoice = xeroAPI.AccountingApi.Invoice(invoices[5].InvoiceID.Value);
            UpdateStatus($"Single Invoice :{singleinvoice}");

            var quotes = xeroAPI.AccountingApi.Quotes();
            UpdateStatus($"Found {quotes.Count} Quotes");
            if (quotes != null && quotes.Count > 0)
            {
                var singlequote = xeroAPI.AccountingApi.Quote(quotes[0].QuoteID.Value);
                UpdateStatus($"Single Quote :{singlequote}");
            }

            UpdateStatus($"Done");
        }
        private void StatusUpdates(object sender, XeroAuth2API.API.StatusEventArgs e)
        {
            // Event fired so recored the log 
            System.Diagnostics.Debug.WriteLine(e.MessageText);

            WriteLogFile($"{e.Status.ToString()} - {e.MessageText}", "APILog", true, true);
            // UpdateStatus($"{e.Status.ToString()} - {e.MessageText}", lstResults); gets stuck due to invoke?!?!?!!?!?
        }
        public void UpdateStatus(string sText, ListBox lstResults = null, bool bSameLine = false, bool bAdd = false)
        {
            if (lstResults == null)
            {
                lstResults = this.lstResults;
            }
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
            }
            catch (System.Exception ex)
            {
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

        private void button1_Click(object sender, EventArgs e)
        {
            xeroAPI.InitializeAPI();
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