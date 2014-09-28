using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.Library.Twitter;
using Ocell.ViewModels;

namespace Ocell.Pages
{
    [ViewModel(typeof(DraftsModel))]
    public partial class Drafts : PhoneApplicationPage
    {
        public Drafts()
        {
            InitializeComponent();

            DraftList.SelectionChanged += (s, e) =>
            {
                (DataContext as DraftsModel).SelectedDraft = DraftList.SelectedItem as TwitterDraft; // LLS is crap and doesn't update the binding.
            };
        }
    }
}