using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.Pages.Elements;

namespace Ocell.Pages.Elements
{
    // Probably this doesn't work pretty good.
    [ViewModel(typeof(DMConversationModel))]
    public partial class DMConversation : PhoneApplicationPage
    {
        public DMConversation()
        {
            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
        }
    }
}
