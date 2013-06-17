using Ocell.Library;
using Ocell.Library.Collections;
using Ocell.Library.Twitter;
using Ocell.Library.Twitter.Comparers;
using Ocell.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using TweetSharp;
#if WP8
using Windows.Phone.Speech.Synthesis;
#endif

namespace Ocell.Pages.Elements
{
    public class NotificationsModel : ExtendedViewModelBase
    {
        private DateTime lastCheckTime;
        private int requestsPending = 0;

        public SortedFilteredObservable<ITweetable> Tweets
        {
            get;
            protected set;
        }

        public NotificationsModel()
        {
            Tweets = new SortedFilteredObservable<ITweetable>(new TweetComparer());
        }

        public void OnLoad()
        {
            lastCheckTime = DateSync.GetLastCheckDate();

            var mentionOptions = new ListTweetsMentioningMeOptions
            {
                Count = 20,
            };

            var dmOption = new ListDirectMessagesReceivedOptions
            {
                Count = 20,
            };
            
            foreach (var account in Config.Accounts)
            {
                if (account.Preferences.MentionsPreferences != Library.Notifications.NotificationType.None)
                {
                    IsLoading = true;
                    Interlocked.Increment(ref requestsPending);
                    ServiceDispatcher.GetService(account).ListTweetsMentioningMe(mentionOptions, (t, r) => FilterAndAddStatuses(t.Cast<ITweetable>(), r)); // Ugh.
                }
                if (account.Preferences.MessagesPreferences != Library.Notifications.NotificationType.None)
                {
                    IsLoading = true;
                    Interlocked.Increment(ref requestsPending);
                    ServiceDispatcher.GetService(account).ListDirectMessagesReceived(dmOption, (t, r) => FilterAndAddStatuses(t.Cast<ITweetable>(), r));
                }
            }
#if WP8
            this.LoadFinished += (s, e) => SpeakNotifications();
#endif
        }
#if WP8
        [Conditional("WP8")]
        private async void SpeakNotifications()
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            StringBuilder sb = new StringBuilder();

            if (Tweets.Count == 0)
            {
                await synth.SpeakTextAsync(Resources.NoNewNotifications);
                return;
            }

            foreach (var tweet in Tweets)
                sb.AppendLine(TweetToText(tweet));

            await synth.SpeakTextAsync(sb.ToString());
        }
#endif
        private string TweetToText(ITweetable tweet)
        {
            string who, when, text;

            if (tweet is TwitterStatus)
                who = String.Format(Resources.NewMention, tweet.AuthorName);
            else
                who = String.Format(Resources.NewMessage, tweet.AuthorName);

            when = new RelativeDateTimeConverter().Convert(tweet.CreatedDate, null, null, null) as string;

            text = tweet.CleanText;

            return String.Format("{0}, {1} : {2}", who, when, text);
        }

        public event EventHandler LoadFinished;

        private void FilterAndAddStatuses(IEnumerable<ITweetable> tweets, TwitterResponse status)
        {
            if (status.StatusCode == System.Net.HttpStatusCode.OK && tweets != null)
                Tweets.BulkAdd(tweets.Where(x => x.CreatedDate > lastCheckTime));

            if (Interlocked.Decrement(ref requestsPending) <= 0)
            {
                IsLoading = false;
                if (LoadFinished != null)
                    LoadFinished(this, new EventArgs());
            }
        }
    }
}
