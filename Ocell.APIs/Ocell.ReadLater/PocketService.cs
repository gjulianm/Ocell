using AncoraMVVM.Rest;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ocell.Library.ReadLater.Pocket
{
    public class PocketService : BaseService, IReadLaterService
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public PocketService(string user, string pass)
            : base(new HttpService())
        {
            Authority = "https://readitlaterlist.com/";
            BasePath = "v2/";

            Username = user;
            Password = pass;

            PersistentUrlParameters.Add("username", Username);
            PersistentUrlParameters.Add("password", Password);
            PersistentUrlParameters.Add("apikey", SensitiveData.PocketAPIKey);
        }



        /// <summary>
        /// Checks if the credentials are valid.
        /// </summary>
        public async Task<HttpResponse> CheckCredentials()
        {
            return await CreateAndExecute("auth", HttpMethod.Get);
        }

        /// <summary>
        /// Adds an URL to read later.
        /// </summary>
        /// <param name="url">URL to add.</param>
        public async Task<HttpResponse> AddUrl(string url)
        {
            return await CreateAndExecute("add", HttpMethod.Get, "url", url);
        }

        /// <summary>
        /// Adds a URL to Pocker with reference to the tweet which generated it.
        /// </summary>
        /// <param name="url">URL to save.</param>
        /// <param name="tweetId">Tweet ID.</param>
        public async Task<HttpResponse> AddUrl(string url, long tweetId)
        {
            return await CreateAndExecute("add", HttpMethod.Get, "ref_id", tweetId, "url", url);
        }

        protected override T Deserialize<T>(string content)
        {
            throw new NotImplementedException(); // Not needed.
        }
    }
}
