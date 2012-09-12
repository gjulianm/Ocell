using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hammock;
using Hammock.Web;
using Ocell.Library.Twitter;
using Ocell.Library.Tasks;

#if !BACKGROUND_AGENT
using Ocell.Library.Filtering;
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
#endif
		}
	}
}
