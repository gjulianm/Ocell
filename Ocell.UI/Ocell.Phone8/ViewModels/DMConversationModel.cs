using AncoraMVVM.Base.Diagnostics;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;
using System.Linq;
using Ocell.Library.RuntimeData;
using TweetSharp;

namespace Ocell.Pages.Elements
{
    [ImplementPropertyChanged]
    public class DMConversationModel : ExtendedViewModelBase
    {
        public string PairName { get; set; }
        public TweetLoader Loader { get; set; }
        public GroupedDM DMGroup { get; set; }

        public DMConversationModel()
        {
            DMGroup = ReceiveMessage<GroupedDM>();

            if (DMGroup == null)
            {
                AncoraLogger.Instance.LogEvent("Unable to receive grouped DM in DMConversationModel");
                Navigator.GoBack();
                return;
            }

            PairName = DMGroup.Messages.First().GetPairName(ApplicationData.CurrentAccount);

            var resource = new TwitterResource
            {
                Type = ResourceType.MessageConversation,
                User = ApplicationData.CurrentAccount,
                Data = PairName
            };

            Loader = new TweetLoader(resource);
            Loader.Source.AddRange(DMGroup.Messages.Cast<ITweetable>());

            Loader.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsLoading")
                    Progress.IsLoading = Loader.IsLoading;
            };
        }
    }
}
