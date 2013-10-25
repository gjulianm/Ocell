using AncoraMVVM.Rest;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ocell.Library.ReadLater.Instapaper
{
    public class InstapaperService : BaseService, IReadLaterService
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        public InstapaperService(string user, string pass)
            : base(new HttpService())
        {
            UserName = user;
            Password = pass;

            Authority = "https://www.instapaper.com/";
            BasePath = "api/";

            PersistentUrlParameters.Add("username", UserName);
            PersistentUrlParameters.Add("password", Password);
        }


        /// <summary>
        /// Checks if the credentials are valid.
        /// </summary>
        public async Task<HttpResponse> CheckCredentials()
        {
            return await CreateAndExecute("authenticate", HttpMethod.Post);
        }

        /// <summary>
        /// Adds an URL to read later.
        /// </summary>
        /// <param name="url">URL to add.</param>
        public async Task<HttpResponse> AddUrl(string url)
        {
            return await CreateAndExecute("url", HttpMethod.Post, "url", url);
        }

        /// <summary>
        /// Adds an URL to read later with selected text.
        /// </summary>
        /// <param name="url">URL to add.</param>
        /// <param name="tweetText">Tweet text.</param>
        public async Task<HttpResponse> AddUrl(string url, string tweetText)
        {
            return await CreateAndExecute("add", HttpMethod.Post, "selection", tweetText, "url", url);
        }

        protected override T Deserialize<T>(string content)
        {
            throw new NotImplementedException(); // Not needed.
        }
    }
}
