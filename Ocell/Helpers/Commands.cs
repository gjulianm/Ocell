using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using TweetSharp;

namespace Ocell.Commands
{
    public class ReplyCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable;
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

            PhoneApplicationFrame service = ((PhoneApplicationFrame)Application.Current.RootVisual);
            Deployment.Current.Dispatcher.BeginInvoke(() => service.Navigate(Uris.WriteTweet));
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ReplyAllCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is TwitterStatus;
        }

        public void Execute(object parameter)
        {
            ITweetable tweet = (ITweetable)parameter;
            DataTransfer.ReplyId = tweet.Id;
            DataTransfer.Text = "";
            foreach (string user in StringManipulator.GetUserNames(tweet.Text))
                DataTransfer.Text += "@" + user + " ";
            PhoneApplicationFrame service = ((PhoneApplicationFrame)Application.Current.RootVisual);
            Deployment.Current.Dispatcher.BeginInvoke(() => service.Navigate(Uris.WriteTweet));
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
            ServiceDispatcher.GetService(DataTransfer.CurrentAccount).Retweet(((ITweetable)parameter).Id, (sts, resp) => { });
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
            ServiceDispatcher.GetService(DataTransfer.CurrentAccount).FavoriteTweet(((ITweetable)parameter).Id, (sts, resp) => { });
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
                    Result = MessageBox.Show("Are you sure you want to delete the account @" + User.ScreenName, "", MessageBoxButton.OKCancel);
                    if (Result == MessageBoxResult.OK)
                    {
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
                ProtectedAccounts.SwitchAccountState(parameter as UserToken);
            }
            catch (Exception)
            {
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
}