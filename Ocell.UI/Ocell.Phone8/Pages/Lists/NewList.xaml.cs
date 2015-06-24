using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.ViewModels.Lists;

namespace Ocell.Pages.Lists
{
    [ViewModel(typeof(NewListModel))]
    public partial class NewList : PhoneApplicationPage
    {
        public NewList()
        {
            InitializeComponent();
        }
    }
}