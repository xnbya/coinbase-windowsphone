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



namespace Coinbase
{
    public partial class MainPage : PhoneApplicationPage
    {
        public bool SaveToken;
       
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ProgressIndicator progress = new ProgressIndicator
            {
                IsVisible = true,
                IsIndeterminate = true,
                Text = "Loading..."
            };
            SystemTray.SetProgressIndicator(this, progress);
            LoadAuth();

            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Microsoft.Phone.Shell.PhoneApplicationService.Current.State.ContainsKey("logoff"))
            {
                NavigationService.RemoveBackEntry();
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("logoff");
            }


        }

        private void LoadAuth()
        {
            System.IO.IsolatedStorage.IsolatedStorageSettings settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("refreshToken"))
            {
                SaveToken = true;  
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/oauth/token?grant_type=refresh_token&refresh_token=" + (string)settings["refreshToken"] + "&redirect_uri=urn:ietf:wg:oauth:2.0:oob&client_id=7c49c1d40b21548106163d2fc4151671f6227cc27033ddf0c5fcb48f74e44019&client_secret=1663b836e3d37fd8e868fdf562d15bde1beef3f27d6301d80fddf280c5765245");
                req.Method = "POST";
                req.Accept = "text/xml";
                AsyncCallback asynccallbk = new AsyncCallback(BalanceClbk);
                req.BeginGetResponse(asynccallbk, req);
            }
            else
            {
                LoadWebbrowser();
            }
        }

        private void LoadWebbrowser()
        {
            SystemTray.SetProgressIndicator(this, null);
                Uri LoginUri = new Uri("https://coinbase.com/oauth/authorize?response_type=code&client_id=7c49c1d40b21548106163d2fc4151671f6227cc27033ddf0c5fcb48f74e44019&redirect_uri=urn:ietf:wg:oauth:2.0:oob");
                webBrowser1.IsScriptEnabled = true;
                webBrowser1.Navigate(LoginUri);
                webBrowser1.Visibility = System.Windows.Visibility.Visible;

                lblTitle.Visibility = System.Windows.Visibility.Visible;
        }


        private void webBrowser1_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SaveToken = true;
            string url = webBrowser1.Source.ToString();
            if(url.Contains("authorize/"))
            {
                string code = url.Split('/')[5];
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://coinbase.com/oauth/token?grant_type=authorization_code&code=" + code + "&redirect_uri=urn:ietf:wg:oauth:2.0:oob&client_id=7c49c1d40b21548106163d2fc4151671f6227cc27033ddf0c5fcb48f74e44019&client_secret=1663b836e3d37fd8e868fdf562d15bde1beef3f27d6301d80fddf280c5765245");
                req.Method = "POST";
                req.Accept = "text/xml";
                AsyncCallback asynccallbk = new AsyncCallback(BalanceClbk);
                req.BeginGetResponse(asynccallbk, req);
                

                //Microsoft.Phone.Shell.PhoneApplicationService.Current.State["authcode"] = code;
               // NavigationService.Navigate(new Uri("/pgMain.xaml", UriKind.Relative));
            }
        }



        private void button2_Click(object sender, RoutedEventArgs e)
        {


        }

        void BalanceClbk(IAsyncResult result)
        {
            try
            {
            
            //Get the tokens from the OAUTH response
            HttpWebRequest request = (HttpWebRequest)result.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
            using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
            {

                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(JSON1.OAUTHstep2));
                JSON1.OAUTHstep2 Response = json.ReadObject(response.GetResponseStream()) as JSON1.OAUTHstep2;
                Microsoft.Phone.Shell.PhoneApplicationService.Current.State["token"] = Response.access_token;
                System.IO.IsolatedStorage.IsolatedStorageSettings settings = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;

                if (SaveToken)
                {
                    if (!settings.Contains("refreshToken"))
                    {
                        settings.Add("refreshToken", Response.refresh_token);
                    }
                    else
                    {
                        settings["refreshToken"] = Response.refresh_token;
                    }
                }
                else
                {
                    settings.Remove("refreshToken");
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // change UI here
                    NavigationService.Navigate(new Uri("/pgMain.xaml", UriKind.Relative));
                    //NavigationService.GoBack();
                });

            }
            }
                catch
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        // change UI here
                        LoadWebbrowser();
                    });
                    
                }

            }

        
        }
    }
