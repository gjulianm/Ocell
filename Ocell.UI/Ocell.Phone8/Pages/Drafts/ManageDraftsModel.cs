using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Ocell
{
    [ImplementPropertyChanged]
    public class ManageDraftsModel : ExtendedViewModelBase
    {
        public ObservableCollection<TwitterDraft> Collection { get; set; }

        public object ListSelection { get; set; }

        public ManageDraftsModel()
            : base("ManageDrafts")
        {
            Collection = new ObservableCollection<TwitterDraft>(Config.Drafts);

            this.NavigatingFrom += (sender, e) =>
            {
                Config.Drafts = new List<TwitterDraft>(Collection);
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
                    Collection.Remove(draft);
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
