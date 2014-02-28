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
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell.Pages.Elements
{
    [ViewModel(typeof(NotificationsModel))]
    public partial class Notifications : PhoneApplicationPage
    {
        public Notifications()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };  
        }
    }
}
