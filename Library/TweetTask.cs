using System;
using System.Collections.Generic;
using System.Net;
using TweetSharp;
using Ocell.Library.Twitter;

namespace Ocell.Library.Tasks
{
    public interface ITweetableTask
    {
        IEnumerable<UserToken> Accounts { get; set; }

        void Execute();
        event EventHandler Completed;
        event EventHandler Error;
    }

    public class TwitterStatusTask : ITweetableTask
    {
        public string Text { get; set; }
        public long InReplyTo { get; set; }
        public IEnumerable<UserToken> Accounts { get; set; }

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
                TwitterService service = ServiceDispatcher.GetService(user);
                service.SendTweet(Text, InReplyTo, ReceiveResponse);
                PendingCalls++;
            }
        }

        protected void ReceiveResponse(TwitterStatus status, TwitterResponse response)
        {
            PendingCalls--;
            if (response.StatusCode == HttpStatusCode.OK && PendingCalls == 0)
            {
                if (Completed != null)
                    Completed(this, null);
            }
            else
            {
                if (Error != null)
                    Error(this, null);
            }
        }

        public event EventHandler Error;
        public event EventHandler Completed;
    }

    public class TwitterDMTask : ITweetableTask
    {
        public string Text { get; set; }
        public long DestinationId { get; set; }
        public IEnumerable<UserToken> Accounts { get; set; }

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
            foreach (UserToken user in Accounts)
            {
                TwitterService service = ServiceDispatcher.GetService(user);
                service.SendDirectMessage((int)DestinationId, Text, ReceiveResponse);
                PendingCalls++;
            }
        }

        protected void ReceiveResponse(TwitterDirectMessage status, TwitterResponse response)
        {
            PendingCalls--;
            if (response.StatusCode == HttpStatusCode.OK && PendingCalls == 0)
            {
                if (Completed != null)
                    Completed(this, null);
            }
            else
            {
                if (Error != null)
                    Error(this, null);
            }
        }

        public event EventHandler Error;
        public event EventHandler Completed;
    }
}
