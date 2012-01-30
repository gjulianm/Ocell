using TweetSharp;

namespace Ocell
{
    public static class Clients
    {
        public static TwitterService Service;
        public static bool isServiceInit = false;
        public static string ScreenName;
        public static int Uid;
        public static void fillScreenName()
        {
            if (isServiceInit)
                Service.GetUserProfile((user, resp) =>
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK) {
                        ScreenName = user.ScreenName;
                        Uid = user.Id;
                        if (UserFilled != null)
                            UserFilled();
                    }
                });
        }
       
        public delegate void OnUserFilled();
        public static event OnUserFilled UserFilled;
    }
}