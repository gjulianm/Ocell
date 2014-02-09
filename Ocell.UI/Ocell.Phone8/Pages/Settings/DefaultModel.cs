using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.ReadLater.Instapaper;
using Ocell.Library.ReadLater.Pocket;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Ocell.Settings
{
    [ImplementPropertyChanged]
    public class DefaultModel : ExtendedViewModelBase
    {
        public string InstapaperUser { get; set; }
        public string InstapaperPassword { get; set; }
        public string PocketUser { get; set; }
        public string PocketPassword { get; set; }

        #region Fields
        public int SelectedFontSize { get; set; }
        public bool RetweetsAsMentions { get; set; }
        public bool PushAvailable { get; set; }
        public bool PushEnabled { get; set; }
        public bool BackgroundUpdateTiles { get; set; }
        public string TweetsPerRequest { get; set; }
        public List<string> NotifyOptions { get; set; }
        public int MentionNotifyOption { get; set; }
        public int MessageNotifyOption { get; set; }
        public int SelectedAccount { get; set; }
        public int SelectedMuteTime { get; set; }
        public SafeObservable<UserToken> Accounts { get; set; }
        public bool ShowResumePositionButton { get; set; }
        public bool GeoTaggingEnabled { get; set; }
        public int SelectedReloadOption { get; set; }

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
                Navigator.Navigate("/Pages/Settings/Backgrounds.xaml");
            });

            showPrivacyPolicy = new DelegateCommand((obj) =>
            {
                Notificator.ShowMessage(Resources.PrivacyPolicy);
            });

            pinComposeToStart = new DelegateCommand((obj) =>
                {
                    SecondaryTiles.CreateComposeTile();
                    pinComposeToStart.RaiseCanExecuteChanged();
                }, (obj) => !SecondaryTiles.ComposeTileIsCreated());

            addAccount = new DelegateCommand((obj) =>
            {
                OAuth.Type = AuthType.Twitter;
                Navigator.Navigate(Uris.LoginPage);
            });

            editFilters = new DelegateCommand((obj) =>
                {
                    DataTransfer.cFilter = Config.GlobalFilter.Value;
                    DataTransfer.IsGlobalFilter = true;
                    Navigator.Navigate(Uris.Filters);
                });

            saveCredentials = new DelegateCommand(async (obj) =>
                {
                    AuthPair PocketPair = null;
                    AuthPair InstapaperPair = null;

                    if (!string.IsNullOrWhiteSpace(PocketUser))
                    {
                        Progress.Text = Resources.VerifyingCredentials;
                        Progress.IsLoading = true;
                        PocketPair = new AuthPair { User = PocketUser, Password = PocketPassword };
                        var service = new PocketService(PocketPair.User, PocketPair.Password);
                        var response = await service.CheckCredentials();

                        if (response.Succeeded)
                        {
                            Notificator.ShowProgressIndicatorMessage(String.Format(Resources.CredentialsSaved, "Pocket"));
                            Config.ReadLaterCredentials.Value.Pocket = PocketPair;
                            Config.ReadLaterCredentials.Value = Config.ReadLaterCredentials.Value;
                        }
                        else
                        {
                            Progress.IsLoading = false;
                            Notificator.ShowError(String.Format(Resources.InvalidCredentials, "Pocket"));
                        }
                    }
                    else
                    {
                        Config.ReadLaterCredentials.Value.Pocket = null;
                        Config.ReadLaterCredentials.Value = Config.ReadLaterCredentials.Value;
                    }

                    if (!string.IsNullOrWhiteSpace(InstapaperUser))
                    {
                        Progress.Text = Resources.VerifyingCredentials;
                        Progress.IsLoading = true;
                        InstapaperPair = new AuthPair { User = InstapaperUser, Password = InstapaperPassword };
                        var service = new InstapaperService(InstapaperPair.User, InstapaperPair.Password);
                        var response = await service.CheckCredentials();

                        if (response.Succeeded)
                        {
                            Notificator.ShowProgressIndicatorMessage(String.Format(Resources.CredentialsSaved, "Instapaper"));
                            Config.ReadLaterCredentials.Value.Instapaper = InstapaperPair;
                            Config.ReadLaterCredentials.Value = Config.ReadLaterCredentials.Value;
                        }
                        else
                        {
                            Progress.IsLoading = false;
                            Notificator.ShowError(String.Format(Resources.InvalidCredentials, "Instapaper"));
                        }
                    }
                    else
                    {
                        Config.ReadLaterCredentials.Value.Pocket = null;
                        Config.ReadLaterCredentials.Value = Config.ReadLaterCredentials.Value;
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
        {
            SelectedFontSize = FontSizeToIndex(((GlobalSettings)App.Current.Resources["GlobalSettings"]).
                            TweetFontSize);
            RetweetsAsMentions = Config.RetweetAsMentions.Value == true;
            BackgroundUpdateTiles = Config.BackgroundLoadColumns.Value == true;
            if (Config.TweetsPerRequest.Value == null)
                Config.TweetsPerRequest.Value = 40;
            TweetsPerRequest = Config.TweetsPerRequest.Value.ToString();
            Accounts = new SafeObservable<UserToken>(Config.Accounts.Value);
            NotifyOptions = new List<string> { Resources.None, Resources.OnlyTile, Resources.ToastAndTile };
            SelectedMuteTime = TimeSpanToSelectedFilter((TimeSpan)Config.DefaultMuteTime.Value);
            ShowResumePositionButton = Config.RecoverReadPositions.Value == true;
            GeoTaggingEnabled = Config.EnabledGeolocation.Value == true;
            SelectedReloadOption = (int)Config.ReloadOptions.Value;

            PushAvailable = TrialInformation.IsFullFeatured;

            PushEnabled = PushAvailable && (Config.PushEnabled.Value == true);

            if (Config.ReadLaterCredentials.Value.Instapaper != null)
            {
                InstapaperUser = Config.ReadLaterCredentials.Value.Instapaper.User;
                InstapaperPassword = Config.ReadLaterCredentials.Value.Instapaper.Password;
            }

            if (Config.ReadLaterCredentials.Value.Pocket != null)
            {
                PocketUser = Config.ReadLaterCredentials.Value.Pocket.User;
                PocketPassword = Config.ReadLaterCredentials.Value.Pocket.Password;
            }

            this.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "RetweetsAsMentions":
                        Config.RetweetAsMentions.Value = RetweetsAsMentions;
                        break;

                    case "BackgroundUpdateTiles":
                        Config.BackgroundLoadColumns.Value = BackgroundUpdateTiles;
                        break;

                    case "TweetsPerRequest":
                        int number;
                        if (int.TryParse(TweetsPerRequest, out number))
                            Config.TweetsPerRequest.Value = number;
                        break;

                    case "SelectedAccount":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Value.Count)
                        {
                            int newOption;

                            newOption = (int)Config.Accounts.Value[SelectedAccount].Preferences.MentionsPreferences;
                            if (newOption != MentionNotifyOption)
                            {
                                mentionFirstChange = true;
                                MentionNotifyOption = newOption;
                            }

                            newOption = (int)Config.Accounts.Value[SelectedAccount].Preferences.MessagesPreferences;
                            if (newOption != MessageNotifyOption)
                            {
                                messageFirstChange = true;
                                MessageNotifyOption = newOption;
                            }
                        }
                        break;

                    case "MentionNotifyOption":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Value.Count)
                            SetMentionNotifyPref((NotificationType)MentionNotifyOption, SelectedAccount);
                        break;

                    case "MessageNotifyOption":
                        if (SelectedAccount >= 0 && SelectedAccount < Config.Accounts.Value.Count)
                            SetMessageNotifyPref((NotificationType)MessageNotifyOption, SelectedAccount);
                        Config.SaveAccounts();
                        break;

                    case "SelectedMuteTime":
                        Config.DefaultMuteTime.Value = SelectedFilterToTimeSpan(SelectedMuteTime);
                        break;

                    case "SelectedFontSize":
                        ((GlobalSettings)App.Current.Resources["GlobalSettings"]).
                            TweetFontSize = IndexToFontSize(SelectedFontSize);
                        break;

                    case "ShowResumePositionButton":
                        Config.RecoverReadPositions.Value = ShowResumePositionButton;
                        break;

                    case "GeoTaggingEnabled":
                        Config.EnabledGeolocation.Value = GeoTaggingEnabled;
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
                        Config.ReloadOptions.Value = (ColumnReloadOptions)SelectedReloadOption;
                        break;
                }
            };

            SelectedAccount = -1;
            if (Config.Accounts.Value.Count > 0)
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

            Config.Accounts.Value[account].Preferences.MentionsPreferences = type;

            if (type == NotificationType.None)
                PushNotifications.UnregisterPushChannel(Config.Accounts.Value[account], "mentions");
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

            Config.Accounts.Value[account].Preferences.MessagesPreferences = type;

            if (type == NotificationType.None)
                PushNotifications.UnregisterPushChannel(Config.Accounts.Value[account], "messages");
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
            if (Config.DefaultMuteTime.Value == TimeSpan.FromHours(1))
                return 0;
            else if (Config.DefaultMuteTime.Value == TimeSpan.FromHours(8))
                return 1;
            else if (Config.DefaultMuteTime.Value == TimeSpan.FromDays(1))
                return 2;
            else if (Config.DefaultMuteTime.Value == TimeSpan.FromDays(7))
                return 3;
            else
                return 4;
        }

        public void Navigated()
        {
            Accounts = new SafeObservable<UserToken>(Config.Accounts.Value);
        }
    }
}