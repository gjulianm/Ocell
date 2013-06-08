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
    public partial class Notifications : PhoneApplicationPage
    {
        NotificationsModel viewModel;

        public Notifications()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };  
            ThemeFunctions.SetBackground(LayoutRoot);

            viewModel  = new NotificationsModel();
            this.DataContext = viewModel;
            this.Loaded += (s, e) => viewModel.OnLoad(); 
        }
    }
}
