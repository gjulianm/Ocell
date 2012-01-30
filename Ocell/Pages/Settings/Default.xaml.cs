using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace Ocell.Settings
{
    public partial class Default : PhoneApplicationPage
    {
        public Default()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Default_Loaded);
        }

        void Default_Loaded(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings config;

            try
            {
                config = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.ToString());
                MessageBox.Show("Error loading configuration.");
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }

            if (string.IsNullOrWhiteSpace(Clients.ScreenName))
            {
                Clients.fillScreenName();
                Clients.UserFilled += new Clients.OnUserFilled(FillUserName);
            }
            else
                FillUserName();
        }

        public void FillUserName()
        {
            UserName.Text = Clients.ScreenName;
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings/OAuth.xaml", UriKind.Relative));
        }
    }
}