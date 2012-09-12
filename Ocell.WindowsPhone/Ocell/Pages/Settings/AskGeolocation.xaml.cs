using Microsoft.Phone.Controls;
using Ocell.Library;

namespace Ocell
{
    public partial class AskGeolocation : PhoneApplicationPage
    {
        public AskGeolocation()
        {
            InitializeComponent();
        }

        private void AcceptBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Config.EnabledGeolocation = true;
            NavigationService.GoBack();
        }

        private void CancelBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Config.EnabledGeolocation = false;
            NavigationService.GoBack();
        }
    }
}
