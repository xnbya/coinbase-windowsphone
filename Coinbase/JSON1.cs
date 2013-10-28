using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Coinbase
{
    public class JSON1
    {
        public class BuySell
        {
            public class Coinbase
            {
                public int cents { get; set; }
                public string currency_iso { get; set; }
            }

            public class Bank
            {
                public int cents { get; set; }
                public string currency_iso { get; set; }
            }

            public class Fees
            {
                public Coinbase coinbase { get; set; }
                public Bank bank { get; set; }
            }

            public class Btc
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Subtotal
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Total
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Transfer
            {
                public string _type { get; set; }
                public string code { get; set; }
                public object created_at { get; set; }
                public Fees fees { get; set; }
                public string status { get; set; }
                public string payout_date { get; set; }
                public Btc btc { get; set; }
                public Subtotal subtotal { get; set; }
                public Total total { get; set; }
            }

            public class RootObject
            {
                public bool success { get; set; }
                public List<string> errors { get; set; }
                public Transfer transfer { get; set; }
            }
        }

        public class UserSettings
        {
            public class Balance
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class BuyLimit
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class SellLimit
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class User2
            {
                public string id { get; set; }
                public string name { get; set; }
                public string email { get; set; }
                public string time_zone { get; set; }
                public string native_currency { get; set; }
                public Balance balance { get; set; }
                public int buy_level { get; set; }
                public int sell_level { get; set; }
                public BuyLimit buy_limit { get; set; }
                public SellLimit sell_limit { get; set; }
            }

            public class User
            {
                public User2 user { get; set; }
            }

            public class RootObject
            {
                public List<User> users { get; set; }
            }
        }

        public class BuyResponse
        {
            public class Subtotal
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Coinbase
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Bank
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Fee
            {
                public Coinbase coinbase { get; set; }
                public Bank bank { get; set; }
            }

            public class Total
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class RootObject
            {
                public Subtotal subtotal { get; set; }
                public List<Fee> fees { get; set; }
                public Total total { get; set; }
            }
        }

        public class Address
        {
            public bool success { get; set; }
            public string address { get; set; }
            public object callback_url { get; set; }
        }


        public class OAUTHstep2
        {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string token_type { get; set; }
            public int expire_in { get; set; }
            public string scope { get; set; }
        }

        public class SendReq
        {
            public class Transaction
            {
                public string to { get; set; }
                public string amount { get; set; }
                public string notes { get; set; }
            }

            public class RootObject
            {
                public Transaction transaction { get; set; }
            }
        }

        public class SendResponse
        {
            public class Amount
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Sender
            {
                public string id { get; set; }
                public string name { get; set; }
                public string email { get; set; }
            }

            public class Recipient
            {
                public string id { get; set; }
                public string name { get; set; }
                public string email { get; set; }
            }

            public class Transaction
            {
                public string id { get; set; }
                public string created_at { get; set; }
                public string hsh { get; set; }
                public string notes { get; set; }
                public Amount amount { get; set; }
                public bool request { get; set; }
                public string status { get; set; }
                public Sender sender { get; set; }
                public Recipient recipient { get; set; }
                public string recipient_address { get; set; }
            }

            public class RootObject
            {
                public bool success { get; set; }
                public List<string> errors { get; set; }
                public Transaction transaction { get; set; }
            }
        }

        public class ResponseHistory
        {
            public class CurrentUser
            {
                public string id { get; set; }
                public string email { get; set; }
                public string name { get; set; }
            }

            public class Balance
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Amount
            {
                public string amount { get; set; }
                public string currency { get; set; }
            }

            public class Sender
            {
                public string id { get; set; }
                public string name { get; set; }
                public string email { get; set; }
            }

            public class Recipient
            {
                public string id { get; set; }
                public string name { get; set; }
                public string email { get; set; }
            }

            public class Transaction
            {
                public string id { get; set; }
                public string created_at { get; set; }
                public Amount amount { get; set; }
                public bool request { get; set; }
                public string status { get; set; }
                public Sender sender { get; set; }
                public Recipient recipient { get; set; }
                public string hsh { get; set; }
                public string recipient_address { get; set; }
            }

            public class Transactions
            {
                public Transaction transaction { get; set; }
            }
           
            public class RootObject
            {
                
                public CurrentUser current_user { get; set; }
                public Balance balance { get; set; }
                public int total_count { get; set; }
                public int num_pages { get; set; }
                public int current_page { get; set; }
                public List<Transactions> transactions { get; set; }
            }
        }
    }
}
