using TweetSharp;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Ocell
{
    public static class Clients
    {
        public static TwitterService Service {
            get {
                return Services.First();
            }

            set {
                if(Services.Count > 0)
                    Services[0] = value;
                else
                    Services.Add(value);
            }
        }
        public static List<TwitterService> Services = new List<TwitterService>();
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