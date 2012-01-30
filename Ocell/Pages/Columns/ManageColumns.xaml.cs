using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace Ocell.AuxScreens
{
    public partial class ManageColumns : PhoneApplicationPage
    {
        public ManageColumns()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ManageColumns_Loaded);
        }

        void ManageColumns_Loaded(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;

            ObservableCollection<TwitterResource> list;

            if (!config.TryGetValue<ObservableCollection<TwitterResource>>("COLUMNS", out list))
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show("Error loading columns."); });
                return;
            }

            MainList.DataContext = list;
            MainList.ItemsSource = list;
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            IsolatedStorageSettings config = IsolatedStorageSettings.ApplicationSettings;
            Image img = sender as Image;

            ObservableCollection<TwitterResource> list;

            if (!config.TryGetValue<ObservableCollection<TwitterResource>>("COLUMNS", out list))
                return;
            
            list.Remove(list.Single(item => (item.String == (String)img.Tag)));

            MainList.DataContext = list;
            MainList.ItemsSource = list;

            config["COLUMNS"] = list;
            config.Save();
        }

        private void ApplicationBarIconButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Columns/AddColumn.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Settings/Default.xaml", UriKind.Relative));
        }

        private void MainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (sender as ListBox).SelectedIndex = -1;
        }
    }
}