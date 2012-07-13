using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Threading;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
#if !BACKGROUND_AGENT
using Ocell.Library.Filtering;
#endif


namespace Ocell.Library
{
    public static partial class Config
    {
        private static Mutex _mutex = new Mutex(false, "Ocell.IsolatedStorageSettings_MUTEX");
        private const int MutexTimeout = 1000;
#if !BACKGROUND_AGENT
        private const string FollowMsg = "FOLLOWMSG";
        private const string ProtectedAccountsKey = "PROTECTEDACC";
        private const string FiltersKey = "FILTERS";
        private const string GlobalFilterKey = "GLOBALFILTER";
        private const string RTAsMentionsKey = "RTASMENTIONS";
        private const string TweetsPerReqKey = "TWEETSXREQ";
        private const string DefaultMuteTimeKey = "DEFAULTMUTETIME";
        private const string DraftsKey = "DRAFTS";
        private const string ReadLaterCredsKey = "READLATERCREDS";
        private const string BackgroundsKey ="BACKGROUNDS";
        private const string FirstInitKey = "ISFIRSTINIT";
#endif

        private const string AccountsKey = "ACCOUNTS";
        private const string ColumnsKey = "COLUMNS";
        private const string TweetTasksKey = "TWEETTASKS";
        private const string BGLoadColumns = "BGLOADCOLUMNS";

        private static List<UserToken> _accounts;
        private static ObservableCollection<TwitterResource> _columns;
        private static List<TwitterStatusTask> _tweetTasks;
        private static bool? _backgroundLoadColumns;

#if !BACKGROUND_AGENT
        private static bool? _followMessageShown;
        private static List<UserToken> _protectedAccounts;
        private static List<ColumnFilter> _filters;
        private static ColumnFilter _globalFilter;
        private static bool? _retweetAsMentions;
        private static int? _tweetsPerRequest;
        private static TimeSpan? _defaultMuteTime;
        private static List<TwitterDraft> _drafts;
        private static ReadLaterCredentials _readLaterCredentials;
        private static OcellTheme _backgroundUrl;
        private static bool? _firstInit;
        
#endif

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
#if !BACKGROUND_AGENT
        public static bool? FirstInit
        {
            get
            {
                return GenericGetFromConfig(FirstInitKey, ref _firstInit);
            }
            set
            {
                GenericSaveToConfig(FirstInitKey, ref _firstInit, value);
            }
        }

        public static OcellTheme Background
        {
            get
            {
                return GenericGetFromConfig<OcellTheme>(BackgroundsKey, ref _backgroundUrl);
            }
            set
            {
                GenericSaveToConfig<OcellTheme>(BackgroundsKey, ref _backgroundUrl, value);
            }
        }

        public static ReadLaterCredentials ReadLaterCredentials
        {
            get
            {
                return GenericGetFromConfig<ReadLaterCredentials>(ReadLaterCredsKey, ref _readLaterCredentials);
            }
            set
            {
                GenericSaveToConfig(ReadLaterCredsKey, ref _readLaterCredentials, value);
            }
        }


        public static List<TwitterDraft> Drafts
        {
            get
            {
                return GenericGetFromConfig<List<TwitterDraft>>(DraftsKey, ref _drafts);
            }
            set
            {
                GenericSaveToConfig<List<TwitterDraft>>(DraftsKey, ref _drafts, value);
            }
        }

        public static TimeSpan? DefaultMuteTime
        {
            get
            {
                return GenericGetFromConfig<TimeSpan?>(DefaultMuteTimeKey, ref _defaultMuteTime);
            }
            set
            {
                GenericSaveToConfig<TimeSpan?>(DefaultMuteTimeKey, ref _defaultMuteTime, value);
            }
        }
        

        public static int? TweetsPerRequest
        {
            get
            {
                return GenericGetFromConfig<int?>(TweetsPerReqKey, ref _tweetsPerRequest);
            }
            set
            {
                GenericSaveToConfig<int?>(TweetsPerReqKey, ref _tweetsPerRequest, value);
            }
        }

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
#endif
        private static T GenericGetFromConfig<T>(string key, ref T element) where T : new()
        {
            if (element != null)
                return element;

            if (_mutex.WaitOne(MutexTimeout))
            {

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
                catch (InvalidCastException e)
                {
                    e.StackTrace.ToString();
                    config.Remove(key);
                    config.Save();
                }
                catch (Exception)
                {
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }

            if (element == null)
                element = new T();

            return element;
        }


        private static void GenericSaveToConfig<T>(string Key, ref T element, T value) where T : new()
        {
            if (value == null)
                return;

            if (_mutex.WaitOne(MutexTimeout))
            {
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
                finally
                {
                    _mutex.ReleaseMutex();
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

        public static void SaveTasks()
        {
            TweetTasks = _tweetTasks;
        }

        public static void ClearAll()
        {
            if (_mutex.WaitOne(MutexTimeout))
            {
                try
                {
                    IsolatedStorageSettings.ApplicationSettings.Clear();
                }
                finally
                {
                    _mutex.ReleaseMutex();
                }
            }
        }

        public static void ClearStaticValues()
        {
            _accounts = null;
            _backgroundLoadColumns = null;
            _columns = null;
            _tweetTasks = null;

#if !BACKGROUND_AGENT
            _defaultMuteTime = null;
            _drafts = null;
            _filters = null;
            _followMessageShown = null;
            _globalFilter = null;
            _protectedAccounts = null;
            _readLaterCredentials = null;
            _retweetAsMentions = null;
            _tweetsPerRequest = null;
#endif
        }
#if !BACKGROUND_AGENT
        public static void SaveProtectedAccounts()
        {
            ProtectedAccounts = _protectedAccounts;
        }

		public static void SaveFilters()
        {
            Filters = _filters;
        }

        public static void SaveDrafts()
        {
            Drafts = _drafts;
        }
#endif
    }
}
