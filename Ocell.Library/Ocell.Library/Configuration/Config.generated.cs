using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ocell.Library.Twitter;
using Ocell.Library.Tasks;
using BufferAPI;
using AncoraMVVM.Base.Interfaces;
#if !BACKGROUND_AGENT
using Ocell.Library.Filtering;
using BufferAPI;
#endif


namespace Ocell.Library
{
	public static partial class Config
	{	
	#region Properties for Background Agent and main app

		private static ConfigItem<List<UserToken>> AccountsconfigItem = new ConfigItem<List<UserToken>> {
			Key = "ACCOUNTS",
					};

		public static List<UserToken> Accounts
		{
			get 
			{
				return AccountsconfigItem.Get();
			}
			set
			{
				AccountsconfigItem.Set(value);
			}
		}

		public static void SaveAccounts()
		{
			AccountsconfigItem.Set(AccountsconfigItem.Get());
		}

		private static ConfigItem<ObservableCollection<TwitterResource>> ColumnsconfigItem = new ConfigItem<ObservableCollection<TwitterResource>> {
			Key = "COLUMNS",
					};

		public static ObservableCollection<TwitterResource> Columns
		{
			get 
			{
				return ColumnsconfigItem.Get();
			}
			set
			{
				ColumnsconfigItem.Set(value);
			}
		}

		public static void SaveColumns()
		{
			ColumnsconfigItem.Set(ColumnsconfigItem.Get());
		}

		private static ConfigItem<List<TwitterStatusTask>> TweetTasksconfigItem = new ConfigItem<List<TwitterStatusTask>> {
			Key = "TWEETTASKS",
					};

		public static List<TwitterStatusTask> TweetTasks
		{
			get 
			{
				return TweetTasksconfigItem.Get();
			}
			set
			{
				TweetTasksconfigItem.Set(value);
			}
		}

		public static void SaveTweetTasks()
		{
			TweetTasksconfigItem.Set(TweetTasksconfigItem.Get());
		}

		private static ConfigItem<bool?> BackgroundLoadColumnsconfigItem = new ConfigItem<bool?> {
			Key = "BGLOADCOLUMNS",
			 DefaultValue = true 		};

		public static bool? BackgroundLoadColumns
		{
			get 
			{
				return BackgroundLoadColumnsconfigItem.Get();
			}
			set
			{
				BackgroundLoadColumnsconfigItem.Set(value);
			}
		}
	#endregion

	#region Properties only for main app
	#if !BACKGROUND_AGENT

		private static ConfigItem<bool?> FollowMessageShownconfigItem = new ConfigItem<bool?> {
			Key = "FOLLOWMSG",
			 DefaultValue = false 		};

		public static bool? FollowMessageShown
		{
			get 
			{
				return FollowMessageShownconfigItem.Get();
			}
			set
			{
				FollowMessageShownconfigItem.Set(value);
			}
		}

		private static ConfigItem<List<UserToken>> ProtectedAccountsconfigItem = new ConfigItem<List<UserToken>> {
			Key = "PROTECTEDACC",
					};

		public static List<UserToken> ProtectedAccounts
		{
			get 
			{
				return ProtectedAccountsconfigItem.Get();
			}
			set
			{
				ProtectedAccountsconfigItem.Set(value);
			}
		}

		public static void SaveProtectedAccounts()
		{
			ProtectedAccountsconfigItem.Set(ProtectedAccountsconfigItem.Get());
		}

		private static ConfigItem<List<ColumnFilter>> FiltersconfigItem = new ConfigItem<List<ColumnFilter>> {
			Key = "FILTERS",
					};

		public static List<ColumnFilter> Filters
		{
			get 
			{
				return FiltersconfigItem.Get();
			}
			set
			{
				FiltersconfigItem.Set(value);
			}
		}

		public static void SaveFilters()
		{
			FiltersconfigItem.Set(FiltersconfigItem.Get());
		}

		private static ConfigItem<ColumnFilter> GlobalFilterconfigItem = new ConfigItem<ColumnFilter> {
			Key = "GLOBALFILTER",
					};

