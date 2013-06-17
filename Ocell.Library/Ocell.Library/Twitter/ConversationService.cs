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
    public class ConversationService
    {
        ITwitterService service;
        SafeObservable<TwitterStatus> searchCache = new SafeObservable<TwitterStatus>();
        SafeObservable<TwitterStatus> processedStatuses = new SafeObservable<TwitterStatus>();
        int pendingCalls = 0;
        Action<IEnumerable<TwitterStatus>, TwitterResponse> callback;
        TwitterResponse okResponse;
        SafeObservable<SearchRequest> searchesPerformed = new SafeObservable<SearchRequest>();

        public ConversationService(UserToken token)
        {
            service = ServiceDispatcher.GetService(token);
        }

        /// <summary>
        /// Return the conversation for a given status. 
        /// </summary>
        /// <param name="id">Tweet ID</param>
        /// <param name="action">Callback. This will be called various times, one for each Twitter response.</param>
        public void GetConversationForStatus(TwitterStatus status, Action<IEnumerable<TwitterStatus>, TwitterResponse> action)
        {
            callback = action;
            GetConversationForStatus(status);
        }

        private void GetConversationForStatus(TwitterStatus status)
        {
            GetRepliesFor(status);
            GetStatusReplied(status);
        }

        private void GetStatusReplied(TwitterStatus status)
        {
            if (status.InReplyToStatusId != null)
            {
                Interlocked.Increment(ref pendingCalls);
                service.GetTweet(new GetTweetOptions { Id = (long)status.InReplyToStatusId }, (result, response) =>
                {
                    if (result == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        RaiseCallback(new List<TwitterStatus>(), response); // report the error
                        TryFinish();
                        return;
                    }

                    okResponse = response;
                    if (!searchCache.Contains(result))
                        searchCache.Add(result);

                    RaiseCallback(new List<TwitterStatus> { status, result }, response);

                    GetConversationForStatus(result);

                    TryFinish();
                });
            }
        }

        private struct SearchRequest
        {
            public string user;
            public long from;
        }

        private bool AreResultsCached(string user, long from)
        {
            return searchesPerformed.Any(x => x.user == user && x.from <= from);
        }

        private void AddCachedResult(string user, long from)
        {
            searchesPerformed.Add(new SearchRequest
            {
                user = user,
                from = from
            });
        }

        private void GetRepliesFor(TwitterStatus status)
        {
            if (processedStatuses.Contains(status))
                return;

            processedStatuses.Add(status); // There's a possibility that a status can be processed two times... but it's not significant nor a real problem.

            if (AreResultsCached(status.AuthorName, status.Id))
            {
                RetrieveRepliesFromCache(status);
                TryFinish();
            }
            else
            {
                Interlocked.Increment(ref pendingCalls);
                service.Search(new SearchOptions { Q = "to:" + status.AuthorName, SinceId = status.Id, Count = 100 },
                    (result, response) => SearchCallback(result, response, status));
            }
        }

        private void SearchCallback(TwitterSearchResult result, TwitterResponse response, TwitterStatus status)
        {
            if (result == null || response.StatusCode != HttpStatusCode.OK || result.Statuses == null)
            {
                RaiseCallback(new List<TwitterStatus>(), response); // report the error
                TryFinish();
                return;
            }

            okResponse = response;
            searchCache.BulkAdd(result.Statuses.Except(searchCache));

            if (result.Statuses.Count() >= 90)
            {
                // There are still more statuses to retrieve
                Interlocked.Increment(ref pendingCalls);
                service.Search(new SearchOptions { Q = "to:" + status.AuthorName, SinceId = status.Id, MaxId = result.Statuses.Min(x => x.Id), Count = 100 },
                    (rs, rp) => SearchCallback(rs, rp, status));
            }
            else
            {
                // Finished!
                AddCachedResult(status.AuthorName, status.Id);
            }

            RetrieveRepliesFromCache(status);

            TryFinish();
        }

        private void RetrieveRepliesFromCache(TwitterStatus status)
        {
            var replies = searchCache.Where(x => x.InReplyToStatusId == status.Id);

            foreach (var reply in replies)
                GetRepliesFor(reply);

            if (replies.Any())
                RaiseCallback(replies, okResponse);
        }

        private void RaiseCallback(IEnumerable<TwitterStatus> statuses, TwitterResponse response)
        {
            if (callback != null)
                callback.Invoke(statuses, response);
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

        public void GetConversationForStatus(string status, Action<IEnumerable<TwitterStatus>, TwitterResponse> action)
        {
            long id;
            if (long.TryParse(status, out id))
            {
                Interlocked.Increment(ref pendingCalls);
                service.GetTweet(new GetTweetOptions { Id = id }, (s, response) =>
                {
                    if (s == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        RaiseCallback(new List<TwitterStatus>(), response);
                        TryFinish();
                        return;
                    }

                    GetConversationForStatus(s, action);
                    TryFinish();
                });
            }
        }
    }
}
