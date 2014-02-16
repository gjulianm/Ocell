using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using AncoraMVVM.Rest;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.ReadLater.Instapaper;
using Ocell.Library.ReadLater.Pocket;
using Ocell.Library.Twitter;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TweetSharp;

namespace Ocell.Commands
{
    public class ReplyCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable && Config.Accounts.Value.Any();
        }

        public void Execute(object parameter)
        {
            if (parameter == null)
                return;

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
        public static string GetReplied(ITweetable tweet)
        {
            string textReplied = "";
            textReplied = "@" + tweet.Author.ScreenName + " ";
            foreach (string user in StringManipulator.GetUserNames(tweet.Text))
                if (DataTransfer.CurrentAccount != null && user != "@" + DataTransfer.CurrentAccount.ScreenName)
                    textReplied += user + " ";

            return textReplied;
        }

        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable && Config.Accounts.Value.Any() && DataTransfer.CurrentAccount != null;
        }

        public void Execute(object parameter)
        {
            ITweetable tweet = (ITweetable)parameter;
            DataTransfer.ReplyId = tweet.Id;
            DataTransfer.Text = GetReplied(tweet);

            Dependency.Resolve<INavigationService>().Navigate(Uris.WriteTweet);
        }

        public event EventHandler CanExecuteChanged;
    }

    // TODO: Man, check the responses. That was lazy.

    public class RetweetCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is TwitterStatus) &&
                Config.Accounts.Value.Count > 0 &&
                DataTransfer.CurrentAccount != null;
        }

        public async void Execute(object parameter)
        {
            Dependency.Resolve<IProgressIndicator>().IsLoading = true;
            Dependency.Resolve<IProgressIndicator>().IsLoading = true;
            await ServiceDispatcher.GetService(DataTransfer.CurrentAccount).RetweetAsync(new RetweetOptions { Id = ((ITweetable)parameter).Id });
            Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.Retweeted);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class FavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is TwitterStatus) &&
                Config.Accounts.Value.Count > 0 &&
                DataTransfer.CurrentAccount != null;
        }

        public async void Execute(object parameter)
        {
            TwitterStatus param = (TwitterStatus)parameter;
            if (param.IsFavorited)
            {
                await ServiceDispatcher.GetService(DataTransfer.CurrentAccount).UnfavoriteTweetAsync(new UnfavoriteTweetOptions { Id = param.Id });
                Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.Unfavorited);
            }
            else
            {
                await ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FavoriteTweetAsync(new FavoriteTweetOptions { Id = param.Id });
                Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.Favorited);
            }
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
                        var copy = new List<TwitterResource>(Config.Columns.Value.Where(item => item.User == User));
                        foreach (var item in copy)
                            Config.Columns.Value.Remove(item);

                        Config.SaveColumns();
                        Config.Accounts.Value.Remove(User);
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
                Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(msg);
            }
            catch (Exception)
            {
                Dependency.Resolve<INotificationService>().ShowError(Resources.ErrorMessage);
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
            Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.Filtered);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ReadLaterCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            var creds = Config.ReadLaterCredentials.Value;
            return parameter is TwitterStatus && (creds != null && creds.Instapaper != null || creds.Pocket != null);
        }

        public async void Execute(object parameter)
        {
            TwitterStatus tweet = parameter as TwitterStatus;
            var credentials = Config.ReadLaterCredentials.Value;

            if (tweet == null)
                return;

            TwitterUrl link = tweet.Entities.FirstOrDefault(item => item != null && item.EntityType == TwitterEntityType.Url) as TwitterUrl;
            HttpResponse response = null;
            string tweetUrl = "http://twitter.com/" + tweet.Author.ScreenName + "/statuses/" + tweet.Id.ToString();

            if (credentials.Pocket != null)
            {
                var service = new PocketService(credentials.Pocket.User, credentials.Pocket.Password);

                Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.SavingForLater);
                if (link != null)
                    response = await service.AddUrl(link.ExpandedValue, tweet.Id);
                else
                    response = await service.AddUrl(tweetUrl);

                CheckResponse(response);
            }

            if (credentials.Instapaper != null)
            {
                var service = new InstapaperService(credentials.Instapaper.User, credentials.Instapaper.Password);

                Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.SavingForLater);
                if (link != null)
                    response = await service.AddUrl(link.ExpandedValue, tweet.Text);
                else
                    response = await service.AddUrl(tweetUrl);

                CheckResponse(response);
            }
        }

        private void CheckResponse(HttpResponse response)
        {
            Dependency.Resolve<IProgressIndicator>().IsLoading = false;

            if (!response.Succeeded)
                Dependency.Resolve<INotificationService>().ShowError(Resources.ErrorSavingLater);
            else
                Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.SavedForLater);
        }

        public event EventHandler CanExecuteChanged;
    }
}