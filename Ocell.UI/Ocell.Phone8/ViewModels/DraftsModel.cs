
using AncoraMVVM.Base;
using Ocell.Library.Twitter;
using Ocell.Pages;
namespace Ocell.ViewModels
{
    public class DraftsModel : ViewModelBase
    {
        public SafeObservable<TwitterDraft> Drafts { get; set; }
        public TwitterDraft SelectedDraft { get; set; }
        public DelegateCommand RemoveDraft { get; set; }

        public DraftsModel()
        {
            RemoveDraft = new DelegateCommand((param) =>
            {
                var draft = param as TwitterDraft;
                if (draft != null)
                    Drafts.Remove(draft);
            });

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectedDraft" && SelectedDraft != null)
                    Navigator.MessageAndNavigate<NewTweetModel, TwitterDraft>(SelectedDraft);
            };
        }
    }
}
