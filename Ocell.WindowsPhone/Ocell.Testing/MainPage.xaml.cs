using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.Phone.Shell;

namespace Ocell.Testing
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        IMobileTestPage mobileTestPage;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemTray.IsVisible = false;

            UIElement testPage = UnitTestSystem.CreateTestPage();

            Application.Current.RootVisual = testPage;
            Application.Current.Host.Settings.EnableFrameRateCounter = false;
            mobileTestPage = testPage as IMobileTestPage;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (mobileTestPage != null)
            {
                e.Cancel = mobileTestPage.NavigateBack();
            }
            
            base.OnBackKeyPress(e);
        }
    }
}