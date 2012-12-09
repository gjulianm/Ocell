using System;
using System.Collections.Generic;
using System.Windows.Input;
using DanielVaughan.Windows;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.ReadLater.Instapaper;
using Ocell.Library.ReadLater.Pocket;
using Ocell.Library.Twitter;
using Ocell.Localization;

namespace Ocell.Settings
{
    public class DefaultModel : ExtendedViewModelBase
    {     
        string instapaperUser;
        public string InstapaperUser
        {
            get { return instapaperUser; }
            set { Assign("InstapaperUser", ref instapaperUser, value); }
        }

        string instapaperPassword;
        public string InstapaperPassword
        {
            get { return instapaperPassword; }
            set { Assign("InstapaperPassword", ref instapaperPassword, value); }
        }

        string pocketUser;
        public string PocketUser
        {
            get { return pocketUser; }
            set { Assign("PocketUser", ref pocketUser, value); }
        }

        string pocketPassword;
        public string PocketPassword
        {
            get { return pocketPassword; }
            set { Assign("PocketPassword", ref pocketPassword, value); }
        }

        #region Fields
        int selectedFontSize;
        public int SelectedFontSize
        {
            get { return selectedFontSize; }
            set { Assign("SelectedFontSize", ref selectedFontSize, value); }
        }

        bool retweetsAsMentions;
        public bool RetweetsAsMentions
        {
            get { return retweetsAsMentions; }
            set { Assign("RetweetsAsMentions", ref retweetsAsMentions, value); }
        }

        bool pushAvailable;
        public bool PushAvailable
        {
            get { return pushAvailable; }
            set { Assign("PushAvailable", ref pushAvailable, value); }
        }

        bool pushEnabled;
        public bool PushEnabled
        {
            get { return pushEnabled; }
            set { Assign("PushEnabled", ref pushEnabled, value); }
        }

        bool backgroundUpdateTiles;
        public bool BackgroundUpdateTiles
        {
            get { return backgroundUpdateTiles; }
            set { Assign("BackgroundUpdateTiles", ref backgroundUpdateTiles, value); }
        }

        string tweetsPerRequest;
        public string TweetsPerRequest
        {
            get { return tweetsPerRequest; }
            set { Assign("TweetsPerRequest", ref tweetsPerRequest, value); }
        }

        List<string> notifyOptions;
        public List<string> NotifyOptions
        {
            get { return notifyOptions; }
            set { Assign("NotifyOptions", ref notifyOptions, value); }
        }

        int mentionNotifyOption;
        public int MentionNotifyOption
        {
            get { return mentionNotifyOption; }
            set { Assign("MentionNotifyOption", ref mentionNotifyOption, value); }
        }

        int messageNotifyOption;
        public int MessageNotifyOption
        {
            get { return messageNotifyOption; }
            set { Assign("MessageNotifyOption", ref messageNotifyOption, value); }
        }

        int selectedAccount;
        public int SelectedAccount
        {
            get { return selectedAccount; }
            set { Assign("SelectedAccount", ref selectedAccount, value); }
        }

        int selectedMuteTime;
        public int SelectedMuteTime
        {
            get { return selectedMuteTime; }
            set { Assign("SelectedMuteTime", ref selectedMuteTime, value); }
        }

        SafeObservable<UserToken> accounts;
        public SafeObservable<UserToken> Accounts
        {
            get { return accounts; }
            set { Assign("Accounts", ref accounts, value); }
        }

        bool showResumePositionButton;
        public bool ShowResumePositionButton
        {
            get { return showResumePositionButton; }
            set { Assign("ShowResumePositionButton", ref showResumePositionButton, value); }
        }

        bool geoTaggingEnabled;
        public bool GeoTaggingEnabled
        {
            get { return geoTaggingEnabled; }
            set { Assign("GeoTaggingEnabled", ref geoTaggingEnabled, value); }
        }
        #endregion

        #region Commands
        DelegateCommand setCustomBackground;
        public ICommand SetCustomBackground
        {
            get { return setCustomBackground; }
        }

        DelegateCommand pinComposeToStart;
        public ICommand PinComposeToStart
        {
            get { return pinComposeToStart; }
        }

        DelegateCommand addAccount;
        public ICommand AddAccount
        {
            get { return addAccount; }
        }

        DelegateCommand editFilters;
        public ICommand EditFilters
        {
            get { return editFilters; }
        }

        DelegateCommand saveCredentials;
        public ICommand SaveCredentials
        {
            get { return saveCredentials; }
        }

