using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DanielVaughan.ComponentModel;
using DanielVaughan;
using DanielVaughan.Windows;
using Ocell.Library;
using System.Collections.Generic;
using Ocell.Library.Twitter;
using System.Collections.ObjectModel;

namespace Ocell
{
    public class ManageDraftsModel : ViewModelBase
    {
        ObservableCollection<TwitterDraft> collection;
        public ObservableCollection<TwitterDraft> Collection
        {
            get { return collection; }
            set { Assign("Collection", ref collection, value); }
        }

        object listSelection;
        public object ListSelection
        {
            get { return listSelection; }
            set { Assign("ListSelection", ref listSelection, value); }
        }

        public ManageDraftsModel()
            : base("ManageDrafts")
        {
            collection = new ObservableCollection<TwitterDraft>(Config.Drafts);

            this.NavigatingFrom += (sender, e) =>
            {
                Config.Drafts = new List<TwitterDraft>(collection);
            };

            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "ListSelection")
                        OnSelectionChanged();
                };
        }

        public void GridHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Grid grid = sender as Grid;
            if (grid == null)
                return;

            TwitterDraft draft = grid.Tag as TwitterDraft;
            if (draft != null && Config.Drafts.Contains(draft))
            {
                var accepts = MessageService.AskYesNoQuestion("Are you sure you want to delete this draft?", "");
                if (accepts)
                {
                    collection.Remove(draft);
                    MessageService.ShowMessage("Draft deleted.", "");
                }
            }
        }

        public void OnSelectionChanged()
        {
            TwitterDraft draft = ListSelection as TwitterDraft;

            if (draft == null)
                return;

            DataTransfer.Draft = draft;
            ListSelection = null;
            GoBack();
        }

    }
}