		public static ColumnFilter GlobalFilter
		{
			get 
			{
				return GlobalFilterconfigItem.Get();
			}
			set
			{
				GlobalFilterconfigItem.Set(value);
			}
		}

		private static ConfigItem<bool?> RetweetAsMentionsconfigItem = new ConfigItem<bool?> {
			Key = "RTASMENTIONS",
			 DefaultValue = true 		};

		public static bool? RetweetAsMentions
		{
			get 
			{
				return RetweetAsMentionsconfigItem.Get();
			}
			set
			{
				RetweetAsMentionsconfigItem.Set(value);
			}
		}

		private static ConfigItem<int?> TweetsPerRequestconfigItem = new ConfigItem<int?> {
			Key = "TWEETSXREQ",
			 DefaultValue = 40 		};

		public static int? TweetsPerRequest
		{
			get 
			{
				return TweetsPerRequestconfigItem.Get();
			}
			set
			{
				TweetsPerRequestconfigItem.Set(value);
			}
		}

		private static ConfigItem<TimeSpan?> DefaultMuteTimeconfigItem = new ConfigItem<TimeSpan?> {
			Key = "DEFAULTMUTETIME",
					};

		public static TimeSpan? DefaultMuteTime
		{
			get 
			{
				return DefaultMuteTimeconfigItem.Get();
			}
			set
			{
				DefaultMuteTimeconfigItem.Set(value);
			}
		}

		private static ConfigItem<List<TwitterDraft>> DraftsconfigItem = new ConfigItem<List<TwitterDraft>> {
			Key = "DRAFTS",
					};

		public static List<TwitterDraft> Drafts
		{
			get 
			{
				return DraftsconfigItem.Get();
			}
			set
			{
				DraftsconfigItem.Set(value);
			}
		}

		public static void SaveDrafts()
		{
			DraftsconfigItem.Set(DraftsconfigItem.Get());
		}

		private static ConfigItem<ReadLaterCredentials> ReadLaterCredentialsconfigItem = new ConfigItem<ReadLaterCredentials> {
			Key = "READLATERCREDS",
					};

		public static ReadLaterCredentials ReadLaterCredentials
		{
			get 
			{
				return ReadLaterCredentialsconfigItem.Get();
			}
			set
			{
				ReadLaterCredentialsconfigItem.Set(value);
			}
		}

		private static ConfigItem<OcellTheme> BackgroundconfigItem = new ConfigItem<OcellTheme> {
			Key = "BACKGROUNDSKEY",
					};

		public static OcellTheme Background
		{
			get 
			{
				return BackgroundconfigItem.Get();
			}
			set
			{
				BackgroundconfigItem.Set(value);
			}
		}

		private static ConfigItem<bool?> FirstInitconfigItem = new ConfigItem<bool?> {
			Key = "ISFIRSTINIT",
					};

		public static bool? FirstInit
		{
			get 
			{
				return FirstInitconfigItem.Get();
			}
			set
			{
				FirstInitconfigItem.Set(value);
			}
		}

		private static ConfigItem<int?> FontSizeconfigItem = new ConfigItem<int?> {
			Key = "FONTSIZE",
			 DefaultValue = 20 		};

		public static int? FontSize
		{
			get 
			{
				return FontSizeconfigItem.Get();
			}
			set
			{
				FontSizeconfigItem.Set(value);
			}
		}

		private static ConfigItem<Dictionary<string, long>> ReadPositionsconfigItem = new ConfigItem<Dictionary<string, long>> {
			Key = "READPOSITIONS",
					};

		public static Dictionary<string, long> ReadPositions
		{
			get 
			{
				return ReadPositionsconfigItem.Get();
			}
			set
			{
				ReadPositionsconfigItem.Set(value);
			}
		}

		public static void SaveReadPositions()
		{
			ReadPositionsconfigItem.Set(ReadPositionsconfigItem.Get());
		}

		private static ConfigItem<bool?> RecoverReadPositionsconfigItem = new ConfigItem<bool?> {
			Key = "RECOVERREAD",
			 DefaultValue = true 		};

