using System.Collections.Generic;
using AncoraMVVM.Base;
using Ocell.Library.RuntimeData;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using TweetSharp;

namespace Ocell.ViewModels.Lists
{
    [ImplementPropertyChanged]
    public sealed class NewListModel : ExtendedViewModelBase
    {
        public string ListName { get; set; }
        public List<string> PrivacyOptions { get; private set; }
        public string SelectedPrivacy { get; set; }
        public TwitterListMode ListMode
        {
            get { return SelectedPrivacy == Resources.Private ? TwitterListMode.Private : TwitterListMode.Public; }
        }

        public DelegateCommand CreateList { get; private set; }

        public bool CanCreateList
        {
            get { return !string.IsNullOrWhiteSpace(ListName); }
        }

        public NewListModel()
        {
            PrivacyOptions = new List<string> { Resources.Public, Resources.Private };
            CreateList = new DelegateCommand(CreateNewList);
        }

        private async void CreateNewList()
        {
            Progress.Loading();
            var service = ServiceDispatcher.GetCurrentService();
            var response = await service.CreateListAsync(new CreateListOptions
            {
                Name = ListName,
                ListOwner = ApplicationData.CurrentAccount.ScreenName,
                Mode = ListMode
            });

            Progress.Finished();

            if (!response.RequestSucceeded)
            {
                Notificator.ShowError(Resources.ErrorCreatingList + response.Error.Message);
                return;
            }

            var resource = new TwitterResource
            {
                Data = ListName,
                Type = ResourceType.List,
                User = ApplicationData.CurrentAccount
            };

            Navigator.MessageAndNavigate<ListModel, TwitterResource>(resource);
            Navigator.ClearLastStackEntries(1);
        }
    }
}
