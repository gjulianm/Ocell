using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;

namespace Ocell.Library
{
    public static class DecisionMaker
    {
        public static int GetAvgTimeBetweenTweets(IEnumerable<TwitterStatus> tweets)
        {
            int i;
            double sum = 0;
            TimeSpan Difference;
            const int tweetsToAnalyze = 3; // Way faster.
            int upperLimit = Math.Min(tweetsToAnalyze, tweets.Count()) - 1;

            for (i = 0; i < upperLimit; i++)
            {
                Difference = tweets.ElementAt(i).CreatedDate - tweets.ElementAt(i + 1).CreatedDate;
                sum += Difference.TotalSeconds;
            }

            return ((int)sum) / tweetsToAnalyze;
        }

        public static int GetAvgTimeBetweenTweets(IEnumerable<ITweetable> Tweets)
        {
            int i;
            double sum = 0;
            TimeSpan Difference;
            const int TweetsToAnalyze = 30;
            int upperLimit = Math.Min(TweetsToAnalyze, Tweets.Count()) - 1;

            for (i = 0; i < upperLimit; i++)
            {
                Difference = Tweets.ElementAt(i).CreatedDate - Tweets.ElementAt(i + 1).CreatedDate;
                sum += Difference.TotalSeconds;
            }

            return ((int)sum) / TweetsToAnalyze;
        }
    }
}
