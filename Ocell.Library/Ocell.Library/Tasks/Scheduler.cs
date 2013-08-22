using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Ocell.Library.Tasks
{
    public class Scheduler
    {
        string accessToken;

        public Scheduler(string token, string secret)
        {
            accessToken = Encrypting.EncodeTokens(token, secret);
        }

        [Conditional("OCELL_FULL")]
        public async void ScheduleTweet(string text, DateTime dueTime, RequestCallback callback)
        {
            TimeSpan diff = dueTime - DateTime.Now;
            long delay = (long)diff.TotalMilliseconds;

            string url = String.Format(SensitiveData.ScheduleUriformat, Uri.EscapeDataString(accessToken), Uri.EscapeDataString(text), delay);

            var request = (HttpWebRequest)WebRequest.Create(url);
            
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch (WebException e)
            {
                response = (HttpWebResponse)e.Response;
            }
            
            if (callback != null)
                    callback(this, response);
        }
    }

    public delegate void RequestCallback(object sender, HttpWebResponse response);
}
