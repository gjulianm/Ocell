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
        private static int GetAvgTimeBetweenTweets(ref IEnumerable<TwitterStatus> Tweets)
        {
            int i;
            double sum = 0;
            TimeSpan Difference;
            const int TweetsToAnalyze = 30;
            int upperLimit = Math.Min(TweetsToAnalyze, Tweets.Count()) - 1;

            for (i = 0; i < upperLimit; i++)
            {
                Difference = Tweets.ElementAt(i).CreatedDate - Tweets.ElementAt(i+1).CreatedDate;
                sum += Difference.TotalSeconds;
            }

            return ((int)sum) / TweetsToAnalyze;
        }

        public static bool ShouldLoadCache(ref IEnumerable<TwitterStatus> List)
        {
            int AverageTimeBetweenTweets;
            TimeSpan CurrentDifference;
            int MaxTimesDifference = 3;

            if(List == null || List.Count() == 0)
                return false;

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return true;

            AverageTimeBetweenTweets = GetAvgTimeBetweenTweets(ref List);
            CurrentDifference = DateTime.Now.ToUniversalTime() - List.First().CreatedDate;

            return (int)CurrentDifference.TotalSeconds < MaxTimesDifference * AverageTimeBetweenTweets;
 
        }
    }
}
