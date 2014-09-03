using AncoraMVVM.Base;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Tasks;
using Ocell.Compatibility;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace Ocell
{
    public class MainPageModel : ExtendedViewModelBase
    {
        DateTime lastAutoReload;
        const int secondsBetweenReloads = 25;

        #region Events
        public void RaiseScrollToTop(TwitterResource resource)
        {
            var pivot = Pivots.FirstOrDefault(x => x.Resource == resource);

            if (pivot != null)
                pivot.ScrollToTop();
        }
        public Action<ColumnModel> ShowRecoverPositionPrompt { get; set; }
        #endregion

        public bool HasLoggedIn { get { return Config.Accounts.Value.Any(); } }
        public ObservableCollection<ColumnModel> Pivots { get; set; }
        public ColumnModel SelectedPivot { get; set; }
        public string CurrentAccountName { get; set; }
        public bool IsSearching { get; set; }
        public string UserSearch { get; set; }
        public int PreloadedLists { get; set; }
        public bool PreloadComplete { get { return PreloadedLists == Config.Columns.Value.Count; } }

        public void RaiseLoggedInChange()
        {
            RaisePropertyChanged("HasLoggedIn");
        }

        public void ReloadAll()
        {
            foreach (var pivot in Pivots)
                pivot.AutoLoad();
        }

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
                    var column = SelectedPivot.Resource;
                    if (Dependency.Resolve<TileManager>().ColumnTileIsCreated(column))
                        Notificator.ShowError(Localization.Resources.ColumnAlreadyPinned);
                    else
                        SecondaryTiles.CreateColumnTile(column);
                }, (obj) => SelectedPivot != null);


            pinToStart.BindCanExecuteToProperty(this, "SelectedPivot");

            filterColumn = new DelegateCommand(() => { });

            filterColumn.BindCanExecuteToProperty(this, "SelectedPivot");

            toMyProfile = new DelegateCommand((obj) =>
                {
                    Navigator.Navigate("/Pages/Elements/User.xaml?user=" + CurrentAccountName);
                }, (obj) => !string.IsNullOrWhiteSpace(CurrentAccountName));

            toMyProfile.BindCanExecuteToProperty(this, "CurrentAccountName");

            goToUser = new DelegateCommand((obj) =>
            {
                IsSearching = false;
                Navigator.Navigate("/Pages/Elements/User.xaml?user=" + UserSearch);
            }, obj => Config.Accounts.Value.Any());

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

        public override void OnNavigating(System.ComponentModel.CancelEventArgs e)
        {
            Config.SaveReadPositions();
            Progress.ClearIndicator();
            base.OnNavigating(e);
        }

        public MainPageModel()
        {
            if (Config.RetweetAsMentions.Value == null)
                Config.RetweetAsMentions.Value = true;
            if (Config.TweetsPerRequest.Value == null)
                Config.TweetsPerRequest.Value = 40;
            if (Config.DefaultMuteTime.Value == null || Config.DefaultMuteTime.Value == TimeSpan.FromHours(0))
                Config.DefaultMuteTime.Value = TimeSpan.FromHours(8);

            lastAutoReload = DateTime.MinValue;
            Pivots = new ObservableCollection<ColumnModel>();

            Pivots.AddListRange(Config.Columns.Value.Select(x => new ColumnModel(x)));

            foreach (var p in Pivots)
            {
                p.RequestRecoverPositionPopup += (sender, e) =>
                {
                    ShowRecoverPositionPrompt(sender as ColumnModel);
                };
            }

            Config.Columns.Value.CollectionChanged += (sender, e) =>
            {
                Dispatcher.InvokeIfRequired(() =>
                    {
                        if (e.NewItems != null)
                        {
                            foreach (var item in e.NewItems)
                            {
                                if ((item is TwitterResource) && !Pivots.Any(x => x.Resource == (TwitterResource)item))
                                    Pivots.Add(new ColumnModel((TwitterResource)item));
                            }
                        }

                        if (e.OldItems != null)
                        {
                            foreach (var item in e.OldItems)
                            {
                                if (item is TwitterResource)
                                {
                                    var pivot = Pivots.FirstOrDefault(x => x.Resource == (TwitterResource)item);
                                    if (pivot != null)
                                        Pivots.Remove(pivot);
                                }
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
            var resource = SelectedPivot.Resource;

            if (resource.User == null)
                return;

            CurrentAccountName = resource.User.ScreenName.ToUpperInvariant();
            ThreadPool.QueueUserWorkItem((context) => SelectedPivot.AutoLoad());
            DataTransfer.CurrentAccount = resource.User;

            if (SelectedPivot.CanRecoverPosition())
                ShowRecoverPositionPrompt(SelectedPivot);
        }

        bool firstNavigation = true;
        public void OnNavigation(string column)
        {
            ReloadAll();

            if (firstNavigation)
            {
                if (!string.IsNullOrWhiteSpace(column))
                {
                    column = Uri.UnescapeDataString(column);

                    SelectedPivot = Pivots.FirstOrDefault(item => item.Resource != null && item.Resource.String == column);
                    SelectedPivot = SelectedPivot ?? Pivots.FirstOrDefault();
                }
                else
                {
                    SelectedPivot = Pivots.FirstOrDefault();
                }
                firstNavigation = false;
            }
        }

    }
}
