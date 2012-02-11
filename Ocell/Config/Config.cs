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
using System.Linq;
using System.IO.IsolatedStorage;


namespace Ocell
{
    public static class Config
    {
        public static List<Account> Accounts
        {
            get
            {
                IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;
                List<Account> val;

                if (!conf.TryGetValue<List<Account>>("ACCOUNTS", out val))
                    return null;
                return val;
            }
            set
            {
                IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;

                if (conf.Contains("ACCOUNTS"))
                    conf["ACCOUNTS"] = value;
                else
                    conf.Add("ACCOUNTS", value);
            }
        }
    }
}
