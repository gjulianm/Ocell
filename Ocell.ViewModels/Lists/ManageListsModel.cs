using System.Collections.Generic;
using System.Threading.Tasks;
using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using TweetSharp;

namespace Ocell.ViewModels
{
    [ImplementPropertyChanged]
    public class ManageListsModel : ExtendedViewModelBase
    {
        public SafeObservable<TwitterResource> Lists { get; private set; }
        public DelegateCommand RemoveList { get; set; }
        public TwitterResource SelectedList { get; set; }

        public ManageListsModel()
        {
            Lists = new SafeObservable<TwitterResource>();
            RemoveList = new DelegateCommand(async (param) => await DeleteList(param as TwitterResource, DataTransfer.CurrentAccount));

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "SelectedList" && SelectedList != null)
                {
                    NavigateToList(SelectedList);
                    SelectedList = null;
                }
            };
        }

        private void NavigateToList(TwitterResource list)
        {
            Navigator.MessageAndNavigate<ListModel, TwitterResource>(list);
        }

        public async override void OnLoad()
        {
            Lists.AddListRange(await GetListsForUser(DataTransfer.CurrentAccount));

            base.OnLoad();
        }

        public async Task<IEnumerable<TwitterResource>> GetListsForUser(UserToken user)
        {
            var lists = new List<TwitterResource>();

            if (user == null)
                return lists;

            var service = ServiceDispatcher.GetService(user);

            var response = await service.ListListsForAsync(new ListListsForOptions
            {
                ScreenName = user.ScreenName
            });

            if (!response.RequestSucceeded)
                throw new ApplicationException(Resources.ErrorGettingLists, response.Error);

            foreach (var list in response.Content)
            {
                var resource = new TwitterResource
                {
                    Type = ResourceType.List,
                    User = user,
                    Data = list.Slug
                };

                lists.Add(resource);
            }

            return lists;
        }

        public async Task DeleteList(TwitterResource list, UserToken user)
        {
            var service = ServiceDispatcher.GetService(user);

            Progress.Loading();

            var response = await service.DeleteListAsync(new DeleteListOptions
            {
                Slug = list.Data,
                OwnerScreenName = user.ScreenName
            });

            Progress.Finished();

            if (!response.RequestSucceeded)
                throw new ApplicationException(Resources.ErrorDeletingList, response.Error);

            Lists.Remove(list);
        }

    }
}