        void SetCommands()
        {
            setCustomBackground = new DelegateCommand((obj) =>
            {
                Navigate("/Pages/Settings/Backgrounds.xaml");
            });

             
            pinComposeToStart = new DelegateCommand((obj) =>
                {
                    SecondaryTiles.CreateComposeTile();
                    pinComposeToStart.RaiseCanExecuteChanged();
                }, (obj) => !SecondaryTiles.ComposeTileIsCreated());

            addAccount = new DelegateCommand((obj) => {
                OAuth.Type = AuthType.Twitter;
                Navigate(Uris.LoginPage);
            });

            editFilters = new DelegateCommand((obj) =>
                {
                    DataTransfer.cFilter = Config.GlobalFilter;
                    DataTransfer.IsGlobalFilter = true;
                    Navigate(Uris.Filters);
                });

            saveCredentials = new DelegateCommand((obj) =>
                {
                    AuthPair PocketPair = null;
                    AuthPair InstapaperPair = null;

                    if (!string.IsNullOrWhiteSpace(PocketUser))
                    {
                        BarText = Resources.VerifyingCredentials;
                        IsLoading = true;
                        PocketPair = new AuthPair { User = PocketUser, Password = PocketPassword };
                        var service = new PocketService { UserName = PocketPair.User, Password = PocketPair.Password };
                        service.CheckCredentials((valid, response) =>
                        {
                            if (valid)
                            {
                                MessageService.ShowLightNotification(String.Format(Resources.CredentialsSaved, "Pocket"));
                                Config.ReadLaterCredentials.Pocket = PocketPair;
                                Config.ReadLaterCredentials = Config.ReadLaterCredentials;
                            }
                            else
                            {
                                IsLoading = false;
                                MessageService.ShowError(String.Format(Resources.InvalidCredentials, "Pocket"));
                            }
                        });
                    }
                    else
                    {
                        Config.ReadLaterCredentials.Pocket = null;
                        Config.ReadLaterCredentials = Config.ReadLaterCredentials;
                    }

                    if (!string.IsNullOrWhiteSpace(InstapaperUser))
                    {
                        BarText = Resources.VerifyingCredentials;
                        IsLoading = true;
                        InstapaperPair = new AuthPair { User = InstapaperUser, Password = InstapaperPassword };
                        var service = new InstapaperService { UserName = InstapaperPair.User, Password = InstapaperPair.Password };
                        service.CheckCredentials((valid, response) =>
                        {
                            if (valid)
                            {
                                MessageService.ShowLightNotification(String.Format(Resources.CredentialsSaved, "Instapaper"));
                                Config.ReadLaterCredentials.Instapaper = InstapaperPair;
                                Config.ReadLaterCredentials = Config.ReadLaterCredentials;
                            }
                            else
                            {
                                IsLoading = false;
                                MessageService.ShowError(String.Format(Resources.InvalidCredentials, "Instapaper"));
                            }
                        });
                    }
                    else
                    {
                        Config.ReadLaterCredentials.Pocket = null;
                        Config.ReadLaterCredentials = Config.ReadLaterCredentials;
                    }
                });
        }
        #endregion

        int FontSizeToIndex(int size)
        {
            if (size == 18)
                return 0;
            else if (size == 26)
                return 2;
            else
                return 1;
        }

        int IndexToFontSize(int index)
        {
            if (index == 0)
                return 18;
            else if (index == 2)
                return 26;
            else
                return 20;
        }

