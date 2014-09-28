using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.ViewModels;

namespace Ocell.Pages.Filters
{
    [ViewModel(typeof(FiltersModel))]
    public partial class Filters : PhoneApplicationPage
    {
        public Filters()
        {
            InitializeComponent();

            FilterList.SelectionChanged += (s, e) =>
            {
                (DataContext as FiltersModel).SelectedFilter = FilterList.SelectedItem; // LLS is crap and doesn't update the binding.
            };
        }
    }
}