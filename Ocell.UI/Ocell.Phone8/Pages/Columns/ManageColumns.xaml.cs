using AncoraMVVM.Base.ViewModelLocator;
using Microsoft.Phone.Controls;
using Ocell.ViewModels;
using System.Windows.Controls;

namespace Ocell.Pages.Columns
{
    [ViewModel(typeof(ManageColumnsModel))]
    public partial class ManageColumns : PhoneApplicationPage
    {
        public ManageColumns()
        {
            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };
        }

        private void ColumnItemTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            /* This should be done with a Command binding in the button itself to the LayoutRoot
             *  DataContext, but for some reason it isn't working. */
            var viewModel = DataContext as ManageColumnsModel;
            var button = sender as Button;

            if (viewModel != null && button != null)
                viewModel.DeleteColumn(button.DataContext);
        }
    }
}