        public DefaultModel()
            : base("Default")
        {
            SelectedFontSize = FontSizeToIndex(((GlobalSettings)App.Current.Resources["GlobalSettings"]).
                            TweetFontSize);
            RetweetsAsMentions = Config.RetweetAsMentions == true;
            BackgroundUpdateTiles = Config.BackgroundLoadColumns == true;
            if (Config.TweetsPerRequest == null)
                Config.TweetsPerRequest = 40;
            TweetsPerRequest = Config.TweetsPerRequest.ToString();
            Accounts = new SafeObservable<UserToken>(Config.Accounts);
            NotifyOptions = new List<string> { Resources.None, Resources.OnlyTile, Resources.ToastAndTile };
            SelectedMuteTime = TimeSpanToSelectedFilter((TimeSpan)Config.DefaultMuteTime);
            ShowResumePositionButton = Config.RecoverReadPositions == true;
            GeoTaggingEnabled = Config.EnabledGeolocation == true;
            
#if OCELL_FULL
            PushAvailable = true;
#else
            PushAvailable = false;
#endif
            PushEnabled = PushAvailable && (Config.PushEnabled == true);

            if (Config.ReadLaterCredentials.Instapaper != null)
            {
                InstapaperUser = Config.ReadLaterCredentials.Instapaper.User;
                InstapaperPassword = Config.ReadLaterCredentials.Instapaper.Password;
            }

            if (Config.ReadLaterCredentials.Pocket != null)
            {
                PocketUser = Config.ReadLaterCredentials.Pocket.User;
                PocketPassword = Config.ReadLaterCredentials.Pocket.Password;
            }

            this.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "RetweetsAsMentions":
                        Config.RetweetAsMentions = RetweetsAsMentions;
                        break;
                    case "BackgroundUpdateTiles":
                        Config.BackgroundLoadColumns = BackgroundUpdateTiles;
                        break;
                    case "TweetsPerRequest":
                        int number;
                        if (int.TryParse(TweetsPerRequest, out number))
                            Config.TweetsPerRequest = number;
                        break;
                    case "SelectedAccount":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Count)
                        {
                            int newOption;

                            newOption = (int)Config.Accounts[SelectedAccount].Preferences.MentionsPreferences;
                            if (newOption != MentionNotifyOption)
                            {
                                mentionFirstChange = true;
                                MentionNotifyOption = newOption;
                            }

                            newOption = (int)Config.Accounts[SelectedAccount].Preferences.MessagesPreferences;
                            if (newOption != MessageNotifyOption)
                            {
                                messageFirstChange = true;
                                MessageNotifyOption = newOption;
                            }
                        }
                        break;
                    case "MentionNotifyOption":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Count)
                            SetMentionNotifyPref((NotificationType)MentionNotifyOption, SelectedAccount);
                        break;
                    case "MessageNotifyOption":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Count)
                            SetMessageNotifyPref((NotificationType)MessageNotifyOption, SelectedAccount);
                        Config.SaveAccounts();
                        break;
                    case "SelectedMuteTime":
                        Config.DefaultMuteTime = SelectedFilterToTimeSpan(SelectedMuteTime);
                        break;
                    case "SelectedFontSize":
                        ((GlobalSettings)App.Current.Resources["GlobalSettings"]).
                            TweetFontSize = IndexToFontSize(SelectedFontSize);
                        break;
                    case "ShowResumePositionButton":
                        Config.RecoverReadPositions = ShowResumePositionButton;
                        break;
                    case "GeoTaggingEnabled":
                        Config.EnabledGeolocation = GeoTaggingEnabled;
                        break;
                    case "PushEnabled":
                        Config.PushEnabled = PushEnabled;
                        if (PushEnabled == false)
                            PushNotifications.UnregisterAll();
                        else
                            PushNotifications.AutoRegisterForNotifications();
                        break;
                }
            };
            
            SelectedAccount = -1;
            if(Config.Accounts.Count > 0)
                SelectedAccount = 0;
            SetCommands();
        }

        bool mentionFirstChange = true;
        void SetMentionNotifyPref(NotificationType type, int account)
        {
            if (mentionFirstChange)
            {
                mentionFirstChange = false;
                return;
            }

            Config.Accounts[account].Preferences.MentionsPreferences = type;

            if (type == NotificationType.None)
                PushNotifications.UnregisterPushChannel(Config.Accounts[account], "mentions");
            else
                PushNotifications.RegisterPushChannel(Config.Accounts[account], "mentions");
        }

        bool messageFirstChange = true;
        void SetMessageNotifyPref(NotificationType type, int account)
        {
            if (messageFirstChange)
            {
                messageFirstChange = false;
                return;
            }

            Config.Accounts[account].Preferences.MessagesPreferences = type;

            if (type == NotificationType.None)
                PushNotifications.UnregisterPushChannel(Config.Accounts[account], "messages");
            else
                PushNotifications.RegisterPushChannel(Config.Accounts[account], "messages");
        }

        TimeSpan SelectedFilterToTimeSpan(int index)
        {
            switch (index)
            {
                case 0:
                    return TimeSpan.FromHours(1);
                case 1:
                    return TimeSpan.FromHours(8);
                case 2:
                    return TimeSpan.FromDays(1);
                case 3:
                    return TimeSpan.FromDays(7);
                case 4:
                    return TimeSpan.MaxValue;
                default:
                    return TimeSpan.FromHours(8);
            }
        }

        int TimeSpanToSelectedFilter(TimeSpan span)
        {
            if (Config.DefaultMuteTime == TimeSpan.FromHours(1))
                return 0;
            else if (Config.DefaultMuteTime == TimeSpan.FromHours(8))
                return 1;
            else if (Config.DefaultMuteTime == TimeSpan.FromDays(1))
                return 2;
            else if (Config.DefaultMuteTime == TimeSpan.FromDays(7))
                return 3;
            else
                return 4;
        }

        public void Navigated()
        {
            Accounts = new SafeObservable<UserToken>(Config.Accounts);
        }
    }
}
