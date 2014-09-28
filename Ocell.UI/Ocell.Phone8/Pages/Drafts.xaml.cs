using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.Library.Twitter;
using Ocell.ViewModels;
using System.Threading.Tasks;

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
                // LLS is crap and doesn't update the binding.
                // Also, delaying the selected item trigger allows the deletion of the item if the user pressed the button.
                Task.Delay(200).ContinueWith((t) =>
                {
                    Dispatcher.BeginInvoke(() => (DataContext as DraftsModel).SelectedDraft = DraftList.SelectedItem as TwitterDraft);
                });
            };
        }

    }
}