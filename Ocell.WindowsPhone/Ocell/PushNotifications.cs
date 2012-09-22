using System;
using System.Net;
using Microsoft.Phone.Notification;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Diagnostics;

namespace Ocell
{
    public static class PushNotifications
    {
        public static void RegisterPushChannel()
        {
            HttpNotificationChannel channel;
            string channelName = "OcellPushChannel";
            channel = HttpNotificationChannel.Find(channelName);

            if (channel == null)
            {
                channel = new HttpNotificationChannel(channelName);
                channel.Open();
                channel.BindToShellTile();
                channel.BindToShellToast();
            }

            if (channel.ChannelUri == null)
                return;

            foreach(var user in Config.Accounts)
                SendRegistrationToServer(user, channel.ChannelUri.ToString());
        }

        [Conditional("OCELL_FULL")]
        static void SendRegistrationToServer(UserToken user, string channelUri)
        {
            string url = string.Format(Library.SensitiveData.PushRegisterUriFormat,
                Uri.EscapeDataString(channelUri), user.Key, user.Secret, user.ScreenName);

            var request = (HttpWebRequest)WebRequest.Create(url);

            var response = request.BeginGetResponse((result) => 
            {
            }, null);
        }

        public static void UnregisterPushChannel()
        {
            HttpNotificationChannel channel;
            string channelName = "OcellPushChannel";
            channel = HttpNotificationChannel.Find(channelName);

            if (channel != null)
                channel.Close();

            foreach (var user in Config.Accounts)
                SendRemoveRequestToServer(user);
        }

        [Conditional("OCELL_FULL")]
        static void SendRemoveRequestToServer(UserToken token)
        {
            string url = string.Format(Library.SensitiveData.PushUnregisterUriFormat, token.Key);

            var request = (HttpWebRequest)WebRequest.Create(url);

            var response = request.BeginGetResponse((result) =>
            {
            }, null);
        }
    }
}
