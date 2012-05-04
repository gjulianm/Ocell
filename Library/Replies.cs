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
using Hammock;
using TweetSharp;
using Hammock.Authentication.OAuth;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ocell.Library.Twitter
{
    // ATTENTION: This is an EXPERIMENTAL class, because it's using an experimental Twitter API path.
    // Because of this, it's standalone in another class and does not use TweetSharp, just for authentication.
    // Not guaranteed to get results, but will not cause any exceptions to the outside because of serialization.
    public class ReplyService
    {
        protected UserToken _account;

        public ReplyService(UserToken account)
        {
            _account = account;
        }

        public void GetRepliesForStatus(TwitterStatus status, Action<IEnumerable<TwitterStatus>, RestResponse> action)
        {
            RestRequest req = PrepareRestRequest(status.Id);
            RestClient client = GetRestClient();

            client.BeginRequest(req, Callback, action);
        }

        protected void Callback(RestRequest request, RestResponse response, object userState)
        {
            Action<IEnumerable<TwitterStatus>, RestResponse> action = userState as Action<IEnumerable<TwitterStatus>, RestResponse>;

            if (response.StatusCode != HttpStatusCode.OK || response.ContentLength == 0)
            {
                if (action != null)
                    action.Invoke(new List<TwitterStatus>(), response);
                return;
            }

            IEnumerable<string> statusStrings = GetStatusesFromResult(response.Content);

            action.Invoke(DeserializeTweets(statusStrings), response);
        }

        protected RestRequest PrepareRestRequest(long id)
        {
            if (_account == null)
                throw new NullReferenceException("Account is null.");

            var credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = SensitiveData.ConsumerToken,
                ConsumerSecret = SensitiveData.ConsumerSecret,
                Token = _account.Key,
                TokenSecret = _account.Secret,
            };

            RestRequest req = new RestRequest
            {
                Credentials = credentials,
                Path = "/related_results/show/" + id.ToString() + ".json?include_entities=true"
            };

            return req;
        }

        protected RestClient GetRestClient()
        {
            RestClient client = new RestClient();
            client.Authority = Globals.RestAPIAuthority;
            client.VersionPath = "1";

            return client;
        }

        protected IEnumerable<string> GetStatusesFromResult(string result)
        {
            Regex rx = new Regex("pattern goes here");
            Match match = rx.Match(result);
            while(match.Success)
            {
                yield return match.Value;
                match = match.NextMatch();
            }
        }
    }
}
