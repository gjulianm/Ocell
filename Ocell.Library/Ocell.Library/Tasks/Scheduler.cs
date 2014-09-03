using Ocell.Library.Crypto;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ocell.Library.Tasks
{
    public class Scheduler
    {
        string accessToken;

        public Scheduler(string token, string secret)
        {
            accessToken = TokenCombinator.EncodeTokens(token, secret);
        }

#if OCELL_FULL
        public async Task<HttpWebResponse> ScheduleTweet(string text, DateTime dueTime)
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

            return response;
        }
#endif
    }
}
