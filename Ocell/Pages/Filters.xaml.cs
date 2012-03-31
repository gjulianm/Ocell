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
using Ocell.Library;

namespace Ocell
{
    public partial class Filters : PhoneApplicationPage
    {
        public Filters()
        {
            InitializeComponent();
			ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);

            this.Loaded += new RoutedEventHandler(Filters_Loaded);
        }

        void Filters_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataTransfer.cFilter == null)
                DataTransfer.cFilter = new ColumnFilter();
            FilterList.DataContext = DataTransfer.cFilter.Predicates;
            FilterList.ItemsSource = DataTransfer.cFilter.Predicates;
        }

        private void add_Click(object sender, System.EventArgs e)
        {
            DataTransfer.Filter = new UserFilter();
            DataTransfer.Filter.Filter = "";
            DataTransfer.Filter.Inclusion = IncludeOrExclude.Include;
            NavigationService.Navigate(new Uri("/Pages/ManageFilter.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click(object sender, System.EventArgs e)
        {
            ColumnFilter ToRemove = Config.Filters.FirstOrDefault(item => item.Resource == DataTransfer.cFilter.Resource);
            if (ToRemove != null)
                Config.Filters.Remove(ToRemove);

            Config.Filters.Add(DataTransfer.cFilter);
            Config.SaveFilters();
            NavigationService.GoBack();
        }
    }
}
