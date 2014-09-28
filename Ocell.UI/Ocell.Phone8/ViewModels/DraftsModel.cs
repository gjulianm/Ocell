
using AncoraMVVM.Base;
using Ocell.Library;
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
            Drafts = new SafeObservable<TwitterDraft>();

            RemoveDraft = new DelegateCommand((param) =>
            {
                var draft = param as TwitterDraft;
                if (draft != null)
                    Config.Drafts.Value.Remove(draft);
            });

            var replayer = new ObservableCollectionReplayer();

            replayer.ReplayTo(Config.Drafts.Value, Drafts);

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectedDraft" && SelectedDraft != null)
                    Navigator.MessageAndNavigate<NewTweetModel, TwitterDraft>(SelectedDraft);
            };
        }
    }
}
