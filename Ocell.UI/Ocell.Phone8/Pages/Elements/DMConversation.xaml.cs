using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Microsoft.Phone.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Pages.Elements;
using System.Linq;
using System.Windows;

namespace Ocell
{
    public partial class DMConversation : PhoneApplicationPage
    {
        DMConversationModel viewModel = new DMConversationModel();
        public DMConversation()
        {
            InitializeComponent();

            InitializeComponent();
            Loaded += (sender, e) => { if (ApplicationBar != null) ApplicationBar.MatchOverriddenTheme(); };


            Loaded += new RoutedEventHandler(DMConversation_Loaded);
            List.Loader.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsLoading")
                        Dependency.Resolve<IProgressIndicator>().IsLoading = List.Loader.IsLoading;
                };

            DataContext = viewModel;
        }

        void DMConversation_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataTransfer.DMGroup == null)
            {
                NavigationService.GoBack();
                return;
            }

            // TODO: Add range to DM.
            // List.Loader.Source.AddRange(DataTransfer.DMGroup.Messages.Cast<ITweetable>());

            string pairName = DataTransfer.DMGroup.Messages.First().GetPairName(DataTransfer.CurrentAccount);

            TwitterResource resource = new TwitterResource
            {
                Type = ResourceType.MessageConversation,
                User = DataTransfer.CurrentAccount,
                Data = pairName
            };

            List.Loader.Resource = resource;
            viewModel.PairName = pairName;
        }
    }
}
