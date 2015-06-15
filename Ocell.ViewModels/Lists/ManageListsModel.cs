using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;
using PropertyChanged;
using System.Collections.Generic;
using System.Threading.Tasks;
using TweetSharp;

namespace Ocell.ViewModels
{
    [ImplementPropertyChanged]
    public class ManageListsModel : ExtendedViewModelBase
    {
        public SafeObservable<TwitterResource> Lists { get; private set; }

        public ManageListsModel()
        {
            Lists = new SafeObservable<TwitterResource>();
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


    }
}
