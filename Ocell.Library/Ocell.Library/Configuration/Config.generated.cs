using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ocell.Library.Twitter;
using Ocell.Library.Tasks;
using BufferAPI;
using AncoraMVVM.Base.Interfaces;
#if !BACKGROUND_AGENT
using Ocell.Library.Filtering;

#endif


namespace Ocell.Library
{
    public static partial class Config
    {
        #region Properties for Background Agent and main app

        private static ConfigItem<List<UserToken>> AccountsConfigItem = new ConfigItem<List<UserToken>>
        {
            Key = "ACCOUNTS",
        };

        public static List<UserToken> Accounts
        {
            get
            {
                return AccountsConfigItem.Value;
            }
            set
            {
                AccountsConfigItem.Value = value;
            }
        }

        public static void SaveAccounts()
        {
            AccountsConfigItem.Value = AccountsConfigItem.Value;
        }

        private static ConfigItem<ObservableCollection<TwitterResource>> ColumnsConfigItem = new ConfigItem<ObservableCollection<TwitterResource>>
        {
            Key = "COLUMNS",
        };

        public static ObservableCollection<TwitterResource> Columns
        {
            get
            {
                return ColumnsConfigItem.Value;
            }
            set
            {
                ColumnsConfigItem.Value = value;
            }
        }

        public static void SaveColumns()
        {
            ColumnsConfigItem.Value = ColumnsConfigItem.Value;
        }

        private static ConfigItem<List<TwitterStatusTask>> TweetTasksConfigItem = new ConfigItem<List<TwitterStatusTask>>
        {
            Key = "TWEETTASKS",
        };

        public static List<TwitterStatusTask> TweetTasks
        {
            get
            {
                return TweetTasksConfigItem.Value;
            }
            set
            {
                TweetTasksConfigItem.Value = value;
            }
        }

        public static void SaveTweetTasks()
        {
            TweetTasksConfigItem.Value = TweetTasksConfigItem.Value;
        }

        private static ConfigItem<bool?> BackgroundLoadColumnsConfigItem = new ConfigItem<bool?>
        {
            Key = "BGLOADCOLUMNS",
            DefaultValue = true
        };

        public static bool? BackgroundLoadColumns
        {
            get
            {
                return BackgroundLoadColumnsConfigItem.Value;
            }
            set
            {
                BackgroundLoadColumnsConfigItem.Value = value;
            }
        }
        #endregion

        #region Properties only for main app
#if !BACKGROUND_AGENT

        private static ConfigItem<bool?> FollowMessageShownConfigItem = new ConfigItem<bool?>
        {
            Key = "FOLLOWMSG",
            DefaultValue = false
        };

        public static bool? FollowMessageShown
        {
            get
            {
                return FollowMessageShownConfigItem.Value;
            }
            set
            {
                FollowMessageShownConfigItem.Value = value;
            }
        }

        private static ConfigItem<List<UserToken>> ProtectedAccountsConfigItem = new ConfigItem<List<UserToken>>
        {
            Key = "PROTECTEDACC",
        };

        public static List<UserToken> ProtectedAccounts
        {
            get
            {
                return ProtectedAccountsConfigItem.Value;
            }
            set
            {
                ProtectedAccountsConfigItem.Value = value;
            }
        }

        public static void SaveProtectedAccounts()
        {
            ProtectedAccountsConfigItem.Value = ProtectedAccountsConfigItem.Value;
        }

        private static ConfigItem<List<ColumnFilter>> FiltersConfigItem = new ConfigItem<List<ColumnFilter>>
        {
            Key = "FILTERS",
        };

        public static List<ColumnFilter> Filters
        {
            get
            {
                return FiltersConfigItem.Value;
            }
            set
            {
                FiltersConfigItem.Value = value;
            }
        }

        public static void SaveFilters()
        {
            FiltersConfigItem.Value = FiltersConfigItem.Value;
        }

        private static ConfigItem<ColumnFilter> GlobalFilterConfigItem = new ConfigItem<ColumnFilter>
        {
            Key = "GLOBALFILTER",
        };

