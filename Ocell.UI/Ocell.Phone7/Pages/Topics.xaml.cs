using System;
using System.Net;
using System.Windows;
using Microsoft.Phone.Controls;
using Ocell.Library.Twitter;

namespace Ocell.Pages
{
    public partial class Topics : PhoneApplicationPage
    {
        public Topics()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); }; 
            var viewModel = new TopicsModel();
            DataContext = viewModel;
            viewModel.ShowLocationsPicker += viewModel_ShowLocationsPicker;
            ThemeFunctions.SetBackground(LayoutRoot);
        }

        void viewModel_ShowLocationsPicker(object sender, EventArgs e)
        {
            LocPicker.Open();
        }
    }
}
