using System.Windows;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Filtering;
using System;

namespace Ocell.Pages.Filtering
{
    public partial class ManageFilter : PhoneApplicationPage
    {
        private ITweetableFilter Filter;
        bool _initialized;

        public ManageFilter()
        {
            _initialized = false;
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
            
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
            if (_initialized)
                return;
            Filter = DataTransfer.Filter;

            if (Filter == null)
                MessageAndExit(Localization.Resources.CouldntLoadFilter);

            if (Filter.Type == FilterType.User)
                Resource.SelectedIndex = 0;
            else if (Filter.Type == FilterType.Source)
                Resource.SelectedIndex = 1;
            else if (Filter.Type == FilterType.Text)
                Resource.SelectedIndex = 2;

            FilterText.Text = Filter.Filter;

            date.Value = Filter.IsValidUntil.Date;
            time.Value = Filter.IsValidUntil;

            if (Filter.Inclusion == IncludeOrExclude.Include)
                Inclusion.SelectedIndex = 1;
            else
                Inclusion.SelectedIndex = 0;

            _initialized = true;
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
                MessageAndExit(Localization.Resources.ErrorSavingFilter);
                return;
            }

            filter.Filter = FilterText.Text;

            filter.IsValidUntil = new DateTime(
                date.Value.Value.Year,
                date.Value.Value.Month,
                date.Value.Value.Day,
                time.Value.Value.Hour,
                time.Value.Value.Minute,
                0);

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
