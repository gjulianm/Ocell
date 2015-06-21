using AncoraMVVM.Base;
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
using Ocell.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ocell.Library.RuntimeData;
using TweetSharp;

// I know there are unused events, I can't do anything. Stfu already.
#pragma warning disable 0067


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
            var args = new NewTweetArgs();

            if (parameter is TwitterStatus)
            {
                args.ReplyToId = tweet.Id;
                args.Type = TweetType.Tweet;
                args.Text = String.Format("@{0} ", tweet.Author.ScreenName);
            }
            else if (parameter is TwitterDirectMessage)
            {
                args.ReplyToId = (parameter as TwitterDirectMessage).SenderId;
                args.Type = TweetType.DirectMessage;
                args.Text = "";
            }

            Dependency.Resolve<INavigationService>().MessageAndNavigate<NewTweetModel, NewTweetArgs>(args);
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
                if (ApplicationData.CurrentAccount != null && user != "@" + ApplicationData.CurrentAccount.ScreenName)
                    textReplied += user + " ";

            return textReplied;
        }

        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable && Config.Accounts.Value.Any() && ApplicationData.CurrentAccount != null;
        }

        public void Execute(object parameter)
        {
            ITweetable tweet = (ITweetable)parameter;
            var args = new NewTweetArgs
            {
                ReplyToId = tweet.Id,
                Text = GetReplied(tweet)
            };

            Dependency.Resolve<INavigationService>().MessageAndNavigate<NewTweetModel, NewTweetArgs>(args);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class RetweetCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is TwitterStatus) &&
                Config.Accounts.Value.Count > 0 &&
                ApplicationData.CurrentAccount != null;
        }

        public async void Execute(object parameter)
        {
            Dependency.Resolve<IProgressIndicator>().IsLoading = true;
            var response = await ServiceDispatcher.GetService(ApplicationData.CurrentAccount).RetweetAsync(new RetweetOptions { Id = ((ITweetable)parameter).Id });
            var notificator = Dependency.Resolve<INotificationService>();

            if (response.RequestSucceeded)
                notificator.ShowProgressIndicatorMessage(Resources.Retweeted);
            else
                notificator.ShowError(Resources.ErrorRetweeting);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class FavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is TwitterStatus) &&
                Config.Accounts.Value.Count > 0 &&
                ApplicationData.CurrentAccount != null;
        }

        public void Execute(object parameter)
        {
            var task = ExecuteAsync(parameter);
        }

        public async Task<bool> ExecuteAsync(object parameter)
        {
            TwitterStatus param = (TwitterStatus)parameter;
            TwitterResponse<TwitterStatus> response;
            var notificator = Dependency.Resolve<INotificationService>();
            var progress = Dependency.Resolve<IProgressIndicator>();
            string successString = "";

            progress.IsLoading = true;

            if (param.IsFavorited)
            {
                response = await ServiceDispatcher.GetService(ApplicationData.CurrentAccount).UnfavoriteTweetAsync(new UnfavoriteTweetOptions { Id = param.Id });
                successString = Resources.Unfavorited;
            }
            else
            {
                response = await ServiceDispatcher.GetService(ApplicationData.CurrentAccount).FavoriteTweetAsync(new FavoriteTweetOptions { Id = param.Id });
                successString = Resources.Favorited;
            }

            progress.IsLoading = false;

            if (response.RequestSucceeded)
                notificator.ShowProgressIndicatorMessage(successString);
            else
                notificator.ShowError(Resources.ErrorMessage + response.Error.Message);

            return response.RequestSucceeded;
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

            FilterManager.CreateAndAddGlobalFilter(tweet.AuthorName, UserFilter.Creator);
            Dependency.Resolve<INotificationService>().ShowProgressIndicatorMessage(Resources.Filtered);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ReadLaterCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (System.ComponentModel.DesignerProperties.IsInDesignTool)
                return true; // Avoid XAML design view errors when not able to resolve dependencies.

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

#pragma warning restore 0067