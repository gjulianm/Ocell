﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ocell.Library.Twitter;
using Ocell.Library.Tasks;

#if !BACKGROUND_AGENT
using Ocell.Library.Filtering;
using BufferAPI;
#endif


namespace Ocell.Library
{
	public static partial class Config
	{	
	#region Properties for Background Agent and main app

		private static List<UserToken> _Accounts;
		public static List<UserToken> Accounts
		{
			get 
			{
				return GenericGetFromConfig<List<UserToken>>("ACCOUNTS", ref _Accounts);
			}
			set
			{
				GenericSaveToConfig<List<UserToken>>("ACCOUNTS", ref _Accounts, value);
			}
		}

		public static void SaveAccounts()
		{
			Accounts = _Accounts;
		}

		private static ObservableCollection<TwitterResource> _Columns;
		public static ObservableCollection<TwitterResource> Columns
		{
			get 
			{
				return GenericGetFromConfig<ObservableCollection<TwitterResource>>("COLUMNS", ref _Columns);
			}
			set
			{
				GenericSaveToConfig<ObservableCollection<TwitterResource>>("COLUMNS", ref _Columns, value);
			}
		}

		public static void SaveColumns()
		{
			Columns = _Columns;
		}

		private static List<TwitterStatusTask> _TweetTasks;
		public static List<TwitterStatusTask> TweetTasks
		{
			get 
			{
				return GenericGetFromConfig<List<TwitterStatusTask>>("TWEETTASKS", ref _TweetTasks);
			}
			set
			{
				GenericSaveToConfig<List<TwitterStatusTask>>("TWEETTASKS", ref _TweetTasks, value);
			}
		}

		public static void SaveTweetTasks()
		{
			TweetTasks = _TweetTasks;
		}

		private static bool? _BackgroundLoadColumns;
		public static bool? BackgroundLoadColumns
		{
			get 
			{
				return GenericGetFromConfig<bool?>("BGLOADCOLUMNS", ref _BackgroundLoadColumns);
			}
			set
			{
				GenericSaveToConfig<bool?>("BGLOADCOLUMNS", ref _BackgroundLoadColumns, value);
			}
		}
	#endregion

	#region Properties only for main app
	#if !BACKGROUND_AGENT

		private static bool? _FollowMessageShown;
		public static bool? FollowMessageShown
		{
			get 
			{
				return GenericGetFromConfig<bool?>("FOLLOWMSG", ref _FollowMessageShown);
			}
			set
			{
				GenericSaveToConfig<bool?>("FOLLOWMSG", ref _FollowMessageShown, value);
			}
		}

		private static List<UserToken> _ProtectedAccounts;
		public static List<UserToken> ProtectedAccounts
		{
			get 
			{
				return GenericGetFromConfig<List<UserToken>>("PROTECTEDACC", ref _ProtectedAccounts);
			}
			set
			{
				GenericSaveToConfig<List<UserToken>>("PROTECTEDACC", ref _ProtectedAccounts, value);
			}
		}

		public static void SaveProtectedAccounts()
		{
			ProtectedAccounts = _ProtectedAccounts;
		}

		private static List<ColumnFilter> _Filters;
		public static List<ColumnFilter> Filters
		{
			get 
			{
				return GenericGetFromConfig<List<ColumnFilter>>("FILTERS", ref _Filters);
			}
			set
			{
				GenericSaveToConfig<List<ColumnFilter>>("FILTERS", ref _Filters, value);
			}
		}

		public static void SaveFilters()
		{
			Filters = _Filters;
		}

		private static ColumnFilter _GlobalFilter;
		public static ColumnFilter GlobalFilter
		{
			get 
			{
				return GenericGetFromConfig<ColumnFilter>("GLOBALFILTER", ref _GlobalFilter);
			}
			set
			{
				GenericSaveToConfig<ColumnFilter>("GLOBALFILTER", ref _GlobalFilter, value);
			}
		}

		private static bool? _RetweetAsMentions;
		public static bool? RetweetAsMentions
		{
			get 
			{
				return GenericGetFromConfig<bool?>("RTASMENTIONS", ref _RetweetAsMentions);
			}
			set
			{
				GenericSaveToConfig<bool?>("RTASMENTIONS", ref _RetweetAsMentions, value);
			}
		}

