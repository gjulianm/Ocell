using System;
using System.Collections.Generic;
using System.Net;
using TweetSharp;

namespace Ocell.Library
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
            TwitterService Service;
            foreach (UserToken User in Accounts)
            {
                Service = ServiceDispatcher.GetService(User);
                Service.SendTweet(Text, InReplyTo, ReceiveResponse);
                PendingCalls++;
            }
        }

        protected void ReceiveResponse(TwitterStatus Status, TwitterResponse Response)
        {
            PendingCalls--;
            if (Response.StatusCode == HttpStatusCode.OK && PendingCalls == 0)
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
        public long DestinationID { get; set; }
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
            TwitterService Service;
            foreach (UserToken User in Accounts)
            {
                Service = ServiceDispatcher.GetService(User);
                Service.SendDirectMessage((int)DestinationID, Text, ReceiveResponse);
                PendingCalls++;
            }
        }

        protected void ReceiveResponse(TwitterDirectMessage Status, TwitterResponse Response)
        {
            PendingCalls--;
            if (Response.StatusCode == HttpStatusCode.OK && PendingCalls == 0)
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
