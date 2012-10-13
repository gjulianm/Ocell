using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Web;
using System.Windows;
using Hammock;
using Hammock.Authentication.OAuth;
using Microsoft.Phone.Controls;
using System.Linq;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Library.Notifications;
using System.Text;
using Ocell.Localization;
using Ocell.Pages.Settings;
using DanielVaughan;

namespace Ocell.Settings
{
    public enum AuthType { Twitter, Buffer}


    public partial class OAuth : PhoneApplicationPage
    {
        OAuthModel viewModel;
        public static AuthType Type { get; set;}

        public OAuth()
        {
            InitializeComponent(); 

            switch (Type)
            {
                case AuthType.Twitter:
                    viewModel = new TwitterOAuthModel();
                    break;
                default:
                    throw new NotImplementedException(string.Format("Type {0} unknown", Type));
            }

            wb.Navigated += (sender, e) => viewModel.BrowserNavigated(e);
            wb.Navigating += (sender, e) => viewModel.BrowserNavigating(e);
            this.Loaded += (sender, e) => viewModel.PageLoaded();
            viewModel.Navigate += (sender, e) => Dispatcher.InvokeIfRequired(() => wb.Navigate(e));
        }               
    }
}