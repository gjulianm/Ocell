using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using Ocell.Library.Twitter;
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell.Pages
{
    [ViewModel(typeof(TopicsModel))]
    public partial class Topics : PhoneApplicationPage
    {
        public Topics()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); }; 
            var viewModel = new TopicsModel();
            DataContext = viewModel;
            viewModel.ShowLocationsPicker += viewModel_ShowLocationsPicker;
        }

        void viewModel_ShowLocationsPicker(object sender, EventArgs e)
        {
            LocPicker.Open();
        }
    }
}
