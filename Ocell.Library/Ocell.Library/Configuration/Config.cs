using AncoraMVVM.Base.Interfaces;
using BufferAPI;
using Ocell.Library.Filtering;
using Ocell.Library.Tasks;
using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TweetSharp;


namespace Ocell.Library
{
    public static class Config
    {
        public static void InitConfig() { } // Sometimes .NET decides to not load the configuration.

        const string pushEnabledKey = "PUSHENABLED";


#if OCELL_FULL
        public readonly static ConfigItem<bool?> PushEnabledConfigItem = new ConfigItem<bool?>
        {
            Key = pushEnabledKey,
            DefaultValue = null
        };
#endif

        public static bool? PushEnabled
        {
            get
            {
#if OCELL_FULL
                return PushEnabledConfigItem.Value;
#else
                return false;
#endif
            }
            set
            {
#if OCELL_FULL
                PushEnabledConfigItem.Value = value;
#endif
            }
        }

        #region Properties for Background Agent and main app

        public readonly static ConfigItem<List<UserToken>> Accounts = new ConfigItem<List<UserToken>>
        {
            Key = "ACCOUNTS",
            DefaultValue = new List<UserToken>()
        };


        public static void SaveAccounts()
        {
            Accounts.Value = Accounts.Value;
        }

        public readonly static ConfigItem<ObservableCollection<TwitterResource>> Columns = new ConfigItem<ObservableCollection<TwitterResource>>
        {
            Key = "COLUMNS",
            DefaultValue = new ObservableCollection<TwitterResource>()
        };


        public static void SaveColumns()
        {
            Columns.Value = Columns.Value;
        }

        public readonly static ConfigItem<List<TwitterStatusTask>> TweetTasks = new ConfigItem<List<TwitterStatusTask>>
        {
            Key = "TWEETTASKS",
            DefaultValue = new List<TwitterStatusTask>()
        };


        public static void SaveTweetTasks()
        {
            TweetTasks.Value = TweetTasks.Value;
        }

        public readonly static ConfigItem<bool?> BackgroundLoadColumns = new ConfigItem<bool?>
        {
            Key = "BGLOADCOLUMNS",
            DefaultValue = true
        };

        #endregion

        #region Properties only for main app
#if !BACKGROUND_AGENT

        public readonly static ConfigItem<bool?> FollowMessageShown = new ConfigItem<bool?>
        {
            Key = "FOLLOWMSG",
            DefaultValue = false
        };


        public readonly static ConfigItem<List<UserToken>> ProtectedAccounts = new ConfigItem<List<UserToken>>
        {
            Key = "PROTECTEDACC",
            DefaultValue = new List<UserToken>()
        };


        public static void SaveProtectedAccounts()
        {
            ProtectedAccounts.Value = ProtectedAccounts.Value;
        }

        public readonly static ConfigItem<Dictionary<TwitterResource, ObservableCollection<ElementFilter<ITweetable>>>> Filters = new ConfigItem<Dictionary<TwitterResource, ObservableCollection<ElementFilter<ITweetable>>>>
        {
            Key = "FILTERS",
            DefaultValue = new Dictionary<TwitterResource, ObservableCollection<ElementFilter<ITweetable>>>()
        };

        public static void SaveFilters()
        {
            Filters.Value = Filters.Value;
        }

        public readonly static ConfigItem<ObservableCollection<ElementFilter<ITweetable>>> GlobalFilter = new ConfigItem<ObservableCollection<ElementFilter<ITweetable>>>
        {
            Key = "GLOBALFILTER",
            DefaultValue = new ObservableCollection<ElementFilter<ITweetable>>()
        };

        public static void SaveGlobalFilter()
        {
            GlobalFilter.Value = GlobalFilter.Value;
        }

        public readonly static ConfigItem<bool?> RetweetAsMentions = new ConfigItem<bool?>
        {
            Key = "RTASMENTIONS",
            DefaultValue = true
        };


        public readonly static ConfigItem<int?> TweetsPerRequest = new ConfigItem<int?>
        {
            Key = "TWEETSXREQ",
            DefaultValue = 40
        };


