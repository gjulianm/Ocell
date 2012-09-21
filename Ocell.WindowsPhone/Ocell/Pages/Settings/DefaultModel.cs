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
using DanielVaughan.ComponentModel;
using DanielVaughan;
using DanielVaughan.Windows;
using Ocell.Library.Twitter;
using Ocell.Library;
using System.Linq;
using Ocell.Library.Notifications;
using System.Collections.Generic;
using Ocell.Library.ReadLater.Pocket;
using Ocell.Library.ReadLater.Instapaper;
using Ocell.Localization;

namespace Ocell.Settings
{
    public class DefaultModel : ExtendedViewModelBase
    {
        bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Assign("IsLoading", ref isLoading, value); }
        }

        string barText;
        public string BarText
        {
            get { return barText; }
            set { Assign("BarText", ref barText, value); }
        }

        

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

            addAccount = new DelegateCommand((obj) => Navigate(Uris.LoginPage));

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
            PushEnabled = Config.PushEnabled == true;

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
                            MentionNotifyOption = (int)Config.Accounts[SelectedAccount].Preferences.MentionsPreferences;
                            MessageNotifyOption = (int)Config.Accounts[SelectedAccount].Preferences.MessagesPreferences;
                        }
                        break;
                    case "MentionNotifyOption":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Count)
                            Config.Accounts[SelectedAccount].Preferences.MentionsPreferences =
                                (NotificationType)MentionNotifyOption;
                        break;
                    case "MessageNotifyOption":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Count)
                            Config.Accounts[SelectedAccount].Preferences.MessagesPreferences =
                                (NotificationType)MessageNotifyOption;
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
                            PushNotifications.UnregisterPushChannel();
                        else
                            PushNotifications.RegisterPushChannel();
                        break;
                }
            };
            
            SelectedAccount = -1;
            if(Config.Accounts.Count > 0)
                SelectedAccount = 0;
            SetCommands();
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
