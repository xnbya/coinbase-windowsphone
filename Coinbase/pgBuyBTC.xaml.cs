using System;
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

namespace Coinbase
{
    public partial class pgBuyBTC : PhoneApplicationPage
    {
        string AuthToken;
        public pgBuyBTC()
        {
            loaded = false;
            InitializeComponent();
            loaded = true;
            AuthToken = (string)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["token"];
        }

        bool loaded;
        private void GetBuyPrice()
        {
            if (loaded)
            {
                string buysell = "sell";
                if (lstBuySell.SelectedIndex == 0)
                    buysell = "buy";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/api/v1/prices/" + buysell + "?qty=" + txtAmount.Text );

                req.Method = "GET";
                req.BeginGetResponse(new AsyncCallback(GetBuyPriceClbk), req);
            }
            
        }

        private void BuySellBitcoins()
        {
            string buysell = "sells";
            if (lstBuySell.SelectedIndex == 0)
                buysell = "buys";

            WebClient wclient = new WebClient();
            wclient.UploadStringCompleted += new UploadStringCompletedEventHandler(wclient_completed);

            string jsonstr = "{\"qty\":" + finalquantity + "}"; 

            wclient.Headers[HttpRequestHeader.ContentType] = "application/json";

            wclient.UploadStringAsync(new Uri("https://coinbase.com/api/v1/" + buysell + "?access_token=" + AuthToken), "POST", jsonstr);
           
        }

        string finalquantity;

        private void wclient_completed(object sender, UploadStringCompletedEventArgs e)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.BuySell.RootObject));

            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(e.Result));

                JSON1.BuySell.RootObject buyresp = json.ReadObject(ms) as JSON1.BuySell.RootObject;
                if (buyresp.success)
                {
                    MessageBox.Show("Successfully requested transfer of " + buyresp.transfer.total + " " + buyresp.transfer.total.currency + ". Will be done at " + buyresp.transfer.payout_date.Substring(0, 10) + " at " + buyresp.transfer.payout_date.Substring(11, 5), "Success", MessageBoxButton.OK);
                }
                else
                {
                    if(buyresp.errors[0] == "Payment method can't be blank")
                    {
                        if (MessageBox.Show("You need to setup a payment method. Open coinbase website to add one now?", "Payment method needed", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            Microsoft.Phone.Tasks.WebBrowserTask wb = new Microsoft.Phone.Tasks.WebBrowserTask();
                            wb.Uri = new Uri("https://coinbase.com/payment_methods", UriKind.Absolute);
                            wb.Show();
                        }
                    }
                    else
                        MessageBox.Show(buyresp.errors[0], "Error!", MessageBoxButton.OK);
                }
                

            
        }

        void GetBuyPriceClbk(IAsyncResult result)
        {
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
            using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.BuyResponse.RootObject));

                JSON1.BuyResponse.RootObject buyresp = json.ReadObject(response.GetResponseStream()) as JSON1.BuyResponse.RootObject;
                            

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    lblSubtotal.Text = "Subtotal: " + buyresp.subtotal.amount + " " + buyresp.subtotal.currency;
                    lblCFees.Text = "Coinbase Fees: " + buyresp.fees[0].coinbase.amount + " " + buyresp.fees[0].coinbase.currency;
                    lblBankFees.Text = "Bank Fees: " + buyresp.fees[1].bank.amount + " " + buyresp.fees[1].bank.currency;
                    lblPrice.Text = "Total: " + buyresp.total.amount + " " + buyresp.total.currency;
                    if (blConfirm)
                    {
                        blConfirm = false;
                        finalquantity = txtAmount.Text;
                        if (MessageBox.Show("Are you sure you want to buy " + finalquantity + " BTC for " + buyresp.total.amount + " " + buyresp.total.currency, "Continue?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {                            
                            BuySellBitcoins();
                        }
                    }
                });




            }
        }

        private void txtAmount_TextChanged(object sender, TextChangedEventArgs e)
        {            
                GetBuyPrice();
        }

        private void lstBuySell_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBuyPrice();
            if (loaded)
            {
                if (lstBuySell.SelectedIndex == 0)
                    btnBuySell.Content = "Buy Bitcoins";
                else
                    btnBuySell.Content = "Sell Bitcoins";
            }
            
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            blConfirm = true;
            GetBuyPrice();

        }

        bool blConfirm;
    }
}