using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using TweetSharp;
using System.Linq;
using Ocell.Library;
using Ocell.Library.Twitter;
using AncoraMVVM.Base.ViewModelLocator;


namespace Ocell.Pages.Columns
{
    // TODO: This is being used?
    [ViewModel(typeof(AddColumnModel))]
    public partial class AddColumn : PhoneApplicationPage
    {
        public AddColumn()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

            this.Loaded += new RoutedEventHandler(AddColumn_Loaded);           
        }

        void AddColumn_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => { NavigationService.RemoveBackEntry(); });
        }
    }
}