

using PropertyChanged;

namespace Ocell.Pages.Elements
{
    [ImplementPropertyChanged]
    public class DMConversationModel : ExtendedViewModelBase
    {
        public string PairName { get; set; }

        public DMConversationModel()
        {
        }
    }
}
