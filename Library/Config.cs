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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO.IsolatedStorage;

namespace Ocell.Library
{
    public static class Config
    {
        private static List<UserToken> _accounts;
        private static ObservableCollection<TwitterResource> _columns;

        public static List<UserToken> Accounts
        {
            get
            {
                if (_accounts != null)
                    return _accounts;

                IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;

                if (!conf.TryGetValue<List<UserToken>>("ACCOUNTS", out _accounts))
                {
                    _accounts = new List<UserToken>();
                    conf.Add("ACCOUNTS", _accounts);
                    conf.Save();
                }

                if (_accounts == null)
                    _accounts = new List<UserToken>();

                return _accounts;
            }
            set
            {
                if (value == null)
                    return;
                
                IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;
                
                _accounts = value;
                if (conf.Contains("ACCOUNTS"))
                    conf["ACCOUNTS"] = value;
                else
                    conf.Add("ACCOUNTS", value);
                conf.Save();
            }
        }

        public static ObservableCollection<TwitterResource> Columns
        {
            get
            {
                IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;

                if (!config.TryGetValue<ObservableCollection<TwitterResource>>("COLUMNS", out _columns))
                {
                    _columns = new ObservableCollection<TwitterResource>();
                    config.Add("COLUMNS", _columns);
                    config.Save();
                }

                if (_columns == null)
                    _columns = new ObservableCollection<TwitterResource>();

                return _columns;
            }
            set
            {
                IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;

                try
                {
                    _columns = value;
                    if (conf.Contains("COLUMNS"))
                        conf["COLUMNS"] = value;
                    else
                        conf.Add("COLUMNS", value);
                    conf.Save();
                }
                catch (Exception)
                {
                }
            }
        }

        public static void SaveAccounts()
        {
            Accounts = _accounts;
        }

        public static void SaveColumns()
        {
            Columns = _columns;
        }
    }
}
