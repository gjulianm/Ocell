﻿using System;
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

namespace Ocell.Library.Twitter
{
    // ATTENTION: This is an EXPERIMENTAL class, because it's using an experimental Twitter API path.
    // Because of this, it's standalone in another class and does not use TweetSharp, just for authentication.
    // Not guaranteed to get results, but will not cause any exceptions to the outside because of serialization.
    public class ConversationService
    {
        protected UserToken _account;
        protected Action<IEnumerable<TwitterStatus>, TwitterResponse> _action;
        protected int _pendingCalls;
        protected List<string> _processedStatus;

        public ConversationService(UserToken account)
        {
            _account = account;
            _pendingCalls = 0;
            _processedStatus = new List<string>();
        }

        public void GetConversationForStatus(string Id, Action<IEnumerable<TwitterStatus>, TwitterResponse> action)
        {
            _action = action;
            GetReplies(Id);
            GetReplied(long.Parse(Id));            
        }
        public void GetConversationForStatus(TwitterStatus status, Action<IEnumerable<TwitterStatus>, TwitterResponse> action)
        {
            GetConversationForStatus(status.Id.ToString(), action);
        }

        protected void GetReplies(string Id)
        {
            if (_processedStatus.Contains("a" + Id))
                return;

            _processedStatus.Add("a" + Id);

            RestRequest req = PrepareRestRequest(Id);
            RestClient client = GetRestClient();

            _pendingCalls++;
            client.BeginRequest(req, Callback, null);
        }

        protected void GetReplied(long Id)
        {
            if (_processedStatus.Contains("b" + Id.ToString()))
                return;

            _processedStatus.Add("b" + Id.ToString());

            TwitterService srv = ServiceDispatcher.GetService(_account);
            _pendingCalls++;
            srv.GetTweet(Id, ReceiveSingleTweet);
        }

        protected RestRequest PrepareRestRequest(string id)
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
                Path = "/related_results/show/" + id + ".json?include_entities=true"
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

        protected void Callback(RestRequest request, RestResponse response, object userState)
        {
            Action<IEnumerable<TwitterStatus>, TwitterResponse> action = _action;

            if (response.StatusCode != HttpStatusCode.OK || response.ContentLength == 0)
            {
                if (action != null)
                    action.Invoke(new List<TwitterStatus>(), new TwitterResponse(response));
                TryFinish();
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
                GetConversationForStatus(status, _action);

            if(action != null)
                action.Invoke(statuses, new TwitterResponse(response));

            TryFinish();
        }
        protected void ReceiveSingleTweet(TwitterStatus status, TwitterResponse response)
        {
            List<TwitterStatus> list = new List<TwitterStatus>();
            if (status == null || response.StatusCode != HttpStatusCode.OK)
            {
                if (_action != null)
                    _action.Invoke(list, response);
            }

            list.Add(status);
            if (status.InReplyToStatusId != null)
                GetReplied((long)status.InReplyToStatusId);

            GetReplies(status.Id.ToString());

            if(_action != null)
                _action.Invoke(list, response);
            TryFinish();
        }

        protected IEnumerable<TwitterStatus> DeserializeTweets(IEnumerable<string> strings)
        {
            TwitterService srv = ServiceDispatcher.GetDefaultService();

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
            _pendingCalls--;
            if (_pendingCalls <= 0)
            {
                if (Finished != null)
                    Finished(this, new EventArgs());
            }
        }

        public event EventHandler Finished;
        
    }
}