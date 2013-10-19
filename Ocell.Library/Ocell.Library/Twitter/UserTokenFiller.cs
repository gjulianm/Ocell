using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using TweetSharp;

namespace Ocell.Library.Twitter
{
    public class UserTokenFiller
    {
        public async Task FillUserData(UserToken token)
        {
            if (!(string.IsNullOrWhiteSpace(token.ScreenName) || string.IsNullOrWhiteSpace(token.AvatarUrl) || token.Id == null))
                return;

            ITwitterService service = ServiceDispatcher.GetService(token);

            var response = await service.GetUserProfileAsync(new GetUserProfileOptions { IncludeEntities = true });

            if (!response.RequestSucceeded)
                return;

            var user = response.Content;

            token.ScreenName = user.ScreenName;
            token.Id = user.Id;
            token.AvatarUrl = user.ProfileImageUrl;
        }
    }
}
