using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.ViewModels;

namespace Ocell.Pages.Filters
{
    [ViewModel(typeof(SingleFilterModel))]
    public partial class SingleFilter : PhoneApplicationPage
    {
        public SingleFilter()
        {
            InitializeComponent();
        }
    }
}