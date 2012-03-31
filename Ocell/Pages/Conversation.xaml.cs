using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TweetSharp;
using Ocell.Library;

namespace Ocell
{
    public partial class Conversation : PhoneApplicationPage
    {
        private TwitterStatus LastStatus;
        private ObservableCollection<TwitterStatus> Source;
        private bool selectionChangeFired = false;
        
        public Conversation()
        {
            InitializeComponent(); ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
            
            this.Loaded += new RoutedEventHandler(Conversation_Loaded);
            
            Source = new ObservableCollection<TwitterStatus>();
        }
        
        private void Conversation_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataTransfer.Status == null)
            {
                NavigationService.GoBack();
                return;
            }

            if (!Source.Contains(DataTransfer.Status))
                Source.Clear();

            LastStatus = DataTransfer.Status;
            if(!Source.Contains(DataTransfer.Status))
            	Source.Add(LastStatus);
            CList.ItemsSource = Source;

            MoreConversation();
        }
        
        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        
        private void MoreConversation()
        {
            TwitterStatus status = Source.Last();
            if(status != null && status.InReplyToStatusId == null)
            {
                EndLoad();
                return;
            }
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            ServiceDispatcher.GetCurrentService().GetTweet((long)status.InReplyToStatusId, ReceiveTweet);
        }
        
        private void ReceiveTweet(TwitterStatus status, TwitterResponse response)
        {
            if(status == null || response.StatusCode != HttpStatusCode.OK)
            {
               EndLoad();
               return;
            }

            Dispatcher.BeginInvoke(() =>
            {
                if(!Source.Contains(status))
                    Source.Add(status);
            });
            MoreConversation();
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!selectionChangeFired)
            {
                DataTransfer.Status = e.AddedItems[0] as TwitterStatus;
                ListBox list = sender as ListBox;
                selectionChangeFired = true;
                list.SelectedIndex = -1;

                NavigationService.Navigate(new Uri("/Pages/Tweet.xaml", UriKind.Relative));
            }
            else
                selectionChangeFired = false;
        }
        private void EndLoad()
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }
    }
}
