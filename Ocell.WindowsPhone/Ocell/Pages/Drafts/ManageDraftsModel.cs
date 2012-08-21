using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;

namespace Ocell
{
    public class ManageDraftsModel : ExtendedViewModelBase
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
                var accepts = MessageService.AskYesNoQuestion(Resources.AskDeleteDraft, "");
                if (accepts)
                {
                    collection.Remove(draft);
                    MessageService.ShowMessage(Resources.DraftDeleted, "");
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
