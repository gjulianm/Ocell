using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.Library.Filtering;
using Ocell.ViewModels;
using System.Threading.Tasks;
using TweetSharp;

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
                // LLS is crap and doesn't update the binding.
                // Also, delaying the selected item trigger allows the deletion of the item if the user pressed the button.
                Task.Delay(200).ContinueWith((t) =>
                {
                    Dispatcher.BeginInvoke(() => (DataContext as FiltersModel).SelectedFilter = FilterList.SelectedItem as ElementFilter<ITweetable>);
                });
            };
        }
    }
}