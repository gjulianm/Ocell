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
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell
{
    [ViewModel(typeof(ManageDraftsModel))]
    public partial class ManageDrafts : PhoneApplicationPage
    {
        private bool _selectionChangeFired;


        public ManageDrafts()
        {
            InitializeComponent();
            
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
             
            _selectionChangeFired = false;
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
            // TODO: WTF.
            (DataContext as ManageDraftsModel).GridHold(sender, e);
        }
    }
}