        public static ColumnFilter GlobalFilter
        {
            get
            {
                return GlobalFilterConfigItem.Value;
            }
            set
            {
                GlobalFilterConfigItem.Value = value;
            }
        }

        private static ConfigItem<bool?> RetweetAsMentionsConfigItem = new ConfigItem<bool?>
        {
            Key = "RTASMENTIONS",
            DefaultValue = true
        };

        public static bool? RetweetAsMentions
        {
            get
            {
                return RetweetAsMentionsConfigItem.Value;
            }
            set
            {
                RetweetAsMentionsConfigItem.Value = value;
            }
        }

        private static ConfigItem<int?> TweetsPerRequestConfigItem = new ConfigItem<int?>
        {
            Key = "TWEETSXREQ",
            DefaultValue = 40
        };

        public static int? TweetsPerRequest
        {
            get
            {
                return TweetsPerRequestConfigItem.Value;
            }
            set
            {
                TweetsPerRequestConfigItem.Value = value;
            }
        }

        private static ConfigItem<TimeSpan?> DefaultMuteTimeConfigItem = new ConfigItem<TimeSpan?>
        {
            Key = "DEFAULTMUTETIME",
        };

        public static TimeSpan? DefaultMuteTime
        {
            get
            {
                return DefaultMuteTimeConfigItem.Value;
            }
            set
            {
                DefaultMuteTimeConfigItem.Value = value;
            }
        }

        private static ConfigItem<List<TwitterDraft>> DraftsConfigItem = new ConfigItem<List<TwitterDraft>>
        {
            Key = "DRAFTS",
        };

        public static List<TwitterDraft> Drafts
        {
            get
            {
                return DraftsConfigItem.Value;
            }
            set
            {
                DraftsConfigItem.Value = value;
            }
        }

        public static void SaveDrafts()
        {
            DraftsConfigItem.Value = DraftsConfigItem.Value;
        }

        private static ConfigItem<ReadLaterCredentials> ReadLaterCredentialsConfigItem = new ConfigItem<ReadLaterCredentials>
        {
            Key = "READLATERCREDS",
        };

        public static ReadLaterCredentials ReadLaterCredentials
        {
            get
            {
                return ReadLaterCredentialsConfigItem.Value;
            }
            set
            {
                ReadLaterCredentialsConfigItem.Value = value;
            }
        }

        private static ConfigItem<OcellTheme> BackgroundConfigItem = new ConfigItem<OcellTheme>
        {
            Key = "BACKGROUNDSKEY",
        };

        public static OcellTheme Background
        {
            get
            {
                return BackgroundConfigItem.Value;
            }
            set
            {
                BackgroundConfigItem.Value = value;
            }
        }

        private static ConfigItem<bool?> FirstInitConfigItem = new ConfigItem<bool?>
        {
            Key = "ISFIRSTINIT",
        };

        public static bool? FirstInit
        {
            get
            {
                return FirstInitConfigItem.Value;
            }
            set
            {
                FirstInitConfigItem.Value = value;
            }
        }

        private static ConfigItem<int?> FontSizeConfigItem = new ConfigItem<int?>
        {
            Key = "FONTSIZE",
            DefaultValue = 20
        };

        public static int? FontSize
        {
            get
            {
                return FontSizeConfigItem.Value;
            }
            set
            {
                FontSizeConfigItem.Value = value;
            }
        }

        private static ConfigItem<Dictionary<string, long>> ReadPositionsConfigItem = new ConfigItem<Dictionary<string, long>>
        {
            Key = "READPOSITIONS",
        };

        public static Dictionary<string, long> ReadPositions
        {
            get
            {
                return ReadPositionsConfigItem.Value;
            }
            set
            {
                ReadPositionsConfigItem.Value = value;
            }
        }

        public static void SaveReadPositions()
        {
            ReadPositionsConfigItem.Value = ReadPositionsConfigItem.Value;
        }

        private static ConfigItem<bool?> RecoverReadPositionsConfigItem = new ConfigItem<bool?>
        {
            Key = "RECOVERREAD",
            DefaultValue = true
        };

