using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Collections.ObjectModel;

namespace Ocell.Pages
{
    public partial class SelectUser : PhoneApplicationPage
    {
        private SelectUserModel viewModel;
        public SelectUser()
        {
            InitializeComponent(); Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };  
            ThemeFunctions.SetBackground(LayoutRoot);
            viewModel = new SelectUserModel();
            DataContext = viewModel;
            UserFilter.TextChanged += new TextChangedEventHandler(OnTextBoxTextChanged);
            this.Loaded += (sender, e) => viewModel.Loaded();
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            // Update the binding source
            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            bindingExpr.UpdateSource();
        }
    }
}