using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Authentication;
using Ocell.Library.Twitter;
using System;

namespace Ocell.LightTwitterService
{

    public class LightTwitterClient
    {
        RestClient _client;
        string _userToken, _userSecret;
        string _consumerToken, _consumerSecret;

        public LightTwitterClient(string consumerToken, string consumerSecret, string userToken, string userSecret)
        {
            _client = new RestClient { Authority = "http://api.twitter.com/", VersionPath = "1" };
            _userToken = userToken;
            _userSecret = userSecret;
            _consumerToken = consumerToken;
            _consumerSecret = consumerSecret;

        }

        public RestRequest TwitterResourceToRequest(TwitterResource resource, int count)
        {
            string path = "";
            switch (resource.Type)
            {
                case ResourceType.Favorites:
                    path = "favorites.json";
                    break;
                case ResourceType.Home:
                    path = "statuses/home_timeline.json?count=" + count.ToString();
                    break;
                case ResourceType.List:
                    path = String.Format("lists/statuses.json?owner_screen_name={0}&slug={1}&per_page={2}",
                        resource.Data.Substring(1, resource.Data.IndexOf('/') - 1),
                                resource.Data.Substring(resource.Data.IndexOf('/') + 1), count);
                    break;
                case ResourceType.Mentions:
                    path = "statuses/mentions.json?count=" + count.ToString();
                    break;
                case ResourceType.Messages:
                    path = "direct_messages.json?count=" + count.ToString();
                    break;
                case ResourceType.Search:
                    path = "search.json?q=" + resource.Data;
                    break;
                case ResourceType.Tweets:
                    path = "statuses/user_timeline.json?screen_name=" + resource.Data;
                    break;
                default:
                    throw new NotSupportedException("This resource is not supported");
            }

            return new RestRequest
            {
                Path = path,
                Credentials = new OAuthCredentials
                            {
                                Type = OAuthType.ProtectedResource,
                                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                ConsumerKey = _consumerToken,
                                ConsumerSecret = _consumerSecret,
                                Token = _userToken,
                                TokenSecret = _userSecret
                            }
            };

        }

        public void GetStatuses(TwitterResource resource, int count, Action<TwitterObjectCollection, RestResponse> action)
        {
            _client.BeginRequest(TwitterResourceToRequest(resource, count), ReceiveStatuses, action);
        }

        void ReceiveStatuses(RestRequest request, RestResponse response, object userState)
        {
            Action<TwitterObjectCollection, RestResponse> action = userState as Action<TwitterObjectCollection, RestResponse>;

            var collection = new TwitterObjectCollection(response.Content);

            if (action != null)
                action(collection, response);
        }

        public void ListMentions(int count, Action<TwitterObjectCollection, RestResponse> action)
        {
            TwitterResource resource = new TwitterResource { Type = ResourceType.Mentions };
            GetStatuses(resource, count, action);
        }

        public void ListMessages(int count, Action<TwitterObjectCollection, RestResponse> action)
        {
            TwitterResource resource = new TwitterResource { Type = ResourceType.Messages };
            GetStatuses(resource, count, action);
        }
    }
}