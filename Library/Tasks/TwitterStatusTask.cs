using System;
using System.Collections.Generic;
using System.Net;
using TweetSharp;
using Ocell.Library.Twitter;
using System.Runtime.Serialization;
namespace Ocell.Library.Tasks
{
    public class TwitterStatusTask
    {
        public string Text { get; set; }
        public long InReplyTo { get; set; }
        public IList<UserToken> Accounts { get; set; }
        public DateTime Scheduled { get; set; }

        protected bool _errorHappened = false;
        protected int PendingCalls;

        public void Execute()
        {
            try
            {
                UnsafeExecute();
            }
            catch (Exception)
            {
            }
        }

        private void UnsafeExecute()
        {
            PendingCalls = 0;
            foreach (var user in Accounts)
            {
                ITwitterService service = ServiceDispatcher.GetService(user);
                service.SendTweet(Text, InReplyTo, ReceiveResponse);
                PendingCalls++;
            }
            if (PendingCalls == 0 && Completed != null)
                Completed(this, new EventArgs());
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
                    Error(this, new EventArgs());
            }
        }

        public event EventHandler Completed;
        public event EventHandler Error;
    }
}