		private static int? _TweetsPerRequest;
		public static int? TweetsPerRequest
		{
			get 
			{
				return GenericGetFromConfig<int?>("TWEETSXREQ", ref _TweetsPerRequest);
			}
			set
			{
				GenericSaveToConfig<int?>("TWEETSXREQ", ref _TweetsPerRequest, value);
			}
		}

		private static TimeSpan? _DefaultMuteTime;
		public static TimeSpan? DefaultMuteTime
		{
			get 
			{
				return GenericGetFromConfig<TimeSpan?>("DEFAULTMUTETIME", ref _DefaultMuteTime);
			}
			set
			{
				GenericSaveToConfig<TimeSpan?>("DEFAULTMUTETIME", ref _DefaultMuteTime, value);
			}
		}

		private static List<TwitterDraft> _Drafts;
		public static List<TwitterDraft> Drafts
		{
			get 
			{
				return GenericGetFromConfig<List<TwitterDraft>>("DRAFTS", ref _Drafts);
			}
			set
			{
				GenericSaveToConfig<List<TwitterDraft>>("DRAFTS", ref _Drafts, value);
			}
		}

		public static void SaveDrafts()
		{
			Drafts = _Drafts;
		}

		private static ReadLaterCredentials _ReadLaterCredentials;
		public static ReadLaterCredentials ReadLaterCredentials
		{
			get 
			{
				return GenericGetFromConfig<ReadLaterCredentials>("READLATERCREDS", ref _ReadLaterCredentials);
			}
			set
			{
				GenericSaveToConfig<ReadLaterCredentials>("READLATERCREDS", ref _ReadLaterCredentials, value);
			}
		}

		private static OcellTheme _Background;
		public static OcellTheme Background
		{
			get 
			{
				return GenericGetFromConfig<OcellTheme>("BACKGROUNDSKEY", ref _Background);
			}
			set
			{
				GenericSaveToConfig<OcellTheme>("BACKGROUNDSKEY", ref _Background, value);
			}
		}

		private static bool? _FirstInit;
		public static bool? FirstInit
		{
			get 
			{
				return GenericGetFromConfig<bool?>("ISFIRSTINIT", ref _FirstInit);
			}
			set
			{
				GenericSaveToConfig<bool?>("ISFIRSTINIT", ref _FirstInit, value);
			}
		}

		private static int? _FontSize;
		public static int? FontSize
		{
			get 
			{
				return GenericGetFromConfig<int?>("FONTSIZE", ref _FontSize);
			}
			set
			{
				GenericSaveToConfig<int?>("FONTSIZE", ref _FontSize, value);
			}
		}

		private static Dictionary<string, long> _ReadPositions;
		public static Dictionary<string, long> ReadPositions
		{
			get 
			{
				return GenericGetFromConfig<Dictionary<string, long>>("READPOSITIONS", ref _ReadPositions);
			}
			set
			{
				GenericSaveToConfig<Dictionary<string, long>>("READPOSITIONS", ref _ReadPositions, value);
			}
		}

		public static void SaveReadPositions()
		{
			ReadPositions = _ReadPositions;
		}

		private static bool? _RecoverReadPositions;
		public static bool? RecoverReadPositions
		{
			get 
			{
				return GenericGetFromConfig<bool?>("RECOVERREAD", ref _RecoverReadPositions);
			}
			set
			{
				GenericSaveToConfig<bool?>("RECOVERREAD", ref _RecoverReadPositions, value);
			}
		}

		private static bool? _EnabledGeolocation;
		public static bool? EnabledGeolocation
		{
			get 
			{
				return GenericGetFromConfig<bool?>("GEOLOC_ENABLED", ref _EnabledGeolocation);
			}
			set
			{
				GenericSaveToConfig<bool?>("GEOLOC_ENABLED", ref _EnabledGeolocation, value);
			}
		}

		private static bool? _TweetGeotagging;
		public static bool? TweetGeotagging
		{
			get 
			{
				return GenericGetFromConfig<bool?>("GEOTAG_TWEETS", ref _TweetGeotagging);
			}
			set
			{
				GenericSaveToConfig<bool?>("GEOTAG_TWEETS", ref _TweetGeotagging, value);
			}
		}

		private static List<BufferProfile> _BufferProfiles;
		public static List<BufferProfile> BufferProfiles
		{
			get 
			{
				return GenericGetFromConfig<List<BufferProfile>>("BUFFER_PROFILES", ref _BufferProfiles);
			}
			set
			{
				GenericSaveToConfig<List<BufferProfile>>("BUFFER_PROFILES", ref _BufferProfiles, value);
			}
		}

