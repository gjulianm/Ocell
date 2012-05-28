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
using DanielVaughan.ComponentModel;
using DanielVaughan;
using DanielVaughan.Windows;
using Ocell.Library;
using System.Collections.Generic;
using Ocell.Library.Twitter;
using System.Collections.ObjectModel;
using TweetSharp;

namespace Ocell.Pages
{
    public class TopicsModel : ViewModelBase
    {
        bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Assign("IsLoading", ref isLoading, value); }
        }

        object listSelection;
        public object ListSelection
        {
            get { return listSelection; }
            set { Assign("ListSelection", ref listSelection, value); }
        }

        IEnumerable<TwitterTrend> collection;
        public IEnumerable<TwitterTrend> Collection
        {
            get { return collection; }
            set { Assign("Collection", ref collection, value); }
        }

        DelegateCommand refresh;
        public ICommand Refresh
        {
            get { return refresh; }
        }

        public TopicsModel()
            : base("TrendingTopics")
        {
            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "ListSelection")
                        OnSelectionChanged();
                };

            refresh = new DelegateCommand((obj) => GetTopics());
            GetTopics();
        }

        private void GetTopics()
        {
            IsLoading = true;
            ServiceDispatcher.GetDefaultService().ListLocalTrendsFor(1, ReceiveTrends);
        }

        private void ReceiveTrends(TweetSharp.TwitterTrends Trends, TweetSharp.TwitterResponse Response)
        {
            IsLoading = false;
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                MessageService.ShowError("Error loading trending topics. Sorry :(");
                GoBack();
            }

            Collection = Trends;
        }

        private void OnSelectionChanged()
        {
            TwitterTrend trend = ListSelection as TwitterTrend;
            
            if (trend == null)
                return;

            ListSelection = null;

            string EscapedQuery = Uri.EscapeDataString(trend.Name);
            Navigate("/Pages/Search/Search.xaml?q=" + EscapedQuery);            
        }
    }
}
