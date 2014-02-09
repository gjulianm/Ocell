using AncoraMVVM.Base;
using Microsoft.Phone.Tasks;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace Ocell
{
    public class BroadcastArgs : EventArgs
    {
        public TwitterResource Resource { get; set; }
        public bool BroadcastAll { get; set; }

        public BroadcastArgs(TwitterResource resource, bool broadcastToAll = false)
        {
            Resource = resource;
            BroadcastAll = broadcastToAll;
        }
    }

    public delegate void BroadcastEventHandler(object sender, BroadcastArgs e);

    public class MainPageModel : ExtendedViewModelBase
    {
        DateTime lastAutoReload;
        const int secondsBetweenReloads = 25;

        #region Events
        public event BroadcastEventHandler ScrollToTop;
        public void RaiseScrollToTop(TwitterResource resource)
        {
            var temp = ScrollToTop;
            if (temp != null)
                temp(this, new BroadcastArgs(resource));
        }

        public event BroadcastEventHandler ReloadLists;
        private void RaiseReload(TwitterResource resource)
        {
            var temp = ReloadLists;
            if (temp != null)
                temp(this, new BroadcastArgs(resource, false));
        }

        private void RaiseReloadAll()
        {
            var temp = ReloadLists;
            if (temp != null)
                temp(this, new BroadcastArgs(Config.Columns.FirstOrDefault(), true));
        }

        public event BroadcastEventHandler CheckIfCanResumePosition;
        private void RaiseCheckIfCanResumePosition(TwitterResource resource)
        {
            var temp = CheckIfCanResumePosition;
            if (temp != null)
                temp(this, new BroadcastArgs(resource, false));
        }
        #endregion

        public bool HasLoggedIn { get { return Config.Accounts.Any(); } }

        public void RaiseLoggedInChange()
        {
            RaisePropertyChanged("HasLoggedIn");
        }

        int loadingCount;
        public int LoadingCount
        {
            get { return loadingCount; }
            set
            {
                loadingCount = value;
                if (loadingCount <= 0)
                    Progress.IsLoading = false;
                else
                    Progress.IsLoading = true;
            }

        }

        Queue<NotifyCollectionChangedEventArgs> collectionChangedArgs;

        public ObservableCollection<TwitterResource> Pivots { get; set; }

        public object SelectedPivot { get; set; }

        public string CurrentAccountName { get; set; }

        public bool IsSearching { get; set; }

        public string UserSearch { get; set; }
        #region Commands
        DelegateCommand pinToStart;
        public ICommand PinToStart
        {
            get { return pinToStart; }
        }

        DelegateCommand filterColumn;
        public ICommand FilterColumn
        {
            get { return filterColumn; }
        }

        DelegateCommand toMyProfile;
        public ICommand ToMyProfile
        {
            get { return toMyProfile; }
        }

        DelegateCommand goToUser;
        public ICommand GoToUser
        {
            get { return goToUser; }
        }

        DelegateCommand feedback;
        public ICommand Feedback
        {
            get { return feedback; }
        }
        #endregion

        private void SetUpCommands()
        {
            pinToStart = new DelegateCommand((obj) =>
                {
                    var column = (TwitterResource)SelectedPivot;
                    /*if (Dependency.Resolve<TileManager>().ColumnTileIsCreated(column))
                        Notificator.ShowError("This column is already pinned.");
                    else
                        SecondaryTiles.CreateColumnTile(column);*/
                    // TODO: Column manager.
                }, (obj) => SelectedPivot != null);

            filterColumn = new DelegateCommand((obj) =>
            {
                var column = (TwitterResource)SelectedPivot;
                DataTransfer.cFilter = Config.Filters.FirstOrDefault(item => item.Resource == column);

                if (DataTransfer.cFilter == null)
                    DataTransfer.cFilter = new ColumnFilter { Resource = column };

                DataTransfer.IsGlobalFilter = false;
                Navigator.Navigate(Uris.Filters);

            }, (obj) => SelectedPivot != null);

            toMyProfile = new DelegateCommand((obj) =>
                {
                    Navigator.Navigate("/Pages/Elements/User.xaml?user=" + CurrentAccountName);
                }, (obj) => !string.IsNullOrWhiteSpace(CurrentAccountName));

            goToUser = new DelegateCommand((obj) =>
            {
                IsSearching = false;
                Navigator.Navigate("/Pages/Elements/User.xaml?user=" + UserSearch);
            }, obj => Config.Accounts.Any());

            feedback = new DelegateCommand((obj) =>
                {
                    var task = new EmailComposeTask();
                    task.Subject = "Ocell - Feedback";
                    task.To = "gjulian93@gmail.com";

                    Dispatcher.InvokeIfRequired(task.Show);
                });

        }

        public override void OnLoad()
        {
            RaisePropertyChanged("Pivots");
        }

        public MainPageModel()
        {
            if (Config.RetweetAsMentions == null)
                Config.RetweetAsMentions = true;
            if (Config.TweetsPerRequest == null)
                Config.TweetsPerRequest = 40;
            if (Config.DefaultMuteTime == null || Config.DefaultMuteTime == TimeSpan.FromHours(0))
                Config.DefaultMuteTime = TimeSpan.FromHours(8);

            lastAutoReload = DateTime.MinValue;
            Pivots = new ObservableCollection<TwitterResource>();
            collectionChangedArgs = new Queue<NotifyCollectionChangedEventArgs>();

            foreach (var pivot in Config.Columns)
                Pivots.Add(pivot);

            Config.Columns.CollectionChanged += (sender, e) =>
            {
                Dispatcher.InvokeIfRequired(() =>
                    {
                        if (e.NewItems != null)
                        {
                            foreach (var item in e.NewItems)
                            {
                                if ((item is TwitterResource) && !Pivots.Contains((TwitterResource)item))
                                    Pivots.Add((TwitterResource)item);
                            }
                        }

                        if (e.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                            {
                                if ((item is TwitterResource) && Pivots.Contains((TwitterResource)item))
                                    Pivots.Remove((TwitterResource)item);
                            }
                        }
                    });
            };

            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "SelectedPivot")
                        UpdatePivot();
                };



            SetUpCommands();
        }

        void UpdatePivot()
        {
            if (SelectedPivot is TwitterResource)
            {
                var resource = (TwitterResource)SelectedPivot;

                if (resource.User == null)
                    return;

                CurrentAccountName = resource.User.ScreenName.ToUpperInvariant();
                ThreadPool.QueueUserWorkItem((context) => RaiseReload(resource));
                DataTransfer.CurrentAccount = resource.User;
                RaiseCheckIfCanResumePosition(resource);
            }
        }

        bool firstNavigation = true;
        public void RaiseNavigatedTo(object sender, System.Windows.Navigation.NavigationEventArgs e, string column)
        {
            ThreadPool.QueueUserWorkItem((context) => RaiseReloadAll());

            if (firstNavigation)
            {
                if (!string.IsNullOrWhiteSpace(column))
                {
                    column = Uri.UnescapeDataString(column);

                    if (Config.Columns.Any(item => item.String == column))
                        SelectedPivot = Config.Columns.First(item => item.String == column);
                }
                else
                {
                    if (Config.Columns.Any())
                        SelectedPivot = Config.Columns.First();
                }
                firstNavigation = false;
            }
        }
    }
}
