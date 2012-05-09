using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Collections.Generic;

namespace Ocell.Pages.Elements
{
    public partial class Conversation : PhoneApplicationPage
    {
        private TwitterStatus LastStatus;
        private ObservableCollection<TwitterStatus> Source;
        private bool selectionChangeFired = false;
        private ConversationService replies;
        private int pendingCalls;
        public Conversation()
        {
            InitializeComponent(); ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
            
            this.Loaded += new RoutedEventHandler(Conversation_Loaded);
            replies = new ConversationService(DataTransfer.CurrentAccount);
            Source = new ObservableCollection<TwitterStatus>();
            pendingCalls = 0;
        }
        
        private void Conversation_Loaded(object sender, RoutedEventArgs e)
        {
            if(DataTransfer.Status == null)
            {
                NavigationService.GoBack();
                return;
            }

            /*if (!Source.Contains(DataTransfer.Status))
                Source.Clear();

            LastStatus = DataTransfer.Status;
            if(!Source.Contains(DataTransfer.Status))
            	Source.Add(LastStatus);
            CList.ItemsSource = Source;

            MoreConversation();*/

            TwitterResource resource = new TwitterResource
            {
                Data = DataTransfer.Status.Id.ToString(),
                Type = ResourceType.Conversation,
                User = DataTransfer.CurrentAccount
            };

            if (CList.Loader == null)
            {
                CList.Loader = new TweetLoader(resource);
            }

            if (CList.Loader.Resource != resource)
            {
                CList.Loader.Source.Clear();
                CList.Loader.Resource = resource;
            }
            
            Dispatcher.BeginInvoke(() => pBar.IsVisible = true);
            CList.Loader.LoadFinished += new EventHandler(LoadFinished);
            CList.Loader.Cached = false;
            CList.Loader.Load();
            
        }

        private void LoadFinished(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }
        
        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!selectionChangeFired)
            {
                DataTransfer.Status = e.AddedItems[0] as TwitterStatus;
                ListBox list = sender as ListBox;
                selectionChangeFired = true;
                list.SelectedIndex = -1;

                NavigationService.Navigate(Uris.ViewTweet);
            }
            else
                selectionChangeFired = false;
        }
        private void EndLoad()
        {
            pendingCalls--;
            if(pendingCalls <= 0)
                Dispatcher.BeginInvoke(() => pBar.IsVisible = false);
        }
    }
}
