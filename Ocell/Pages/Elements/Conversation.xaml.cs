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

            DataContext = CList.Loader;

            CList.Loader.Cached = false;
            CList.Loader.Load();            
        }
    }
}
