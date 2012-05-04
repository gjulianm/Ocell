using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using TweetSharp;
using Ocell.Library.Twitter;
using Ocell.Library.Filtering;
using Ocell.Library.Tasks;


namespace Ocell.Library
{
    public static class Config 
    {
        private const string AccountsKey = "ACCOUNTS";
        private const string ColumnsKey = "COLUMNS";
        private const string FollowMsg = "FOLLOWMSG";
        private const string TweetTasksKey = "TWEETTASKS";
        private const string BGLoadColumns = "BGLOADCOLUMNS";
        private const string ProtectedAccountsKey = "PROTECTEDACC";
        private const string FiltersKey = "FILTERS";
        private const string GlobalFilterKey = "GLOBALFILTER";
        private const string RTAsMentionsKey = "RTASMENTIONS";

        private static List<UserToken> _accounts;
        private static ObservableCollection<TwitterResource> _columns;
        private static bool? _followMessageShown;
        private static List<TwitterStatusTask> _tweetTasks;
        private static bool? _backgroundLoadColumns;
        private static List<UserToken> _protectedAccounts;
        private static List<ColumnFilter> _filters;
        private static ColumnFilter _globalFilter;
        private static bool? _retweetAsMentions;

        public static bool? RetweetAsMentions
        {
            get
            {
                return GenericGetFromConfig<bool?>(RTAsMentionsKey, ref _retweetAsMentions);
            }
            set
            {
                GenericSaveToConfig<bool?>(RTAsMentionsKey, ref _retweetAsMentions, value);
            }
        }

        public static ColumnFilter GlobalFilter
        {
            get
            {
                return GenericGetFromConfig<ColumnFilter>(GlobalFilterKey, ref _globalFilter);
            }
            set
            {
                GenericSaveToConfig<ColumnFilter>(GlobalFilterKey, ref _globalFilter, value);
            }
        }

        public static List<ColumnFilter> Filters
        {
            get
            {
                return GenericGetFromConfig<List<ColumnFilter>>(FiltersKey, ref _filters);
            }
            set
            {
                GenericSaveToConfig<List<ColumnFilter>>(FiltersKey, ref _filters, value);
            }
        }

        public static List<UserToken> ProtectedAccounts
        {
            get
            {
                return GenericGetFromConfig<List<UserToken>>(ProtectedAccountsKey, ref _protectedAccounts);
            }
            set
            {
                GenericSaveToConfig<List<UserToken>>(ProtectedAccountsKey, ref _protectedAccounts, value);
            }
        }

        public static bool? BackgroundLoadColumns
        {
            get
            {
                return GenericGetFromConfig<bool?>(BGLoadColumns, ref _backgroundLoadColumns);
            }
            set
            {
                GenericSaveToConfig<bool?>(BGLoadColumns, ref _backgroundLoadColumns, value);
            }
        }

        public static List<TwitterStatusTask> TweetTasks
        {
            get
            {
                return GenericGetFromConfig<List<TwitterStatusTask>>(TweetTasksKey, ref _tweetTasks);
            }
            set
            {
                GenericSaveToConfig<List<TwitterStatusTask>>(TweetTasksKey, ref _tweetTasks, value);
            }
        }

        public static List<UserToken> Accounts
        {
            get
            {
                return GenericGetFromConfig<List<UserToken>>(AccountsKey, ref _accounts);
            }
            set
            {
                GenericSaveToConfig<List<UserToken>>(AccountsKey, ref _accounts, value);
            }
        }

        public static ObservableCollection<TwitterResource> Columns
        {
            get
            {
                return GenericGetFromConfig<ObservableCollection<TwitterResource>>(ColumnsKey, ref _columns);
            }
            set
            {

                GenericSaveToConfig<ObservableCollection<TwitterResource>>(ColumnsKey, ref _columns, value);
            }
        }

        public static ColumnFilter FilterGlobal
        {
            get
            {
                return GenericGetFromConfig<ColumnFilter>(GlobalFilterKey, ref _globalFilter);
            }
            set
            {
                GenericSaveToConfig<ColumnFilter>(GlobalFilterKey, ref _globalFilter, value);
            }
        }


        public static bool? FollowMessageShown
        {
            get
            {
                return GenericGetFromConfig<bool?>(FollowMsg, ref _followMessageShown);
            }
            set
            {
                GenericSaveToConfig<bool?>(FollowMsg, ref _followMessageShown, value);
            }

        }
        private static T GenericGetFromConfig<T>(string key, ref T element) where T : new()
        {
            if (element != null)
                return element;

            IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;

            try
            {
                if (!config.TryGetValue<T>(key, out element))
                {
                    element = new T();
                    config.Add(key, element);
                    config.Save();
                }
            }
            catch (InvalidCastException)
            {
                config.Remove(key);
            }
            catch (Exception)
            {
            }

            if (element == null)
                element = new T();

            return element;
        }
        

        private static void GenericSaveToConfig<T>(string Key, ref T element, T value) where T : new()
        {
            if (value == null)
                return;

            IsolatedStorageSettings conf = IsolatedStorageSettings.ApplicationSettings;

            try
            {
                element = value;
                if (conf.Contains(Key))
                    conf[Key] = value;
                else
                    conf.Add(Key, value);
                conf.Save();
            }
            catch (Exception)
            {
                throw;
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

        public static void SaveTasks()
        {
            TweetTasks = _tweetTasks;
        }

        public static void SaveProtectedAccounts()
        {
            ProtectedAccounts = _protectedAccounts;
        }

        public static void Dispose()
        {
            _accounts = null;
            _backgroundLoadColumns = null;
            _columns = null;
            _followMessageShown = null;
            _protectedAccounts = null;
            _tweetTasks = null;
        }

		public static void SaveFilters()
        {
            Filters = _filters;
        }
    }
}
