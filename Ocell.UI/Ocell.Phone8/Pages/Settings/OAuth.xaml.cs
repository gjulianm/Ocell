
using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Controls;
using Ocell.Pages.Settings;
using System;

namespace Ocell.Settings
{
    public enum AuthType { Twitter, Buffer }

    public partial class OAuth : PhoneApplicationPage
    {
        private OAuthModel viewModel;
        public static AuthType Type { get; set; }

        public OAuth()
        {
            InitializeComponent();

            switch (Type)
            {
                case AuthType.Twitter:
                    viewModel = new TwitterOAuthModel();
                    break;

                case AuthType.Buffer:
                    viewModel = new BufferOAuthModel();
                    break;

                default:
                    throw new NotImplementedException(string.Format("Type {0} unknown", Type));
            }

            wb.Navigated += (sender, e) => viewModel.BrowserNavigated(e);
            wb.Navigating += (sender, e) => viewModel.BrowserNavigating(e);
            this.Loaded += (sender, e) => viewModel.PageLoaded();
            viewModel.BrowserNavigate += (sender, e) => Dependency.Resolve<IDispatcher>().InvokeIfRequired(() => wb.Navigate(e));

            DataContext = viewModel;
        }
    }
}