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
using Ocell.Library.Twitter;

namespace Ocell
{
    public partial class ManageDrafts : PhoneApplicationPage
    {
        private bool _selectionChangeFired;

        public ManageDrafts()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(ManageDrafts_Loaded);
            _selectionChangeFired = false;
        }

        void ManageDrafts_Loaded(object sender, RoutedEventArgs e)
        {
            draftsList.ItemsSource = Config.Drafts;
            draftsList.DataContext = Config.Drafts;

        }

        private void draftsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_selectionChangeFired)
            {
                if (e.AddedItems.Count <= 0)
                    return;
                TwitterDraft draft = e.AddedItems[0] as TwitterDraft;
                if (draft != null)
                {
                    DataTransfer.Draft = draft;
                    _selectionChangeFired = false;
                    draftsList.SelectedIndex = -1;
                    NavigationService.GoBack();
                }
            }
            else
                _selectionChangeFired = false;
        }

        private void Grid_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null)
                return;
            TwitterDraft draft = grid.Tag as TwitterDraft;
            if(draft != null && Config.Drafts.Contains(draft))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    var result = MessageBox.Show("Are you sure you want to delete this draft?", "", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        Config.Drafts.Remove(draft);
                        Config.SaveDrafts();
                        draftsList.ItemsSource = null;
                        draftsList.ItemsSource = Config.Drafts;
                        MessageBox.Show("Draft deleted.");
                    }
                });
            }
        }
    }
}
