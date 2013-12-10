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
using Microsoft.Phone.Shell;
using System.Runtime.Serialization.Json;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text.RegularExpressions;

namespace Coinbase
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
            if (PhoneApplicationService.Current.State.ContainsKey("settings"))
                settings = (JSON1.UserSettings.User2)PhoneApplicationService.Current.State["settings"];
            else
                GetUserInfo();
            if (IsolatedStorageSettings.ApplicationSettings.Contains("passcode"))
            {
                tglPasscode.IsChecked = true;
                txtPassocde.Visibility = System.Windows.Visibility.Visible;
                lblPasscode.Visibility = System.Windows.Visibility.Visible;
                lblPasscode2.Visibility = System.Windows.Visibility.Visible;
                txtOldPasscode.Visibility = System.Windows.Visibility.Visible;
            }
            GetCbaseCurrencies();
            AuthToken = (string)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["token"];

                
            
        }

        private string AuthToken;
        private JSON1.UserSettings.User2 settings;


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

            settings = UserSettings.users[0].user;

            if (IsolatedStorageSettings.ApplicationSettings.Contains("localcurrency"))
            {
                if (settings.native_currency != (string)IsolatedStorageSettings.ApplicationSettings["localcurrency"])
                {
                    IsolatedStorageSettings.ApplicationSettings["localcurrency"] = settings.native_currency;                    
                }
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings["localcurrency"] = settings.native_currency;                
            }

            PhoneApplicationService.Current.State["settings"] = settings;


        }   

        public class currencyitem
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        

        private void GetCbaseCurrencies()
        {
            //get hte user's account settings
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/api/v1/currencies");
            req.Method = "GET";
            req.BeginGetResponse(new AsyncCallback(GetCbaseCurrenciesClbk), req);
        }

        private void GetCbaseCurrenciesClbk(IAsyncResult result)
        {
            int currentindex = 0;
            string currentcode = settings.native_currency;
            List<currencyitem> currenciesforlst = new List<currencyitem>();
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
            using(StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string rawresult = reader.ReadToEnd();

                //convert \u... to unicode letters
                
                rawresult = Regex.Replace(rawresult, @"\\u(?<Value>[a-zA-Z0-9]{4})", m => {
                return ((char) int.Parse( m.Groups["Value"].Value, System.Globalization.NumberStyles.HexNumber)).ToString();
                } );

                string[] currencies = rawresult.Split('[');
                foreach (string currency in currencies)
                {
                    if (currency.Length > 4)
                    {
                        string[] bitso = currency.Split('"');
                        currenciesforlst.Add(new currencyitem() { Name = bitso[1], Code = bitso[3] });
                        if (bitso[3] == currentcode)
                            currentindex = currenciesforlst.Count;
                    }
                }

            }

            Dispatcher.BeginInvoke(() =>
                {
                   lstpkCurrency.ItemsSource = currenciesforlst;
                   lstpkCurrency.SelectedIndex = currentindex - 1;
                });
                
            }

        private void toggleSwitch1_Checked(object sender, RoutedEventArgs e)
        {
            lblPasscode.Visibility = System.Windows.Visibility.Visible;
            txtPassocde.Visibility = System.Windows.Visibility.Visible;
        }

        private void tglPasscode_Unchecked(object sender, RoutedEventArgs e)
        {
            lblPasscode.Visibility = System.Windows.Visibility.Collapsed;
            txtPassocde.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            //change the currency?
            if (lstpkCurrency.SelectedItem != null)
            {
                currencyitem selectedcurr = (currencyitem)lstpkCurrency.SelectedItem;
                if (settings.native_currency != selectedcurr.Code)
                {
                    try
                    {
                        UpdateCurrency(selectedcurr.Code);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Unable to update currency", MessageBoxButton.OK);
                    }
                }
            }


            //passcode logic
            string oldsavedpassword = null;
            bool existsavedpasswd = IsolatedStorageSettings.ApplicationSettings.Contains("passcode");
            if (existsavedpasswd)
            {
                oldsavedpassword = (string)IsolatedStorageSettings.ApplicationSettings["passcode"];
            }

            //delete password
            if (tglPasscode.IsChecked == false && existsavedpasswd && txtOldPasscode.Text == oldsavedpassword)
            {
                IsolatedStorageSettings.ApplicationSettings.Remove("passcode");
                MessageBox.Show("Deleted Passcode", "Success", MessageBoxButton.OK);
            }
            //change or add password
            else if(txtPassocde.Text.Length > 0 && tglPasscode.IsChecked == true && (existsavedpasswd == false || txtOldPasscode.Text == oldsavedpassword))
            {
                                        if (txtPassocde.Text.Length < 3)
                        {
                            MessageBox.Show("Passcode is not long enough", "Error", MessageBoxButton.OK);
                        }
                        else
                        {
                            System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings["passcode"] = txtPassocde.Text;
                            MessageBox.Show("Updated passcode", "Success", MessageBoxButton.OK);
                        }
            }
            //show error
            else if(txtPassocde.Text.Length > 0 || (existsavedpasswd && tglPasscode.IsChecked == false))
            {
                MessageBox.Show("Please enter your old passcode", "Incorrect passcode", MessageBoxButton.OK);
            }

        }

        public string currcodeapp;

        private void UpdateCurrency(string newcurrcode)
        {                        
            //webclient is easier to use when posting
            WebClient wclient = new WebClient();
            wclient.UploadStringCompleted += new UploadStringCompletedEventHandler(wclient_completed);

            string jsonstr = "{\"user\":{\"native_currency\":\"" + newcurrcode + "\"}}";
            currcodeapp = newcurrcode;

            wclient.Headers[HttpRequestHeader.ContentType] = "application/json";

            wclient.UploadStringAsync(new Uri("https://coinbase.com/api/v1/users/" + settings.id + "?access_token=" + AuthToken), "PUT", jsonstr);
            
        }

        private void wclient_completed(object sender, UploadStringCompletedEventArgs e)
        {
            string[] resultbits = e.Result.Split('"');
            if (resultbits[2].Contains("true"))
            {
                MessageBox.Show("Updated Currency", "Success", MessageBoxButton.OK);
                PhoneApplicationService.Current.State["newcurr"] = currcodeapp;
            }
            else
                MessageBox.Show(resultbits[5]);

        }

        

    }
}