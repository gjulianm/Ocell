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


namespace Ocell.Pages.Columns
{
    public partial class AddColumn : PhoneApplicationPage
    {
        AddColumnModel viewModel;

        public AddColumn()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
            
            if(ApplicationBar != null) 
                ApplicationBar.MatchOverriddenTheme(); 
            

            viewModel = new AddColumnModel();
            DataContext = viewModel;
            this.Loaded += new RoutedEventHandler(AddColumn_Loaded);           
        }

        void AddColumn_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => { NavigationService.RemoveBackEntry(); });
            viewModel.OnLoad();
        }
    }
}