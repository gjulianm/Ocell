using DanielVaughan;
using DanielVaughan.Services;
using Microsoft.Phone.Notification;
using Ocell.Library;
using Ocell.Library.Notifications;
using Ocell.Library.Twitter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Ocell
{
    public enum OSVersion { WP7, WP8 }

    public static class PushNotifications
    {
        public static OSVersion WPVersion { get; set; }

        private struct RegistrationInfo
        {
            public string ChannelUri;
            public string Type;
            public UserToken User;
        }

        public static void AutoRegisterForNotifications()
        {
            if (Config.PushEnabled != true || !TrialInformation.IsFullFeatured)
                return;

            HttpNotificationChannel channel;
            string channelName = "OcellPushChannel";

            try
            {
                channel = OpenOrCreateChannel(channelName);
            }
            catch (Exception)
            {
                return; // Don't report to user.
            }

            if (channel.ChannelUri == null)
                return;

            string channelUri = channel.ChannelUri.ToString();
            List<RegistrationInfo> regs = new List<RegistrationInfo>();

            foreach (var user in Config.Accounts)
            {
                if (user.Preferences.MentionsPreferences != NotificationType.None &&
                    user.Preferences.MessagesPreferences != NotificationType.None)
                {
                    regs.Add(new RegistrationInfo
                    {
                        ChannelUri = channelUri,
                        Type = "mm",
                        User = user
                    });
                }
                else if (user.Preferences.MessagesPreferences != NotificationType.None)
                {
                    regs.Add(new RegistrationInfo
                    {
                        ChannelUri = channelUri,
                        Type = "messages",
                        User = user
                    });
                }
                else if (user.Preferences.MentionsPreferences != NotificationType.None)
                {
                    regs.Add(new RegistrationInfo
                    {
                        ChannelUri = channelUri,
                        Type = "mentions",
                        User = user
                    });
                }
            }

            if (regs.Count > 0)
                SendRegistrationToServer(regs);
        }

        private static HttpNotificationChannel OpenOrCreateChannel(string channelName)
        {
            HttpNotificationChannel channel;
            channel = HttpNotificationChannel.Find(channelName);

            if (channel == null)
            {
                channel = new HttpNotificationChannel(channelName);
                channel.Open();
                channel.BindToShellTile();
                channel.BindToShellToast();
            }
            return channel;
        }

        public static void UnregisterAll()
        {
            foreach (var user in Config.Accounts)
                UnregisterPushChannel(user, "mm");
        }

        [Conditional("OCELL_FULL")]
        private static async void SendRegistrationToServer(IEnumerable<RegistrationInfo> regs)
        {
            // I'm just so sorry about all this crap.
            string separator = "¬";
            string urls = "", tokens = "", names = "", types = "";
            string version = WPVersion == OSVersion.WP8 ? "8" : "7";

            foreach (var reg in regs)
            {
                urls += reg.ChannelUri + separator;
                names += reg.User.ScreenName + separator;
                tokens += Library.Encrypting.EncodeTokens(reg.User.Key, reg.User.Secret) + separator;
                types += reg.Type + separator;
            }

            // Remove last separator
            urls.Substring(0, urls.Length - 1);
            tokens.Substring(0, tokens.Length - 1);
            names.Substring(0, names.Length - 1);
            types.Substring(0, types.Length - 1);

            string postContents = "{\"AccessTokens\" : \"" + tokens + "\",\"PushUris\" : \"" + urls + "\",\"Usernames\" : \"" + names + "\",\"Types\" : \"" + types + "\",\"OSVersion\" : \"" + version + "\"}";

            var request = new HttpRequestMessage(HttpMethod.Post, SensitiveData.PushRegisterPostUriFormat);

            request.Content = new StringContent(postContents, UTF8Encoding.UTF8, "application/json");

            try
            {
                var response = await new HttpClient().SendAsync(request);
                ReportRegisterToUser(response);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error sending push registration: {0}", ex);
            }
        }

        [Conditional("DEBUG")]
        private static void ReportRegisterToUser(HttpResponseMessage resp)
        {
            string msg = string.Format("Response from push server: {0}", resp.StatusCode);
            Dependency.Resolve<IMessageService>().ShowMessage(msg);
        }

        public static void UnregisterPushChannel(UserToken user, string type)
        {
            HttpNotificationChannel channel;
            string channelName = "OcellPushChannel";
            channel = HttpNotificationChannel.Find(channelName);

            if (channel != null)
                channel.Close();

            SendRemoveRequestToServer(user, type);
        }

        [Conditional("OCELL_FULL")]
        private static async void SendRemoveRequestToServer(UserToken token, string type)
        {
            string encoded = Library.Encrypting.EncodeTokens(token.Key, token.Secret);
            string url = string.Format(Library.SensitiveData.PushUnregisterUriFormat, Uri.EscapeDataString(encoded), type);

            var request = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                var response = await request.GetResponseAsync();
            }
            catch (Exception)
            {
            }
        }
    }
}