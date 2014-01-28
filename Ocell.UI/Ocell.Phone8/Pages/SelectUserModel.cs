using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using TweetSharp;

namespace Ocell.Pages
{
    [ImplementPropertyChanged]
    public class SelectUserModel : ExtendedViewModelBase
    {
        private IUserProvider provider;

        public CollectionViewSource Collection { get; set; }

        public object Sender { get; set; }


        public object Destinatary { get; set; }

        public string UserFilter { get; set; }

        public IEnumerable<UserToken> Accounts { get; set; }

        DelegateCommand goNext;
        public ICommand GoNext
        {
            get { return goNext; }
        }

        public SelectUserModel()
            : base("SelectUserForDM")
        {
            provider = Dependency.Resolve<IUserProvider>();
            provider.GetFollowers = true;
            provider.GetFollowing = false;
            provider.Finished += (sender, e) => Progress.IsLoading = false;
            provider.Error += (sender, e) =>
            {
                Progress.IsLoading = false;
                MessageService.ShowError(Localization.Resources.ErrorDownloadingUsers);
            };

            Collection = new CollectionViewSource();
            Collection.Source = provider.Users;

            Collection.SortDescriptions.Add(new SortDescription("ScreenName", System.ComponentModel.ListSortDirection.Ascending));

            Accounts = Config.Accounts;

            goNext = new DelegateCommand((obj) =>
            {
                DataTransfer.ReplyingDM = true;
                Navigate(new Uri("/Pages/NewTweet.xaml?removeBack=1", UriKind.Relative));

            }, (obj) =>
                {
                    return Destinatary != null && Sender != null;
                });

            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "Destinatary")
                        DestinataryUpdated();
                    else if (e.PropertyName == "Sender")
                        SenderUpdated();
                    else if (e.PropertyName == "UserFilter")
                        UserFilterUpdated();
                };
        }

        public void Loaded()
        {
            Sender = Config.Accounts.FirstOrDefault();
        }

        private void DestinataryUpdated()
        {
            TwitterUser user = Destinatary as TwitterUser;
            if (user != null)
                DataTransfer.DMDestinationId = user.Id;
            goNext.RaiseCanExecuteChanged();
        }

        private void SenderUpdated()
        {
            provider.Users.Clear();
            provider.User = Sender as UserToken;
            ThreadPool.QueueUserWorkItem((context) =>
            {
                Progress.IsLoading = true;
                provider.Start();
            });
            goNext.RaiseCanExecuteChanged();
            DataTransfer.CurrentAccount = Sender as UserToken;
        }

        private void UserFilterUpdated()
        {
            Collection.View.Filter = new Predicate<object>(item => (item != null)
                    && (item is TwitterUser)
                    && (item as TwitterUser).ScreenName.ToLowerInvariant().Contains(UserFilter.ToLowerInvariant()));
        }
    }
}
