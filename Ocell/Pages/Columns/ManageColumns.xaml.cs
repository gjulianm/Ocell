using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Ocell.Library;

namespace Ocell.Pages.Columns
{
    public partial class ManageColumns : PhoneApplicationPage
    {
        public ManageColumns()
        {
            InitializeComponent(); 
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(ManageColumns_Loaded);
        }

        void ManageColumns_Loaded(object sender, RoutedEventArgs e)
        {
            MainList.DataContext = Config.Columns;
            MainList.ItemsSource = Config.Columns;
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;

            foreach (var item in Config.Columns)
            {
                if (item.String == (string)img.Tag)
                {
                    Config.Columns.Remove(item);
                    break;
                }
            }

            MainList.DataContext = Config.Columns;
            MainList.ItemsSource = Config.Columns;

            Config.SaveColumns();
        }

        private void ApplicationBarIconButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(Uris.SelectUserForColumn);
        }

        private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(Uris.Settings);
        }

        private void MainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (sender as ListBox).SelectedIndex = -1;
        }
    }
}