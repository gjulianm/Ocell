using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.RuntimeData;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using TweetSharp;

namespace Ocell.ViewModels.Lists
{
    [ImplementPropertyChanged]
    public class ManageListsModel : ExtendedViewModelBase
    {
        public SafeObservable<TwitterResource> Lists { get; private set; }
        public DelegateCommand RemoveList { get; set; }
        public object SelectedList { get; set; }

        public ManageListsModel()
        {
            Lists = new SafeObservable<TwitterResource>();
            RemoveList = new DelegateCommand(async param => await DeleteList(param as TwitterResource, ApplicationData.CurrentAccount));

            PropertyChanged += (sender, e) =>
            {
                var list = SelectedList as TwitterResource;
                if (e.PropertyName == "SelectedList" && SelectedList != null && list != null)
                {
                    NavigateToList(list);
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
            Lists.AddListRange(await GetListsForUser(ApplicationData.CurrentAccount));

            base.OnLoad();
        }

        public async Task<IEnumerable<TwitterResource>> GetListsForUser(UserToken user)
        {
            Progress.Loading();

            if (user == null)
                return new List<TwitterResource>();

            var service = ServiceDispatcher.GetService(user);

            var response = await service.ListListsForAsync(new ListListsForOptions
            {
                ScreenName = user.ScreenName
            });

            Progress.Finished();

            if (!response.RequestSucceeded)
                throw new ApplicationException(Resources.ErrorGettingLists, response.Error);

            return response.Content.Select(list => new TwitterResource
            {
                Type = ResourceType.List,
                User = user,
                Data = list.Slug
            }).ToList();
        }

        public async Task DeleteList(TwitterResource list, UserToken user)
        {
            if (list == null)
                return;

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
