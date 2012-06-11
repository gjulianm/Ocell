using System;
using System.Net;
using TweetSharp;
using Ocell.Library.Twitter;

namespace Ocell.Library.Tasks
{
    public class TaskExecutor
    {
        public TwitterStatusTask Task { get; set; }
        TwitterResponse _error = null;
        int PendingCalls;

        public TaskExecutor(TwitterStatusTask task)
        {
            Task = task;
        }

        public void Execute()
        {
            try
            {
                UnsafeExecute();
            }
            catch (Exception)
            {
                if (Error != null)
                    Error(this, null);
            }
        }

        private void ReceiveResponse(TwitterStatus status, TwitterResponse response)
        {
            if (status != null && response.StatusCode == HttpStatusCode.OK)
            {
                if (Completed != null)
                    Completed(this, new EventArgs());
            }
            else
            {
                if (Error != null)
                    Error(this, response);
            }
        }

        private void UnsafeExecute()
        {
            PendingCalls = 0;
            foreach (var user in Task.Accounts)
            {
#if BACKGROUND_AGENT
                ITwitterService service = new TwitterService(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret,
                    user.Key, user.Secret);
#else
                ITwitterService service = ServiceDispatcher.GetService(user);
#endif
                service.SendTweet(Task.Text, Task.InReplyTo, ReceiveResponse);
                PendingCalls++;
            }
            if (PendingCalls == 0 && Completed != null)
                Completed(this, new EventArgs());
        }

        public EventHandler Completed;
        public TwitterHandler Error;
    }
}
