﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Runtime.Serialization.Json;
using Microsoft.Phone.Shell;
using TCD.Device.Camera;
using System.IO.IsolatedStorage;



namespace Coinbase
{
    public partial class pgMain : PhoneApplicationPage
    {

        ApplicationBarIconButton btnSend;
        ApplicationBarIconButton btnNewAddress;
        ApplicationBarIconButton btnScanQR;
        ApplicationBarIconButton btnRefresh;
        ApplicationBarIconButton btnTlbrCopyAdr;
        public string AuthToken;
        string currentaddress;
        decimal currentaccountBTC;
        public string currency;
        public decimal exchange;

        public pgMain()
        {            
            InitializeComponent();            


            //UI stuff
            CreateButtons();
            LoadCurrencies();
            

        }

        private void LoadCurrencies()
        {
            List<string> currencies = new List<string>();
            currencies.Add("mBTC");
            currencies.Add("BTC");
            //currencies.Add("µBTC");
            if(currency != null)            
                currencies.Add(currency);
            Dispatcher.BeginInvoke(() =>
                {
                    lstpkCurrency.ItemsSource = currencies;
                });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //this alows things to be done when user lands on page from anywhere

            base.OnNavigatedTo(e);

            //when navigated back from pgscan
            if (Microsoft.Phone.Shell.PhoneApplicationService.Current.State.ContainsKey("qrscan"))
            {
                //try this, as there is no actuall standard for the QR codes something may go wrong
                try
                {
                    string rawaddress = (string)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["qrscan"];
                    if (rawaddress.Contains(':'))
                    {
                        string[] bits = rawaddress.Split(':')[1].Split('?');
                        rawaddress = bits[0];
                        foreach (string part in bits)
                        {
                            if (part.Contains("amount"))
                            {
                                string amount = part.Split('=')[1];
                                decimal decamount = 0;
                                if (decimal.TryParse(amount, out decamount))
                                {
                                    txtAmount.Text = decamount.ToString();
                                    lstpkCurrency.SelectedIndex = 1;
                                }
                            }
                        }
                    }

                    txtSendTo.Text = rawaddress;

                    PhoneApplicationService.Current.State.Remove("qrscan");
                }
                catch
                {
                }
            }

            //when changed local curr in settings
            if (PhoneApplicationService.Current.State.ContainsKey("newcurr"))
            {
                currency = (string)PhoneApplicationService.Current.State["newcurr"];
                PhoneApplicationService.Current.State.Remove("newcurr");
                GetBalanceLocalCurrency();
            }

            //need to refresh the token
            if (!PhoneApplicationService.Current.State.ContainsKey("tokenTime") || ((DateTime)PhoneApplicationService.Current.State["tokenTime"]).AddHours(2) < DateTime.Now)
            {
                System.IO.IsolatedStorage.IsolatedStorageSettings settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
                if (settings.Contains("refreshToken"))
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/oauth/token?grant_type=refresh_token&refresh_token=" + (string)settings["refreshToken"] + "&redirect_uri=urn:ietf:wg:oauth:2.0:oob&client_id=7c49c1d40b21548106163d2fc4151671f6227cc27033ddf0c5fcb48f74e44019&client_secret=1663b836e3d37fd8e868fdf562d15bde1beef3f27d6301d80fddf280c5765245");
                    req.Method = "POST";
                    req.Accept = "text/xml";
                    AsyncCallback asynccallbk = new AsyncCallback(TokenRefreshClbk);
                    req.BeginGetResponse(asynccallbk, req);
                }
            }
            else if (AuthToken == null)
            {
                AuthToken = (string)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["token"];
                GetHistory();
                GetAddress();
            }

            
        }

        void TokenRefreshClbk(IAsyncResult result)
        {          
                //Get the tokens from the OAUTH response
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
                using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
                {

                    DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.OAUTHstep2));
                    JSON1.OAUTHstep2 Response = json.ReadObject(response.GetResponseStream()) as JSON1.OAUTHstep2;
                    Microsoft.Phone.Shell.PhoneApplicationService.Current.State["token"] = Response.access_token;
                    Microsoft.Phone.Shell.PhoneApplicationService.Current.State["tokenTime"] = DateTime.Now;
                    System.IO.IsolatedStorage.IsolatedStorageSettings settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
                    settings["refreshToken"] = Response.refresh_token;