        public readonly static ConfigItem<TimeSpan?> DefaultMuteTime = new ConfigItem<TimeSpan?>
        {
            Key = "DEFAULTMUTETIME",
            DefaultValue = TimeSpan.FromHours(8)
        };


        public readonly static ConfigItem<ObservableCollection<TwitterDraft>> Drafts = new ConfigItem<ObservableCollection<TwitterDraft>>
        {
            Key = "DRAFTS",
            DefaultValue = new ObservableCollection<TwitterDraft>()
        };


        public static void SaveDrafts()
        {
            Drafts.Value = Drafts.Value;
        }

        public readonly static ConfigItem<ReadLaterCredentials> ReadLaterCredentials = new ConfigItem<ReadLaterCredentials>
        {
            Key = "READLATERCREDS",
            DefaultValue = new ReadLaterCredentials()
        };

        public readonly static ConfigItem<OcellTheme> Background = new ConfigItem<OcellTheme>
        {
            Key = "BACKGROUNDSKEY",
            DefaultValue = new OcellTheme { Background = BackgroundType.ThemeDependant }
        };


        public readonly static ConfigItem<bool?> FirstInit = new ConfigItem<bool?>
        {
            Key = "ISFIRSTINIT"
        };


        public readonly static ConfigItem<int?> FontSize = new ConfigItem<int?>
        {
            Key = "FONTSIZE",
            DefaultValue = 20
        };


        public readonly static ConfigItem<Dictionary<string, long>> ReadPositions = new ConfigItem<Dictionary<string, long>>
        {
            Key = "READPOSITIONS",
            DefaultValue = new Dictionary<string, long>()
        };


        public static void SaveReadPositions()
        {
            ReadPositions.Value = ReadPositions.Value;
        }

        public readonly static ConfigItem<bool?> RecoverReadPositions = new ConfigItem<bool?>
        {
            Key = "RECOVERREAD",
            DefaultValue = true
        };


        public readonly static ConfigItem<bool?> EnabledGeolocation = new ConfigItem<bool?>
        {
            Key = "GEOLOC_ENABLED"
        };


        public readonly static ConfigItem<bool?> TweetGeotagging = new ConfigItem<bool?>
        {
            Key = "GEOTAG_TWEETS",
            DefaultValue = true
        };


        public readonly static ConfigItem<List<BufferProfile>> BufferProfiles = new ConfigItem<List<BufferProfile>>
        {
            Key = "BUFFER_PROFILES",
            DefaultValue = new List<BufferProfile>()
        };


        public static void SaveBufferProfiles()
        {
            BufferProfiles.Value = BufferProfiles.Value;
        }

        public readonly static ConfigItem<string> BufferAccessToken = new ConfigItem<string>
        {
            Key = "BUFFER_ACCESS_TOKEN"
        };


        public readonly static ConfigItem<DateTime?> TrialStart = new ConfigItem<DateTime?>
        {
            Key = "TRIAL_INSTALLED_TIME",
            DefaultValue = DateTime.MaxValue
        };


        public readonly static ConfigItem<bool?> CouponCodeValidated = new ConfigItem<bool?>
        {
            Key = "COUPON_CODE",
            DefaultValue = false
        };


        public readonly static ConfigItem<ColumnReloadOptions?> ReloadOptions = new ConfigItem<ColumnReloadOptions?>
        {
            Key = "RELOAD_OPS",
            DefaultValue = ColumnReloadOptions.KeepPosition
        };


        public readonly static ConfigItem<string> TopicPlace = new ConfigItem<string>
        {
            Key = "TOPICS_NAME"
        };


        public readonly static ConfigItem<long?> TopicPlaceId = new ConfigItem<long?>
        {
            Key = "TOPICS_ID",
            DefaultValue = -1
        };

        public readonly static ConfigItem<EmbeddedWebOptions?> WebOptions = new ConfigItem<EmbeddedWebOptions?>
        {
            Key = "WEB_OPS",
            DefaultValue = EmbeddedWebOptions.FullWeb
        };

#endif
        #endregion
    }
}
