using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using TweetSharp;

namespace Ocell.Library
{
    public static class Config 
    {
<<<<<<< HEAD
        private const string AccountsKey = "ACCOUNTS";
        private const string ColumnsKey = "COLUMNS";
        private const string FollowMsg = "FOLLOWMSG";
        private const string TweetTasksKey = "TWEETTASKS";
        private const string BGLoadColumns = "BGLOADCOLUMNS";
        private const string ProtectedAccountsKey = "PROTECTEDACC";
=======
        private static readonly string AccountsKey = "ACCOUNTS";
        private static readonly string ColumnsKey = "COLUMNS";
        private static readonly string FollowMsg = "FOLLOWMSG";
        private static readonly string TweetTasksKey = "TWEETTASKS";
        private static readonly string BGLoadColumns = "BGLOADCOLUMNS";
        private static readonly string ProtectedAccountsKey = "PROTECTEDACC";
        private static readonly string FiltersKey = "FILTERS";
        private static readonly string GlobalFilterKey = "GLOBALFILTER";
>>>>>>> feature/collectionviewsource

        private static List<UserToken> _accounts;
        private static ObservableCollection<TwitterResource> _columns;
        private static bool? _followMessageShown;
        private static List<ITweetableTask> _tweetTasks;
        private static bool? _backgroundLoadColumns;
        private static List<UserToken> _protectedAccounts;
        private static List<ColumnFilter> _filters;
        private static ColumnFilter _globalFilter;

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

        public static List<ITweetableTask> TweetTasks
        {
            get
            {
                return GenericGetFromConfig<List<ITweetableTask>>(TweetTasksKey, ref _tweetTasks);
            }
            set
            {
                GenericSaveToConfig<List<ITweetableTask>>(TweetTasksKey, ref _tweetTasks, value);
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

<<<<<<< HEAD
        private static T GenericGetFromConfig<T>(string key, ref T element) where T : new()
=======
        public static bool? FollowMessageShown
        {
            get
            {
                return GenericGetFromConfig<bool?>(FollowMsg, ref _FollowMessageShown);
            }
            set
            {
                GenericSaveToConfig<bool?>(FollowMsg, ref _FollowMessageShown, value);
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

        private static T GenericGetFromConfig<T>(string Key, ref T element) where T : new()
>>>>>>> feature/collectionviewsource
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

<<<<<<< HEAD
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

=======
>>>>>>> feature/collectionviewsource
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

<<<<<<< HEAD
        public static void Dispose()
        {
            _accounts = null;
            _backgroundLoadColumns = null;
            _columns = null;
            _followMessageShown = null;
            _protectedAccounts = null;
            _tweetTasks = null;
=======
        public static void SaveFilters()
        {
            Filters = _filters;
>>>>>>> feature/collectionviewsource
        }
    }
}
