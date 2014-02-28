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
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell.Pages.Settings
{
    [ViewModel(typeof(BackgroundsModel))]
    public partial class Backgrounds : PhoneApplicationPage
    {
        public Backgrounds()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); }; 
        }
    }
}