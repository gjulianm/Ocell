﻿using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Filtering;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ocell.Pages.Filtering
{
    public partial class Filters : PhoneApplicationPage
    {
        public Filters()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };

            this.Loaded += new RoutedEventHandler(Filters_Loaded);
        }

        void Filters_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataTransfer.cFilter == null)
                DataTransfer.cFilter = new ColumnFilter();
            FilterList.DataContext = null;
            FilterList.ItemsSource = null;
            FilterList.DataContext = DataTransfer.cFilter.Predicates;
            FilterList.ItemsSource = DataTransfer.cFilter.Predicates;
            FilterList.UpdateLayout();
        }

        private void add_Click(object sender, System.EventArgs e)
        {
            DataTransfer.Filter = new ITweetableFilter();
            DataTransfer.Filter.Type = FilterType.User;
            DataTransfer.Filter.Filter = "";
            DataTransfer.Filter.Inclusion = IncludeOrExclude.Include;
            if (Config.DefaultMuteTime.Value == TimeSpan.MaxValue)
                DataTransfer.Filter.IsValidUntil = DateTime.Now.AddYears(1);
            else
                DataTransfer.Filter.IsValidUntil = DateTime.Now + (TimeSpan)Config.DefaultMuteTime.Value;

            NavigationService.Navigate(Uris.SingleFilter);
        }

        private void ApplicationBarIconButton_Click(object sender, System.EventArgs e)
        {
            // Save.
            if (!DataTransfer.IsGlobalFilter)
            {
                ColumnFilter ToRemove = Config.Filters.Value.FirstOrDefault(item => item.Resource == DataTransfer.cFilter.Resource);
                if (ToRemove != null)
                    Config.Filters.Value.Remove(ToRemove);

                Config.Filters.Value.Add(DataTransfer.cFilter);
                Config.SaveFilters();
                DataTransfer.ShouldReloadFilters = true;
            }
            else
            {
                Config.GlobalFilter.Value = DataTransfer.cFilter;
            }
            NavigationService.GoBack();
        }

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid != null && grid.Tag is ITweetableFilter)
            {
                DataTransfer.Filter = grid.Tag as ITweetableFilter;
                NavigationService.Navigate(Uris.SingleFilter);
            }
        }

        private void Grid_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid grid = sender as Grid;
            Dispatcher.BeginInvoke(() =>
            {
                MessageBoxResult result = MessageBox.Show(Localization.Resources.AskFilterDelete, "", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    if (grid != null && grid.Tag is ITweetableFilter)
                    {
                        DataTransfer.cFilter.RemoveFilter(grid.Tag as ITweetableFilter);
                        FilterList.DataContext = null;
                        FilterList.ItemsSource = null;
                        FilterList.DataContext = DataTransfer.cFilter.Predicates;
                        FilterList.ItemsSource = DataTransfer.cFilter.Predicates;
                    }
                }
            });
        }

        private void FilterList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (FilterList.SelectedIndex != -1)
                FilterList.SelectedIndex = -1;
        }
    }
}
