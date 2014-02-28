using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AncoraMVVM.Base.ViewModelLocator;

namespace Ocell.Pages.Settings
{
    [ViewModel(typeof(CouponCodesModel))]
    public partial class CouponCodes : PhoneApplicationPage
    {
        public CouponCodes()
        {
            InitializeComponent();
        }
    }
}