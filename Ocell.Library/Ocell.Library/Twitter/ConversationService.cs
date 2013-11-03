using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TweetSharp;

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

        private async void GetStatusReplied(TwitterStatus status)
        {
            if (status.InReplyToStatusId != null)
            {
                Interlocked.Increment(ref pendingCalls);
                var response = await service.GetTweetAsync(new GetTweetOptions { Id = (long)status.InReplyToStatusId });

                if (!response.RequestSucceeded)
                {
                    RaiseCallback(new List<TwitterStatus>(), response); // report the error
                    TryFinish();
                    return;
                }

                var result = response.Content;

                okResponse = response;
                if (!searchCache.Contains(result))
                    searchCache.Add(result);

                RaiseCallback(new List<TwitterStatus> { status, result }, response);

                GetConversationForStatus(result);

                TryFinish();
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
                SearchReplies(status);
            }
        }

        private async void SearchReplies(TwitterStatus status, long? maxId = null)
        {
            Interlocked.Increment(ref pendingCalls);

            var response = await service.SearchAsync(new SearchOptions { Q = "to:" + status.AuthorName, SinceId = status.Id, Count = 100, MaxId = maxId });

            if (!response.RequestSucceeded)
            {
                RaiseCallback(new List<TwitterStatus>(), response); // report the error
                TryFinish();
                return;
            }

            var result = response.Content;

            okResponse = response;
            searchCache.BulkAdd(result.Statuses.Except(searchCache));

            if (result.Statuses.Count() >= 90)
            {
                // There are still more statuses to retrieve
                SearchReplies(status, result.Statuses.Min(x => x.Id));
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

        public async void GetConversationForStatus(string statusId, Action<IEnumerable<TwitterStatus>, TwitterResponse> action)
        {
            long id;
            if (long.TryParse(statusId, out id))
            {
                Interlocked.Increment(ref pendingCalls);
                var response = await service.GetTweetAsync(new GetTweetOptions { Id = id });

                if (!response.RequestSucceeded)
                {
                    RaiseCallback(new List<TwitterStatus>(), response);
                    TryFinish();
                    return;
                }

                GetConversationForStatus(response.Content, action);
                TryFinish();
            }
        }
    }
}
