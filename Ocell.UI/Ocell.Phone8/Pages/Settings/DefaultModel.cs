using DanielVaughan.Windows;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.ReadLater.Instapaper;
using Ocell.Library.ReadLater.Pocket;
using Ocell.Library.Twitter;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Ocell.Settings
{
    public class DefaultModel : ExtendedViewModelBase
    {
        private string instapaperUser;
        public string InstapaperUser
        {
            get { return instapaperUser; }
            set { Assign("InstapaperUser", ref instapaperUser, value); }
        }

        private string instapaperPassword;
        public string InstapaperPassword
        {
            get { return instapaperPassword; }
            set { Assign("InstapaperPassword", ref instapaperPassword, value); }
        }

        private string pocketUser;
        public string PocketUser
        {
            get { return pocketUser; }
            set { Assign("PocketUser", ref pocketUser, value); }
        }

        private string pocketPassword;
        public string PocketPassword
        {
            get { return pocketPassword; }
            set { Assign("PocketPassword", ref pocketPassword, value); }
        }

        #region Fields
        private int selectedFontSize;
        public int SelectedFontSize
        {
            get { return selectedFontSize; }
            set { Assign("SelectedFontSize", ref selectedFontSize, value); }
        }

        private bool retweetsAsMentions;
        public bool RetweetsAsMentions
        {
            get { return retweetsAsMentions; }
            set { Assign("RetweetsAsMentions", ref retweetsAsMentions, value); }
        }

        private bool pushAvailable;
        public bool PushAvailable
        {
            get { return pushAvailable; }
            set { Assign("PushAvailable", ref pushAvailable, value); }
        }

        private bool pushEnabled;
        public bool PushEnabled
        {
            get { return pushEnabled; }
            set { Assign("PushEnabled", ref pushEnabled, value); }
        }

        private bool backgroundUpdateTiles;
        public bool BackgroundUpdateTiles
        {
            get { return backgroundUpdateTiles; }
            set { Assign("BackgroundUpdateTiles", ref backgroundUpdateTiles, value); }
        }

        private string tweetsPerRequest;
        public string TweetsPerRequest
        {
            get { return tweetsPerRequest; }
            set { Assign("TweetsPerRequest", ref tweetsPerRequest, value); }
        }

        private List<string> notifyOptions;
        public List<string> NotifyOptions
        {
            get { return notifyOptions; }
            set { Assign("NotifyOptions", ref notifyOptions, value); }
        }

        private int mentionNotifyOption;
        public int MentionNotifyOption
        {
            get { return mentionNotifyOption; }
            set { Assign("MentionNotifyOption", ref mentionNotifyOption, value); }
        }

        private int messageNotifyOption;
        public int MessageNotifyOption
        {
            get { return messageNotifyOption; }
            set { Assign("MessageNotifyOption", ref messageNotifyOption, value); }
        }

        private int selectedAccount;
        public int SelectedAccount
        {
            get { return selectedAccount; }
            set { Assign("SelectedAccount", ref selectedAccount, value); }
        }

        private int selectedMuteTime;
        public int SelectedMuteTime
        {
            get { return selectedMuteTime; }
            set { Assign("SelectedMuteTime", ref selectedMuteTime, value); }
        }

        private SafeObservable<UserToken> accounts;
        public SafeObservable<UserToken> Accounts
        {
            get { return accounts; }
            set { Assign("Accounts", ref accounts, value); }
        }

        private bool showResumePositionButton;
        public bool ShowResumePositionButton
        {
            get { return showResumePositionButton; }
            set { Assign("ShowResumePositionButton", ref showResumePositionButton, value); }
        }

        private bool geoTaggingEnabled;
        public bool GeoTaggingEnabled
        {
            get { return geoTaggingEnabled; }
            set { Assign("GeoTaggingEnabled", ref geoTaggingEnabled, value); }
        }

        private int selectedReloadOption;
        public int SelectedReloadOption
        {
            get { return selectedReloadOption; }
            set { Assign("SelectedReloadOption", ref selectedReloadOption, value); }
        }

        #endregion Fields

        #region Commands
        private DelegateCommand setCustomBackground;
        public ICommand SetCustomBackground
        {
            get { return setCustomBackground; }
        }

        private DelegateCommand pinComposeToStart;
        public ICommand PinComposeToStart
        {
            get { return pinComposeToStart; }
        }

        private DelegateCommand addAccount;
        public ICommand AddAccount
        {
            get { return addAccount; }
        }

        private DelegateCommand editFilters;
        public ICommand EditFilters
        {
            get { return editFilters; }
        }

        private DelegateCommand saveCredentials;
        public ICommand SaveCredentials
        {
            get { return saveCredentials; }
        }

        private DelegateCommand showPrivacyPolicy;
        public ICommand ShowPrivacyPolicy
        {
            get { return showPrivacyPolicy; }
        }

        private void SetCommands()
        {
            setCustomBackground = new DelegateCommand((obj) =>
            {
                Navigate("/Pages/Settings/Backgrounds.xaml");
            });

            showPrivacyPolicy = new DelegateCommand((obj) =>
            {
                MessageService.ShowMessage(Resources.PrivacyPolicy);
            });

            pinComposeToStart = new DelegateCommand((obj) =>
                {
                    SecondaryTiles.CreateComposeTile();
                    pinComposeToStart.RaiseCanExecuteChanged();
                }, (obj) => !SecondaryTiles.ComposeTileIsCreated());

            addAccount = new DelegateCommand((obj) =>
            {
                OAuth.Type = AuthType.Twitter;
                Navigate(Uris.LoginPage);
            });

            editFilters = new DelegateCommand((obj) =>
                {
                    DataTransfer.cFilter = Config.GlobalFilter;
                    DataTransfer.IsGlobalFilter = true;
                    Navigate(Uris.Filters);
                });

            saveCredentials = new DelegateCommand(async (obj) =>
                {
                    AuthPair PocketPair = null;
                    AuthPair InstapaperPair = null;

                    if (!string.IsNullOrWhiteSpace(PocketUser))
                    {
                        BarText = Resources.VerifyingCredentials;
                        IsLoading = true;
                        PocketPair = new AuthPair { User = PocketUser, Password = PocketPassword };
                        var service = new PocketService(PocketPair.User, PocketPair.Password);
                        var response = await service.CheckCredentials();

                        if (response.Succeeded)
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
                        var service = new InstapaperService(InstapaperPair.User, InstapaperPair.Password);
                        var response = await service.CheckCredentials();

                        if (response.Succeeded)
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
                    }
                    else
                    {
                        Config.ReadLaterCredentials.Pocket = null;
                        Config.ReadLaterCredentials = Config.ReadLaterCredentials;
                    }
                });
        }

        #endregion Commands

        private int FontSizeToIndex(int size)
        {
            if (size == 18)
                return 0;
            else if (size == 26)
                return 2;
            else
                return 1;
        }

        private int IndexToFontSize(int index)
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
            SelectedReloadOption = (int)Config.ReloadOptions;

            PushAvailable = TrialInformation.IsFullFeatured;

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
                        if (!TrialInformation.IsFullFeatured)
                        {
                            if (PushEnabled)
                            {
                                TrialInformation.ShowBuyDialog();
                                PushEnabled = false;
                            }
                            return;
                        }

                        Config.PushEnabled = PushEnabled;
                        if (PushEnabled == false)
                            PushNotifications.UnregisterAll();
                        else
                            PushNotifications.AutoRegisterForNotifications();
                        break;

                    case "SelectedReloadOption":
                        Config.ReloadOptions = (ColumnReloadOptions)SelectedReloadOption;
                        break;
                }
            };

            SelectedAccount = -1;
            if (Config.Accounts.Count > 0)
                SelectedAccount = 0;
            SetCommands();
        }

        private bool mentionFirstChange = true;
        private void SetMentionNotifyPref(NotificationType type, int account)
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
                PushNotifications.AutoRegisterForNotifications();
        }

        private bool messageFirstChange = true;
        private void SetMessageNotifyPref(NotificationType type, int account)
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
                PushNotifications.AutoRegisterForNotifications();
        }

        private TimeSpan SelectedFilterToTimeSpan(int index)
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

        private int TimeSpanToSelectedFilter(TimeSpan span)
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