                    AuthToken = Response.access_token;
                    GetHistory();
                    GetAddress();

                }
           
        }
    

        public void GetAddress()
        {
            //gets recive address
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/api/v1/account/receive_address?access_token=" + AuthToken);
            req.Method = "GET";   
            req.BeginGetResponse(new AsyncCallback(AddressClbk), req);
        }

        private void AddressClbk(IAsyncResult result)
        {
            //code for loading the users current btc address
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.Address));
            
            JSON1.Address Response = json.ReadObject(response.GetResponseStream()) as JSON1.Address;
            if (Response.success)
            {
                currentaddress = Response.address;
                string qrcodestr = "bitcoin:" + currentaddress; 

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    txtBTCAddress.Text = currentaddress;
                    //encodes QR code
                    image1.Source = TCD.Device.Camera.Barcodes.Encoder.GenerateQRCode(qrcodestr, 350);
                });
            }
            else
            {
               // txtBTCAddress.Text = "no addresses found";
            }

        }

        private void CreateButtons()
        {
            //buttons for app bar
            btnSend = appbutton("send", "/Images/upload.png", btnSend_Click);
            btnNewAddress = appbutton("new address", "/Images/add.png", btnNewAddress_Click);
            btnScanQR = appbutton("scan qr code", "/Images/eye.png", btnScanQR_Click);
            btnRefresh = appbutton("refresh", "/Images/refresh.png", btnRefresh_Click);
            btnTlbrCopyAdr = appbutton("copy", "/Images/copy.png", btnTlbrCopyAdr_Click);
        }

        private ApplicationBarIconButton appbutton(string text, string image, EventHandler evhandle)
        {
            ApplicationBarIconButton btn = new ApplicationBarIconButton();
            btn = new ApplicationBarIconButton();
            btn.IconUri = new Uri(image, UriKind.Relative);
            btn.Text = text;
            btn.Click += new EventHandler(evhandle);
            return btn;
        }

        private void btnTlbrCopyAdr_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(currentaddress);
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            GetHistory();
        }

        void btnNewAddress_Click(object sender, EventArgs e)
        {
            //get new recieve address
        }

        void btnSend_Click(object sender, EventArgs e)
        {
            SendBTC(false);
 
        }

        private void SendBTC(bool NeedsFee)
        {
            decimal rawamount = 0;
            if(decimal.TryParse(txtAmount.Text,out rawamount))
            {
            
            decimal BTCamount = 0;
            switch((string)lstpkCurrency.SelectedItem)
            {
                case "BTC":
                    BTCamount = rawamount;
                    break;
                case "mBTC":
                    BTCamount = rawamount / 1000;
                    break;
                default:
                    BTCamount = decimal.Round(rawamount / exchange,6);
                    break;
            }

            if (NeedsFee || MessageBox.Show("Send " + BTCamount + " BTC (" + decimal.Round(BTCamount*exchange,2).ToString() + " " + currency + ") to " + txtSendTo.Text + " ?", "Confirm sending", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //send money
                //webclient is easier to use when posting
                WebClient wclient = new WebClient();
                wclient.UploadStringCompleted += new UploadStringCompletedEventHandler(wclient_completed);
                string user_fee = "";
                if (NeedsFee)
                    user_fee = "\"user_fee\":\"0.0005\",";

                string jsonstr = "{\"transaction\":{\"to\":\"" + txtSendTo.Text + "\",\"amount\":\"" + BTCamount.ToString() + "\"," + user_fee + "\"notes\":\"" + txtMessage.Text + "\"}}";

                wclient.Headers[HttpRequestHeader.ContentType] = "application/json";

                wclient.UploadStringAsync(new Uri("https://coinbase.com/api/v1/transactions/send_money?access_token=" + AuthToken), "POST", jsonstr);
            }
            }
                else
                {
                    MessageBox.Show("Please enter a valid amount","Unable to send",MessageBoxButton.OK);
                }
        }

        private void wclient_completed(object sender, UploadStringCompletedEventArgs e)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.SendResponse.RootObject));

            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(e.Result));
            JSON1.SendResponse.RootObject Response = json.ReadObject(ms) as JSON1.SendResponse.RootObject;
            if (Response.success)
            {
                if (Response.transaction.recipient != null)
                    MessageBox.Show("Successfully sent " + Response.transaction.amount.amount + " BTC to " + Response.transaction.recipient.name, "Money Sent", MessageBoxButton.OK);
                else
                    MessageBox.Show("Successfully sent " + Response.transaction.amount.amount + " BTC", "Success", MessageBoxButton.OK);
                txtAmount.Text = "";
                txtMessage.Text = "";
                txtSendTo.Text = "";
                GetHistory();
            }
            else
            {
                if (Response.errors[0].Contains("fee"))
                {
                    if (MessageBox.Show(Response.errors[0], "Add fee?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        SendBTC(true);
                }
                else
                    MessageBox.Show(Response.errors[0], "Error Sending", MessageBoxButton.OK);
            }

        }

        private void btnScanQR_Click(object sender, EventArgs e)
        {
            //loads the QR scanner
            try
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.Navigate(new Uri("/Scan.xaml", UriKind.Relative));
                });
            }
            catch
            {
                MessageBox.Show("Error scanning QR code");
            }
            

        }

        private void GetUserInfo()
        {
            //get hte user's account settings
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/api/v1/users?access_token=" + AuthToken);
            req.Method = "GET";
            req.BeginGetResponse(new AsyncCallback(UserInfoClbk), req);
        }

        private void UserInfoClbk(IAsyncResult result)
        {            
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.UserSettings.RootObject));

            JSON1.UserSettings.RootObject UserSettings = json.ReadObject(response.GetResponseStream()) as JSON1.UserSettings.RootObject;

            JSON1.UserSettings.User2 settings = UserSettings.users[0].user;

            if (IsolatedStorageSettings.ApplicationSettings.Contains("localcurrency"))
            {
                if (settings.native_currency != (string)IsolatedStorageSettings.ApplicationSettings["localcurrency"])
                {
                    IsolatedStorageSettings.ApplicationSettings["localcurrency"] = settings.native_currency;
                    GetBalanceLocalCurrency();
                }
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings["localcurrency"] = settings.native_currency;
                GetBalanceLocalCurrency();
            }

            PhoneApplicationService.Current.State["settings"] = settings;


        }



        private void GetBalanceLocalCurrency()
        {
            //convert the BTC to an amount in local currency
             currency = "USD";
            if (System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Contains("localcurrency"))
            {
                currency = (string)System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings["localcurrency"];
            }
            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/api/v1/prices/spot_rate?currency=" + currency);
            req.Method = "GET";
            req.BeginGetResponse(new AsyncCallback(LocalCurrBalanceClbk), req);
            LoadCurrencies();
        }

        void LocalCurrBalanceClbk(IAsyncResult result)
        {
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
            using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
            {
                try
                {
                    string result2 = httpWebStreamReader.ReadToEnd();
                    string[] results = result2.Split('"');
                    exchange = decimal.Parse(results[3]);
                    decimal amount = exchange * currentaccountBTC;
                    Dispatcher.BeginInvoke(() =>
                        {
                            lblLocalCurrAmount.Text = decimal.Round(amount, 2).ToString() + " " + results[7];
                            lblBTCLocalExch.Text = "1 BTC = " + exchange.ToString() + " " + results[7];
                        });
                }
                catch
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        lblLocalCurrAmount.Text = "error getting local currency";
                    });
                }

            }
            GetUserInfo();
        }

        private void GetHistory()
        { 
            //getting transaction history
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/api/v1/transactions?access_token=" + AuthToken);
            req.Method = "GET";
            req.Accept = "text/xml";
            req.BeginGetResponse(new AsyncCallback(HistoryClbk), req);
        }

        private JSON1.ResponseHistory.RootObject TransactionHistory;

        private void HistoryClbk(IAsyncResult result)
        {
            //code for loading the history page ie list
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);   
                
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.ResponseHistory.RootObject));
                
                TransactionHistory = json.ReadObject(response.GetResponseStream()) as JSON1.ResponseHistory.RootObject;
                string userID = TransactionHistory.current_user.id;

            //test
                //TransactionHistory.balance.amount = "2.039453";

                if (decimal.TryParse(TransactionHistory.balance.amount, out currentaccountBTC))
                {
                    GetBalanceLocalCurrency();
                }


                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    lblUser.Text = TransactionHistory.current_user.email;
                    lblBalanceBTC.Text = TransactionHistory.balance.amount + " " + TransactionHistory.balance.currency;
                    List<TransactionLstbxItem> TransactionsForList = new List<TransactionLstbxItem>();
                    foreach (JSON1.ResponseHistory.Transactions transactionA in TransactionHistory.transactions)
                    {
                        JSON1.ResponseHistory.Transaction trans = transactionA.transaction;

                        string requeststr = "";
                        if (trans.request)
                            requeststr = "requested ";

                        if (trans.amount.amount.Substring(0,1) != "-")
                            {
                                string sender = "";
                                if (trans.sender == null)
                                {
                                    sender = "an external account";
                                }
                                else
                                {
                                    sender = trans.sender.name;
                                }
                                //lstHistory.Items.Add("Recieved " + trans.amount.amount + trans.amount.currency + " from " + sender);
                                TransactionsForList.Add(new TransactionLstbxItem() { BTC = "→" + trans.amount.amount + " " + trans.amount.currency, Name = requeststr + "from " + sender, id = trans.id });
                            }
                            else
                            {
                                string recipient = "";
                                if (trans.recipient == null)
                                {
                                    recipient = trans.recipient_address.Substring(0, 20) + "...";
                                    
                                }
                                else
                                {
                                    recipient = trans.recipient.name;
                                }
                                TransactionsForList.Add(new TransactionLstbxItem() { BTC = trans.amount.amount.Substring(1) + " " + trans.amount.currency + " →", Name = requeststr + "to " + recipient, id = trans.id });
                                
                            }
                        

                       
                    }
                    
                    lstHistory.ItemsSource = TransactionsForList;
                   
                });
            
            
        }

        public class TransactionLstbxItem
        {
            //used in the history lstpcker
            public string BTC { get; set; }
            public string Name {get;set;}
            public string id { get; set; }
        }

        

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            GetHistory();
        }



        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();
        }

        private void PanoramaItem_MouseMove(object sender, MouseEventArgs e)
        {
            ApplicationBar.Mode = Microsoft.Phone.Shell.ApplicationBarMode.Default;
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //load the differnt buttons for the panormaa pages
            while(ApplicationBar.Buttons.Count != 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }

            switch (pan.SelectedIndex)
            {
                case 1:
                    ApplicationBar.Buttons.Add(btnSend);
                    ApplicationBar.Buttons.Add(btnScanQR);
                    ApplicationBar.Mode = Microsoft.Phone.Shell.ApplicationBarMode.Default;
                    break;
                case 2:
                    //ApplicationBar.Buttons.Add(btnNewAddress);
                    ApplicationBar.Buttons.Add(btnTlbrCopyAdr);
                    ApplicationBar.Mode = ApplicationBarMode.Default;
                    break;
                case 3:
                    ApplicationBar.Buttons.Add(btnRefresh);
                    
                    ApplicationBar.Mode = ApplicationBarMode.Default;
                    break;
                default:
                    ApplicationBar.Mode = ApplicationBarMode.Minimized;
                    break;
            }

        }



        private void btnCopyAddress_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(currentaddress);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            //logout
            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings.Remove("refreshToken");
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["logoff"] = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                
                NavigationService.Navigate(new Uri("/pgLogin.xaml", UriKind.Relative));
                
            });
            
        }

        private void lstHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstHistory.SelectedIndex >= 0)
            {
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State["transaction"] = TransactionHistory.transactions[lstHistory.SelectedIndex].transaction;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {

                    NavigationService.Navigate(new Uri("/pgTransaction.xaml", UriKind.Relative));

                });
            }
        }

        private void btnBuyBTC_Click(object sender, RoutedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {

                NavigationService.Navigate(new Uri("/pgBuyBTC.xaml", UriKind.Relative));

            });
        }

    }
}