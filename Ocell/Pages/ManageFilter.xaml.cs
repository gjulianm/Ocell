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
    public partial class ManageFilter : PhoneApplicationPage
    {
        private ITweetableFilter Filter;

        public ManageFilter()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
            this.Loaded +=new RoutedEventHandler(ManageFilter_Loaded);
        }

        void MessageAndExit(string msg)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(msg);
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            });
        }

        void ManageFilter_Loaded(object sender, RoutedEventArgs e)
        {
            Filter = DataTransfer.Filter;

            if (Filter == null)
                MessageAndExit("We couldn't load this filter, sorry.");

            if (Filter.Type == FilterType.User)
                Resource.SelectedIndex = 0;
            else if (Filter.Type == FilterType.Source)
                Resource.SelectedIndex = 1;
            else if (Filter.Type == FilterType.Text)
                Resource.SelectedIndex = 2;

            FilterText.Text = Filter.Filter;

            if (Filter.Inclusion == IncludeOrExclude.Include)
                Inclusion.SelectedIndex = 1;
            else
                Inclusion.SelectedIndex = 0;
        }

        private void ApplicationBarIconButton_Click(object sender, System.EventArgs e)
        {
            ITweetableFilter filter = new ITweetableFilter();

            if (Resource.SelectedIndex == 0)
                filter.Type = FilterType.User;
            else if (Resource.SelectedIndex == 1)
                filter.Type = FilterType.Source;
            else if (Resource.SelectedIndex == 2)
                filter.Type = FilterType.Text;
            else
            {
                MessageAndExit("Error saving this filter");
                return;
            }

            filter.Filter = FilterText.Text;

            if (Inclusion.SelectedIndex == 1)
                filter.Inclusion = IncludeOrExclude.Include;
            else
                filter.Inclusion = IncludeOrExclude.Exclude;

            DataTransfer.cFilter.RemoveFilter(Filter);
            DataTransfer.cFilter.AddFilter(filter);
            DataTransfer.Filter = filter;

            NavigationService.GoBack();
        }
    }
}
