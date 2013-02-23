using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Threading;
using Ocell.Library.ReadLater.Instapaper;
using Ocell.Library.ReadLater.Pocket;
using Ocell.Library.ReadLater;
using System.Linq;
using DanielVaughan;
using DanielVaughan.Services;
using System.Collections.Generic;
using Ocell.Localization;

namespace Ocell.Commands
{
    public class ReplyCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable && Config.Accounts.Any();
        }

        public void Execute(object parameter)
        {
            ITweetable tweet = (ITweetable)parameter;
            DataTransfer.Text = "@" + tweet.Author.ScreenName + " ";
            if (parameter is TwitterStatus)
            {
                DataTransfer.ReplyId = tweet.Id;
                DataTransfer.ReplyingDM = false;
            }
            else if (parameter is TwitterDirectMessage)
            {
                DataTransfer.DMDestinationId = (parameter as TwitterDirectMessage).SenderId;
                DataTransfer.ReplyingDM = true;
            }

            Dependency.Resolve<INavigationService>().Navigate(Uris.WriteTweet);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ReplyAllCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable && Config.Accounts.Any() && DataTransfer.CurrentAccount != null;
        }

        public void Execute(object parameter)
        {
            ITweetable tweet = (ITweetable)parameter;
            DataTransfer.ReplyId = tweet.Id;
            DataTransfer.Text = "@" + tweet.Author.ScreenName + " ";
            foreach (string user in StringManipulator.GetUserNames(tweet.Text))
                if(DataTransfer.CurrentAccount != null && user != "@" + DataTransfer.CurrentAccount.ScreenName)
                    DataTransfer.Text += user + " ";

            Dependency.Resolve<INavigationService>().Navigate(Uris.WriteTweet);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class RetweetCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is TwitterStatus) &&
                Config.Accounts.Count > 0 &&
                DataTransfer.CurrentAccount != null;
        }

        public void Execute(object parameter)
        {
            Dependency.Resolve<IMessageService>().SetLoadingBar(true);
            ServiceDispatcher.GetService(DataTransfer.CurrentAccount).Retweet(new RetweetOptions { Id = ((ITweetable)parameter).Id }, (sts, resp) =>
            {
                Dependency.Resolve<IMessageService>().ShowLightNotification(Resources.Retweeted);
            });
        }

        public event EventHandler CanExecuteChanged;
    }

    public class FavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is TwitterStatus) &&
                Config.Accounts.Count > 0 &&
                DataTransfer.CurrentAccount != null;
        }

        public void Execute(object parameter)
        {
            TwitterStatus param = (TwitterStatus)parameter;
            if (param.IsFavorited)
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnfavoriteTweet(new UnfavoriteTweetOptions { Id = param.Id }, (sts, resp) =>
                {
                    Dependency.Resolve<IMessageService>().ShowLightNotification(Resources.Unfavorited);
                });
            else
                ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FavoriteTweet(new FavoriteTweetOptions { Id = param.Id }, (sts, resp) =>
                {
                    Dependency.Resolve<IMessageService>().ShowLightNotification(Resources.Favorited);
                });
        }

        public event EventHandler CanExecuteChanged;
    }

    public class DeleteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is UserToken);
        }

        public void Execute(object parameter)
        {
            UserToken User = parameter as UserToken;
            if (User != null)
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBoxResult Result;
                    Result = MessageBox.Show(String.Format(Resources.AskAccountDelete, User.ScreenName), "", MessageBoxButton.OKCancel);
                    if (Result == MessageBoxResult.OK)
                    {
                        // Make a copy: removing while iterating causes an error.
                        var copy = new List<TwitterResource>(Config.Columns.Where(item => item.User == User));
                        foreach (var item in copy)
                            Config.Columns.Remove(item);

                        Config.SaveColumns();
                        Config.Accounts.Remove(User);
                        Config.SaveAccounts();
                        PhoneApplicationFrame service = ((PhoneApplicationFrame)Application.Current.RootVisual);
                        if (service.CanGoBack)
                            service.GoBack();
                    }
                });
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ProtectCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is UserToken);
        }

        public void Execute(object parameter)
        {
            try
            {
                var isProtected = ProtectedAccounts.SwitchAccountState(parameter as UserToken);
                string msg;
                if (isProtected)
                    msg = Resources.AccountProtected;
                else
                    msg = Resources.AccountUnprotected;
                Dependency.Resolve<IMessageService>().ShowLightNotification(msg);
            }
            catch (Exception)
            {
                Dependency.Resolve<IMessageService>().ShowError(Resources.ErrorMessage);
            }
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ModifyFilterCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetableFilter;
        }

        public void Execute(object parameter)
        {
            DataTransfer.Filter = parameter as ITweetableFilter;
            PhoneApplicationFrame service = ((PhoneApplicationFrame)Application.Current.RootVisual);
            service.Navigate(Uris.SingleFilter);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class RemoveFilterCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetableFilter;
        }

        public void Execute(object parameter)
        {
            DataTransfer.cFilter.RemoveFilter(parameter as ITweetableFilter);
            PhoneApplicationFrame service = ((PhoneApplicationFrame)Application.Current.RootVisual);
            service.Navigate(Uris.Filters);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class MuteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable;
        }

        public void Execute(object parameter)
        {
            ITweetable tweet = parameter as ITweetable;

            if (tweet == null)
                return;

            ITweeter author = tweet.Author;

            FilterManager.SetupMute(FilterType.User, author.ScreenName);
            Dependency.Resolve<IMessageService>().ShowLightNotification(Resources.Filtered);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ReadLaterCommand : ICommand
    {
        private int _pendingCalls;

        public bool CanExecute(object parameter)
        {
            var creds = Config.ReadLaterCredentials;
            return parameter is TwitterStatus && (creds.Instapaper != null || creds.Pocket != null);
        }

        public void Execute(object parameter)
        {
            TwitterStatus tweet = parameter as TwitterStatus;
            _pendingCalls = 0;
            var credentials = Config.ReadLaterCredentials;

            if (tweet == null)
                return;

            if (credentials.Pocket != null)
            {
                var service = new PocketService();
                service.UserName = credentials.Pocket.User;
                service.Password = credentials.Pocket.Password;

                TwitterUrl link = tweet.Entities.FirstOrDefault(item => item != null && item.EntityType == TwitterEntityType.Url) as TwitterUrl;
                Dependency.Resolve<IMessageService>().SetLoadingBar(true, Resources.SavingForLater);
                _pendingCalls++;
                if (link != null)
                    service.AddUrl(link.ExpandedValue, tweet.Id, Callback);
                else
                {
                    string url = "http://twitter.com/" + tweet.Author.ScreenName + "/statuses/" + tweet.Id.ToString();
                    service.AddUrl(url, Callback);
                }
            }
            if (credentials.Instapaper != null)
            {
                var service = new InstapaperService();
                service.UserName = credentials.Instapaper.User;
                service.Password = credentials.Instapaper.Password;

                TwitterUrl link = tweet.Entities.FirstOrDefault(item => item != null && item.EntityType == TwitterEntityType.Url) as TwitterUrl;
                Dependency.Resolve<IMessageService>().SetLoadingBar(true, Resources.SavingForLater);
                _pendingCalls++;
                if (link != null)
                    service.AddUrl(link.ExpandedValue, tweet.Text, Callback);
                else
                {
                    string url = "http://twitter.com/" + tweet.Author.ScreenName + "/statuses/" + tweet.Id.ToString();
                    service.AddUrl(url, Callback);
                }
            }
        }

        private void Callback(ReadLaterResponse response)
        {
            Dependency.Resolve<IMessageService>().SetLoadingBar(false);
            _pendingCalls--;
            if (response.Result != ReadLaterResult.Accepted)
            {
                Dependency.Resolve<IMessageService>().ShowError(Resources.ErrorSavingLater);
            }
            else if (_pendingCalls <= 0)
            {
                Dependency.Resolve<IMessageService>().ShowLightNotification(Resources.SavedForLater);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}