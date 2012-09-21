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
using System.Collections.Generic;
using Ocell.Library.Twitter;
using TweetSharp;
using Newtonsoft.Json;

namespace Ocell.Library.Twitter
{
    public class StreamingClient
    {
        TwitterService service;
        UserToken user;
        public StreamingClient(UserToken account)
        {
            service = (TwitterService)ServiceDispatcher.GetService(account);
        }

        public void StartStreaming()
        {
            service.StreamUser(ReceiveArtifact);
        }

        public void StopStreaming()
        {
            service.CancelStreaming();
        }

        void ReceiveArtifact(TwitterStreamArtifact artifact, TwitterResponse response)
        {
            var contents = response.Response;

            if (IsMention(contents))
                RaiseMention(contents);
            else if (IsDM(contents))
                RaiseDM(contents);
            else if (IsTweet(contents))
                RaiseTweet(contents);
        }

        bool IsMention(string tweet)
        {
            return tweet != null && tweet.Contains("@" + user.ScreenName);
        }

        bool IsDM(string tweet)
        {
            return tweet.Contains("recipient_screen_name");
        }

        bool IsTweet(string tweet)
        {
            return tweet.Contains("user");
        }

        void RaiseMention(string text)
        {
            try
            {
                TwitterStatus status = service.Deserialize<TwitterStatus>(text);

                if (status != null && MentionEvent != null)
                    MentionEvent(this, status);
            }
            catch
            {
            }
        }

        void RaiseTweet(string text)
        {
            try
            {
                TwitterStatus status = service.Deserialize<TwitterStatus>(text);

                if (status != null && TweetEvent != null)
                    TweetEvent(this, status);
            }
            catch
            {
            }
        }

        void RaiseDM(string text)
        {
            try
            {
                TwitterDirectMessage status = service.Deserialize<TwitterDirectMessage>(text);

                if (status != null && MessageEvent != null)
                    MessageEvent(this, status);
            }
            catch
            {
            }
        }

        public event StreamEventHandler<TwitterStatus> MentionEvent;
        public event StreamEventHandler<TwitterStatus> TweetEvent;
        public event StreamEventHandler<TwitterDirectMessage> MessageEvent;
    }

    public delegate void StreamEventHandler<T>(object sender, T tweet);
}
