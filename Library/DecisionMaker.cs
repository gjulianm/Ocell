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
        private static int GetAvgTimeBetweenTweets(ref IEnumerable<TwitterStatus> tweets)
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

        public static bool ShouldLoadCache(ref IEnumerable<TwitterStatus> List)
        {
            /*
             * Supposing we get ~20 tweets per time, this is an acceptable value
             *  so the user does not lose too many tweets.
             */
            const int maxTimesDifference = 20;

            if(List == null || !List.Any())
                return false;

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return true;

            int averageTimeBetweenTweets = GetAvgTimeBetweenTweets(ref List);
            int currentDifference = (int)Math.Abs((DateTime.Now.ToUniversalTime() - List.First().CreatedDate).TotalSeconds);

            return currentDifference < maxTimesDifference * averageTimeBetweenTweets;
 
        }
    }
}