		public static bool? RecoverReadPositions
		{
			get 
			{
				return RecoverReadPositionsconfigItem.Get();
			}
			set
			{
				RecoverReadPositionsconfigItem.Set(value);
			}
		}

		private static ConfigItem<bool?> EnabledGeolocationconfigItem = new ConfigItem<bool?> {
			Key = "GEOLOC_ENABLED",
					};

		public static bool? EnabledGeolocation
		{
			get 
			{
				return EnabledGeolocationconfigItem.Get();
			}
			set
			{
				EnabledGeolocationconfigItem.Set(value);
			}
		}

		private static ConfigItem<bool?> TweetGeotaggingconfigItem = new ConfigItem<bool?> {
			Key = "GEOTAG_TWEETS",
			 DefaultValue = true 		};

		public static bool? TweetGeotagging
		{
			get 
			{
				return TweetGeotaggingconfigItem.Get();
			}
			set
			{
				TweetGeotaggingconfigItem.Set(value);
			}
		}

		private static ConfigItem<List<BufferProfile>> BufferProfilesconfigItem = new ConfigItem<List<BufferProfile>> {
			Key = "BUFFER_PROFILES",
					};

		public static List<BufferProfile> BufferProfiles
		{
			get 
			{
				return BufferProfilesconfigItem.Get();
			}
			set
			{
				BufferProfilesconfigItem.Set(value);
			}
		}

		public static void SaveBufferProfiles()
		{
			BufferProfilesconfigItem.Set(BufferProfilesconfigItem.Get());
		}

		private static ConfigItem<string> BufferAccessTokenconfigItem = new ConfigItem<string> {
			Key = "BUFFER_ACCESS_TOKEN",
					};

		public static string BufferAccessToken
		{
			get 
			{
				return BufferAccessTokenconfigItem.Get();
			}
			set
			{
				BufferAccessTokenconfigItem.Set(value);
			}
		}

		private static ConfigItem<DateTime?> TrialStartconfigItem = new ConfigItem<DateTime?> {
			Key = "TRIAL_INSTALLED_TIME",
			 DefaultValue = DateTime.MaxValue 		};

		public static DateTime? TrialStart
		{
			get 
			{
				return TrialStartconfigItem.Get();
			}
			set
			{
				TrialStartconfigItem.Set(value);
			}
		}

		private static ConfigItem<bool?> CouponCodeValidatedconfigItem = new ConfigItem<bool?> {
			Key = "COUPON_CODE",
			 DefaultValue = false 		};

		public static bool? CouponCodeValidated
		{
			get 
			{
				return CouponCodeValidatedconfigItem.Get();
			}
			set
			{
				CouponCodeValidatedconfigItem.Set(value);
			}
		}

		private static ConfigItem<ColumnReloadOptions?> ReloadOptionsconfigItem = new ConfigItem<ColumnReloadOptions?> {
			Key = "RELOAD_OPS",
			 DefaultValue = ColumnReloadOptions.KeepPosition 		};

		public static ColumnReloadOptions? ReloadOptions
		{
			get 
			{
				return ReloadOptionsconfigItem.Get();
			}
			set
			{
				ReloadOptionsconfigItem.Set(value);
			}
		}

		private static ConfigItem<string> TopicPlaceconfigItem = new ConfigItem<string> {
			Key = "TOPICS_NAME",
					};

		public static string TopicPlace
		{
			get 
			{
				return TopicPlaceconfigItem.Get();
			}
			set
			{
				TopicPlaceconfigItem.Set(value);
			}
		}

		private static ConfigItem<long?> TopicPlaceIdconfigItem = new ConfigItem<long?> {
			Key = "TOPICS_ID",
			 DefaultValue = -1 		};

		public static long? TopicPlaceId
		{
			get 
			{
				return TopicPlaceIdconfigItem.Get();
			}
			set
			{
				TopicPlaceIdconfigItem.Set(value);
			}
		}
	#endif
	#endregion


	}
}
