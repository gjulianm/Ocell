using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell.Pages.Columns
{
    public partial class ManageColumns : PhoneApplicationPage
    {
        public ManageColumns()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); }; 
            
            

            this.Loaded += new RoutedEventHandler(ManageColumns_Loaded);
        }

        void ManageColumns_Loaded(object sender, RoutedEventArgs e)
        {
            MainList.DataContext = Config.Columns.Value;
            MainList.ItemsSource = Config.Columns.Value;
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image img = sender as Image;

            foreach (var item in Config.Columns.Value)
            {
                if (item.String == (string)img.Tag)
                {
                    Config.Columns.Value.Remove(item);
                    DataTransfer.ShouldReloadColumns = true;
                    break;
                }
            }

            MainList.DataContext = Config.Columns.Value;
            MainList.ItemsSource = Config.Columns.Value;

            Config.SaveColumns();
        }

        private void MainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (sender as ListBox).SelectedIndex = -1;
        }

        private void menuItemClick(object sender, EventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null && item.Tag is TwitterResource)
            {
                TwitterResource resource = (TwitterResource)item.Tag;
                try
                {
                    Config.Columns.Value.Remove(resource);
                }
                catch (Exception)
                {
                    return;
                }
                Config.SaveColumns();
                DataTransfer.ShouldReloadColumns = true;
            }
        }
    }
}