		public static void SaveBufferProfiles()
		{
			BufferProfiles = _BufferProfiles;
		}

		private static string _BufferAccessToken;
		public static string BufferAccessToken
		{
			get 
			{
				return GenericGetFromConfig<string>("BUFFER_ACCESS_TOKEN", ref _BufferAccessToken);
			}
			set
			{
				GenericSaveToConfig<string>("BUFFER_ACCESS_TOKEN", ref _BufferAccessToken, value);
			}
		}

		private static DateTime? _TrialStart;
		public static DateTime? TrialStart
		{
			get 
			{
				return GenericGetFromConfig<DateTime?>("TRIAL_INSTALLED_TIME", ref _TrialStart);
			}
			set
			{
				GenericSaveToConfig<DateTime?>("TRIAL_INSTALLED_TIME", ref _TrialStart, value);
			}
		}

		private static bool? _CouponCodeValidated;
		public static bool? CouponCodeValidated
		{
			get 
			{
				return GenericGetFromConfig<bool?>("COUPON_CODE", ref _CouponCodeValidated);
			}
			set
			{
				GenericSaveToConfig<bool?>("COUPON_CODE", ref _CouponCodeValidated, value);
			}
		}

		private static ColumnReloadOptions? _ReloadOptions;
		public static ColumnReloadOptions? ReloadOptions
		{
			get 
			{
				return GenericGetFromConfig<ColumnReloadOptions?>("RELOAD_OPS", ref _ReloadOptions);
			}
			set
			{
				GenericSaveToConfig<ColumnReloadOptions?>("RELOAD_OPS", ref _ReloadOptions, value);
			}
		}

		private static string _TopicPlace;
		public static string TopicPlace
		{
			get 
			{
				return GenericGetFromConfig<string>("TOPICS_NAME", ref _TopicPlace);
			}
			set
			{
				GenericSaveToConfig<string>("TOPICS_NAME", ref _TopicPlace, value);
			}
		}

		private static long? _TopicPlaceId;
		public static long? TopicPlaceId
		{
			get 
			{
				return GenericGetFromConfig<long?>("TOPICS_ID", ref _TopicPlaceId);
			}
			set
			{
				GenericSaveToConfig<long?>("TOPICS_ID", ref _TopicPlaceId, value);
			}
		}
	#endif
	#endregion

		public static void ClearStaticValues()
		{
			_Accounts = null;
			_Columns = null;
			_TweetTasks = null;
			_BackgroundLoadColumns = null;

#if !BACKGROUND_AGENT
			_FollowMessageShown = null;
			_ProtectedAccounts = null;
			_Filters = null;
			_GlobalFilter = null;
			_RetweetAsMentions = null;
			_TweetsPerRequest = null;
			_DefaultMuteTime = null;
			_Drafts = null;
			_ReadLaterCredentials = null;
			_Background = null;
			_FirstInit = null;
			_FontSize = null;
			_ReadPositions = null;
			_RecoverReadPositions = null;
			_EnabledGeolocation = null;
			_TweetGeotagging = null;
			_BufferProfiles = null;
			_BufferAccessToken = null;
			_TrialStart = null;
			_CouponCodeValidated = null;
			_ReloadOptions = null;
			_TopicPlace = null;
			_TopicPlaceId = null;
#endif
		}

		static void GenerateDefaultDictionary()
		{
			defaultValues = new Dictionary<string, object>();

			defaultValues.Add("BGLOADCOLUMNS", true);

#if !BACKGROUND_AGENT
			defaultValues.Add("FOLLOWMSG", false);
			defaultValues.Add("RTASMENTIONS", true);
			defaultValues.Add("TWEETSXREQ", 40);
			defaultValues.Add("FONTSIZE", 20);
			defaultValues.Add("RECOVERREAD", true);
			defaultValues.Add("GEOTAG_TWEETS", true);
			defaultValues.Add("TRIAL_INSTALLED_TIME", DateTime.MaxValue);
			defaultValues.Add("COUPON_CODE", false);
			defaultValues.Add("RELOAD_OPS", ColumnReloadOptions.KeepPosition);
			defaultValues.Add("TOPICS_ID", -1);
#endif
		}
	}
}
