using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Collections.ObjectModel;
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell.Pages
{
    [ViewModel(typeof(SelectUserModel))]        
    public partial class SelectUser : PhoneApplicationPage
    {
        public SelectUser()
        {
            InitializeComponent(); 
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };  
        }
    }
}