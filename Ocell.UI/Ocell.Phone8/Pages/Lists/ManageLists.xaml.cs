using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.ViewModels;
using Ocell.ViewModels.Lists;

namespace Ocell.Pages.Lists
{
    [ViewModel(typeof(ManageListsModel))]
    public partial class ManageLists : PhoneApplicationPage
    {
        public ManageLists()
        {
            InitializeComponent();
        }
    }
}