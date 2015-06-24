using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AncoraMVVM.Base.Collections;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Library.Twitter.Comparers;
using Ocell.Localization;
using PropertyChanged;
using TweetSharp;
using Windows.Phone.Speech.Synthesis;
#if WP8
using Windows.Phone.Speech.Synthesis;
using AncoraMVVM.Base.Collections;
using PropertyChanged;
#endif

namespace Ocell.Pages.Elements
{
    [ImplementPropertyChanged]
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

        public override async void OnLoad()
        {
            lastCheckTime = await DateSync.GetLastCheckDate();

            var mentionOptions = new ListTweetsMentioningMeOptions
            {
                Count = 20,
            };

            var dmOption = new ListDirectMessagesReceivedOptions
            {
                Count = 20,
            };

            IEnumerable<ITweetable> statuses = null;

            foreach (var account in Config.Accounts.Value)
            {
                if (account.Preferences.MentionsPreferences != Library.Notifications.NotificationType.None)
                {
                    Progress.IsLoading = true;
                    Interlocked.Increment(ref requestsPending);
                    var result = await ServiceDispatcher.GetService(account).ListTweetsMentioningMeAsync(mentionOptions);

                    if (result.RequestSucceeded)
                        statuses = result.Content.Cast<ITweetable>();
                }
                if (account.Preferences.MessagesPreferences != Library.Notifications.NotificationType.None)
                {
                    Progress.IsLoading = true;
                    Interlocked.Increment(ref requestsPending);
                    var result = await ServiceDispatcher.GetService(account).ListDirectMessagesReceivedAsync(dmOption);

                    if (result.RequestSucceeded)
                        statuses = result.Content.Cast<ITweetable>();
                }
            }

            if (statuses != null)
                FilterAndAddStatuses(statuses);

            this.LoadFinished += (s, e) => SpeakNotifications();
        }

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

        private void FilterAndAddStatuses(IEnumerable<ITweetable> tweets)
        {
            Tweets.AddRange(tweets.Where(x => x.CreatedDate > lastCheckTime));

            if (Interlocked.Decrement(ref requestsPending) <= 0)
            {
                Progress.IsLoading = false;
                if (LoadFinished != null)
                    LoadFinished(this, new EventArgs());
            }
        }
    }
}