        public static bool? RecoverReadPositions
        {
            get
            {
                return RecoverReadPositionsConfigItem.Value;
            }
            set
            {
                RecoverReadPositionsConfigItem.Value = value;
            }
        }

        private static ConfigItem<bool?> EnabledGeolocationConfigItem = new ConfigItem<bool?>
        {
            Key = "GEOLOC_ENABLED",
        };

        public static bool? EnabledGeolocation
        {
            get
            {
                return EnabledGeolocationConfigItem.Value;
            }
            set
            {
                EnabledGeolocationConfigItem.Value = value;
            }
        }

        private static ConfigItem<bool?> TweetGeotaggingConfigItem = new ConfigItem<bool?>
        {
            Key = "GEOTAG_TWEETS",
            DefaultValue = true
        };

        public static bool? TweetGeotagging
        {
            get
            {
                return TweetGeotaggingConfigItem.Value;
            }
            set
            {
                TweetGeotaggingConfigItem.Value = value;
            }
        }

        private static ConfigItem<List<BufferProfile>> BufferProfilesConfigItem = new ConfigItem<List<BufferProfile>>
        {
            Key = "BUFFER_PROFILES",
        };

        public static List<BufferProfile> BufferProfiles
        {
            get
            {
                return BufferProfilesConfigItem.Value;
            }
            set
            {
                BufferProfilesConfigItem.Value = value;
            }
        }

        public static void SaveBufferProfiles()
        {
            BufferProfilesConfigItem.Value = BufferProfilesConfigItem.Value;
        }

        private static ConfigItem<string> BufferAccessTokenConfigItem = new ConfigItem<string>
        {
            Key = "BUFFER_ACCESS_TOKEN",
        };

        public static string BufferAccessToken
        {
            get
            {
                return BufferAccessTokenConfigItem.Value;
            }
            set
            {
                BufferAccessTokenConfigItem.Value = value;
            }
        }

        private static ConfigItem<DateTime?> TrialStartConfigItem = new ConfigItem<DateTime?>
        {
            Key = "TRIAL_INSTALLED_TIME",
            DefaultValue = DateTime.MaxValue
        };

        public static DateTime? TrialStart
        {
            get
            {
                return TrialStartConfigItem.Value;
            }
            set
            {
                TrialStartConfigItem.Value = value;
            }
        }

        private static ConfigItem<bool?> CouponCodeValidatedConfigItem = new ConfigItem<bool?>
        {
            Key = "COUPON_CODE",
            DefaultValue = false
        };

        public static bool? CouponCodeValidated
        {
            get
            {
                return CouponCodeValidatedConfigItem.Value;
            }
            set
            {
                CouponCodeValidatedConfigItem.Value = value;
            }
        }

        private static ConfigItem<ColumnReloadOptions?> ReloadOptionsConfigItem = new ConfigItem<ColumnReloadOptions?>
        {
            Key = "RELOAD_OPS",
            DefaultValue = ColumnReloadOptions.KeepPosition
        };

        public static ColumnReloadOptions? ReloadOptions
        {
            get
            {
                return ReloadOptionsConfigItem.Value;
            }
            set
            {
                ReloadOptionsConfigItem.Value = value;
            }
        }

        private static ConfigItem<string> TopicPlaceConfigItem = new ConfigItem<string>
        {
            Key = "TOPICS_NAME",
        };

        public static string TopicPlace
        {
            get
            {
                return TopicPlaceConfigItem.Value;
            }
            set
            {
                TopicPlaceConfigItem.Value = value;
            }
        }

        private static ConfigItem<long?> TopicPlaceIdConfigItem = new ConfigItem<long?>
        {
            Key = "TOPICS_ID",
            DefaultValue = -1
        };

        public static long? TopicPlaceId
        {
            get
            {
                return TopicPlaceIdConfigItem.Value;
            }
            set
            {
                TopicPlaceIdConfigItem.Value = value;
            }
        }
#endif
        #endregion


    }
}
