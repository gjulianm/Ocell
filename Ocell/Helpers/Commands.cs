using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
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
            DataTransfer.ReplyId = tweet.Id;
            PhoneApplicationFrame service = ((PhoneApplicationFrame)Application.Current.RootVisual);
            Deployment.Current.Dispatcher.BeginInvoke(() => service.Navigate(new Uri("/Pages/NewTweet.xaml", UriKind.Relative)));
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ReplyAllCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ITweetable;
        }

        public void Execute(object parameter)
        {
            ITweetable tweet = (ITweetable)parameter;
            DataTransfer.ReplyId = tweet.Id;
            DataTransfer.Text = "";
            foreach (string user in StringManipulator.GetUserNames(tweet.Text))
                DataTransfer.Text += "@" + user + " ";
            PhoneApplicationFrame service = ((PhoneApplicationFrame)Application.Current.RootVisual);
            Deployment.Current.Dispatcher.BeginInvoke(() => service.Navigate(new Uri("/Pages/NewTweet.xaml", UriKind.Relative)));
        }

        public event EventHandler CanExecuteChanged;
    }

    public class RetweetCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is ITweetable) &&
                Clients.isServiceInit;
        }

        public void Execute(object parameter)
        {
            Clients.Service.Retweet(((ITweetable)parameter).Id, (sts, resp) => {});
        }

        public event EventHandler CanExecuteChanged;
    }

    public class FavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return (parameter is ITweetable) &&
                Clients.isServiceInit;
        }

        public void Execute(object parameter)
        {
            Clients.Service.FavoriteTweet(((ITweetable)parameter).Id, (sts, resp) => { });
        }

        public event EventHandler CanExecuteChanged;
    }
}