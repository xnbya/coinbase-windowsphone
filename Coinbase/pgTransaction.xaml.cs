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

namespace Coinbase
{
    public partial class pgTransaction : PhoneApplicationPage
    {
        public pgTransaction()
        {
            InitializeComponent();
            if (Microsoft.Phone.Shell.PhoneApplicationService.Current.State.ContainsKey("transaction"))
            {
                JSON1.ResponseHistory.Transaction trans = (JSON1.ResponseHistory.Transaction)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["transaction"];
                string[]timebits =  trans.created_at.Split('T');
                lblTime.Text = timebits[0].Replace('-', '/') + " at " + timebits[1].Split('-')[0];
                string BTC = trans.amount.amount;
                if (BTC.StartsWith("-"))
                    BTC = BTC.Substring(1);
                lblBTC.Text = BTC  + " " + trans.amount.currency;

                if (trans.sender == null)                
                    lblFrom.Text = "from an external account";
                else
                    lblFrom.Text = "from " + trans.sender.name;
                if (trans.recipient == null)
                    lblTo.Text = "to " + trans.recipient_address;
                else
                    lblTo.Text = "to " + trans.recipient.name;
                lblStatus.Text = trans.status;
                if (trans.status == "complete")
                    recGreen.Visibility = System.Windows.Visibility.Visible;
                if (trans.request)                
                    lblRequest.Visibility = System.Windows.Visibility.Visible;
                

            }
        }

        private void lblTo_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            
        }
    }
}