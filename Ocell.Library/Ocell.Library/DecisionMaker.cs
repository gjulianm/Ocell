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
using TweetSharp;
using System.Linq;

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
                Difference = tweets.ElementAt(i).CreatedDate - tweets.ElementAt(i+1).CreatedDate;
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
