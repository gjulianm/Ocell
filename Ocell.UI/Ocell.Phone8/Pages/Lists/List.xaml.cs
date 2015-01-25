using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.ViewModels;

namespace Ocell.Pages.Lists
{
    [ViewModel(typeof(ListModel))]
    public partial class List : PhoneApplicationPage
    {
        public List()
        {
            InitializeComponent();
        }
    }
}