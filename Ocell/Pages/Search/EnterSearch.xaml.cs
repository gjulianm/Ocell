using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Ocell.Pages.Search
{
    public partial class EnterSearch : PhoneApplicationPage
    {
        public EnterSearch()
        {
            InitializeComponent();
            ThemeFunctions.ChangeBackgroundIfLightTheme(LayoutRoot);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	NavigationService.Navigate(new Uri("/Pages/Search/Search.xaml?q=" + SearchQuery.Text, UriKind.Relative));
        }
    }
}
