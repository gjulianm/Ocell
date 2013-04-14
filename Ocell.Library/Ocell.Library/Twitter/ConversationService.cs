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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;

namespace Ocell.Library.Twitter
{
    // ATTENTION: This is an EXPERIMENTAL class, because it's using an experimental Twitter API path.
    // Because of this, it's standalone in another class and does not use TweetSharp, just for authentication.
    // Not guaranteed to get results, but will not cause any exceptions to the outside because of serialization.
    public class ConversationService
    {
        protected UserToken account;
        protected Action<IEnumerable<TwitterStatus>, TwitterResponse> callback;
        protected int pendingCalls;
        protected SafeObservable<string> processedForReplies;
        protected SafeObservable<string> processedForReplied;
        protected Action<bool> checkFirstReplyCallback;
        protected bool checkOnlyFirstReply;

        public ConversationService(UserToken token)
        {
            account = token;
            pendingCalls = 0;
            processedForReplies = new SafeObservable<string>();
            processedForReplied = new SafeObservable<string>();
            checkOnlyFirstReply = false;
        }

        /// <summary>
        /// Queries the Twitter API to check if a tweet has replies.
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="action">Callback. The bool argument indicates if there are replies</param>
        public void CheckIfReplied(TwitterStatus status, Action<bool> action)
        {
            callback = CheckIfRepliedCallback;
            checkFirstReplyCallback = action;
            checkOnlyFirstReply = true;
            GetReplies(status.Id.ToString());
        }

        protected void CheckIfRepliedCallback(IEnumerable<TwitterStatus> statuses, TwitterResponse response)
        {
            if (checkFirstReplyCallback != null)
                checkFirstReplyCallback.Invoke(statuses.Any());
        }

        /// <summary>
        /// Return the conversation for a given status. 
        /// </summary>
        /// <param name="id">Tweet ID</param>
        /// <param name="action">Callback. This will be called various times, one for each Twitter response.</param>
        public void GetConversationForStatus(string id, Action<IEnumerable<TwitterStatus>, TwitterResponse> action)
        {
            callback = action;
            checkOnlyFirstReply = false;
            GetReplies(id);
            GetReplied(long.Parse(id));
        }

        /// <summary>
        /// Return the conversation for a given status. 
        /// </summary>
        /// <param name="status">Tweet.</param>
        /// <param name="action">Callback. This will be called various times, one for each Twitter response.</param>
        public void GetConversationForStatus(TwitterStatus status, Action<IEnumerable<TwitterStatus>, TwitterResponse> action)
        {
            GetConversationForStatus(status.Id.ToString(), action);
        }

        protected void GetReplies(string id)
        {
            if (processedForReplies.Contains(id))
                return;

            processedForReplies.Add(id);

            RestRequest req = PrepareRestRequest(id);
            RestClient client = GetRestClient();

            Interlocked.Increment(ref pendingCalls);
            client.BeginRequest(req, Callback, null);
        }

        protected void GetReplied(long id)
        {
            if (processedForReplied.Contains(id.ToString()))
                return;

            processedForReplies.Add(id.ToString());

            ITwitterService srv = ServiceDispatcher.GetService(account);
            pendingCalls++;
            srv.GetTweet(new GetTweetOptions { Id = id }, ReceiveSingleTweet);
        }

        protected RestRequest PrepareRestRequest(string id)
        {
            var credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = SensitiveData.ConsumerToken,
                ConsumerSecret = SensitiveData.ConsumerSecret,
            };

            if (account != null)
            {
                credentials.Token = account.Key;
                credentials.TokenSecret = account.Secret;
            }

            RestRequest req = new RestRequest
            {
                Credentials = credentials,
                Path = "/related_results/show/" + id + ".json?include_entities=true"
            };

            return req;
        }
        protected RestClient GetRestClient()
        {
            RestClient client = new RestClient();
            client.Authority = Globals.Authority;
            client.VersionPath = "1";

            return client;
        }

        protected void Callback(RestRequest request, RestResponse response, object userState)
        {
            if (response.StatusCode != HttpStatusCode.OK || response.ContentLength == 0)
            {
                if (callback != null)
                    callback.Invoke(new List<TwitterStatus>(), new TwitterResponse(response, null));
                TryFinish();
                return;
            }

            if (checkOnlyFirstReply)
            {
                if (checkFirstReplyCallback != null)
                    checkFirstReplyCallback.Invoke(response.ContentLength > 2);
                return;
            }

            IEnumerable<TwitterStatus> statuses;
            try
            {
                IEnumerable<string> statusStrings = GetStatusesFromResult(response.Content);
                statuses = new List<TwitterStatus>(DeserializeTweets(statusStrings));
            }
            catch (Exception)
            {
                statuses = new List<TwitterStatus>();
            }

            foreach (var status in statuses)
                GetConversationForStatus(status, callback);

            if (callback != null)
                callback.Invoke(statuses, new TwitterResponse(response));

            TryFinish();
        }

        protected void ReceiveSingleTweet(TwitterStatus status, TwitterResponse response)
        {
            List<TwitterStatus> list = new List<TwitterStatus>();
            if (status == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (callback != null)
                    callback.Invoke(list, response);
                return;
            }

            list.Add(status);
            if (status.InReplyToStatusId != null)
                GetReplied((long)status.InReplyToStatusId);

            GetReplies(status.Id.ToString());

            if (callback != null)
                callback.Invoke(list, response);
            TryFinish();
        }

        protected IEnumerable<TwitterStatus> DeserializeTweets(IEnumerable<string> strings)
        {
            TwitterService srv = new TwitterService();

            // Little side note: I LOVE yield return.
            foreach (string status in strings)
                yield return srv.Deserialize<TwitterStatus>(status);
        }
        protected IEnumerable<string> GetStatusesFromResult(string result)
        {
            int valueStart;
            string value;
            int pos;
            int pendingBrackets;

            valueStart = result.IndexOf("value\":");
            while (valueStart != -1)
            {
                pos = result.IndexOf('{', valueStart) + 1;
                pendingBrackets = 1;
                value = "{";
                while (pendingBrackets > 0)
                {
                    value += result[pos];
                    if (result[pos] == '{')
                        pendingBrackets++;
                    if (result[pos] == '}')
                        pendingBrackets--;
                    pos++;
                }
                yield return value;
                valueStart = result.IndexOf("value\":", pos);
            }
        }

        private void TryFinish()
        {
            if (Interlocked.Decrement(ref pendingCalls) <= 0)
            {
                if (Finished != null)
                    Finished(this, new EventArgs());
            }
        }

        public event EventHandler Finished;
    }
}
