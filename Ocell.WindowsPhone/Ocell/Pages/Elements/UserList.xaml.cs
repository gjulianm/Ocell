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
using Ocell.Pages.Elements;

namespace Ocell
{
    public partial class UserList : PhoneApplicationPage
    {
        UserListModel viewModel;

        public UserList()
        {
            InitializeComponent();
            viewModel = new UserListModel();
            DataContext = viewModel;

            ThemeFunctions.SetBackground(LayoutRoot);
            this.Loaded += (sender, e) =>
            {
                string resource, user;
                if (!NavigationContext.QueryString.TryGetValue("resource", out resource) || !NavigationContext.QueryString.TryGetValue("user", out user))
                {
                    Dispatcher.BeginInvoke(() => {
                        MessageBox.Show("An error has occurred.", "Error", MessageBoxButton.OK);
                        NavigationService.GoBack();
                    });
                    
                    return;
                }

                viewModel.Loaded(resource, user);
            };
        }
    }
}
