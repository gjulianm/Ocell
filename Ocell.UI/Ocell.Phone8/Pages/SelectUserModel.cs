using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using DanielVaughan.ComponentModel;
using DanielVaughan.Windows;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Threading;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using System.Linq;

namespace Ocell.Pages
{
    public class SelectUserModel : ExtendedViewModelBase
    {
        private IUserProvider provider;

        CollectionViewSource collection;
        public CollectionViewSource Collection
        {
            get { return collection; }
            set { Assign("Collection", ref collection, value); }
        }

        object sender;
        public object Sender
        {
            get { return sender; }
            set { Assign("Sender", ref sender, value); }
        }


        object destinatary;
        public object Destinatary
        {
            get { return destinatary; }
            set { Assign("Destinatary", ref destinatary, value); }
        }

        string userFilter;
        public string UserFilter
        {
            get { return userFilter; }
            set { Assign("UserFilter", ref userFilter, value); }
        }

        IEnumerable<UserToken> accounts;
        public IEnumerable<UserToken> Accounts
        {
            get { return accounts; }
            set { Assign("Accounts", ref accounts, value); }
        }

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
            provider.Finished += (sender, e) => IsLoading = false;
            provider.Error += (sender, e) =>
            {
                IsLoading = false;
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
                IsLoading = true;
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
