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
using System.Device.Location;
using System.Linq;

namespace Ocell.Pages
{
    public class TopicsModel : ExtendedViewModelBase
    {
        GeoCoordinateWatcher geoWatcher;

        bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Assign("IsLoading", ref isLoading, value); }
        }

        string placeName;
        public string PlaceName
        {
            get { return placeName; }
            set { Assign("PlaceName", ref placeName, value); }
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

        DelegateCommand showGlobal;
        public ICommand ShowGlobal
        {
            get { return showGlobal; }
        }

        public TopicsModel()
            : base("TrendingTopics")
        {
            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "ListSelection")
                        OnSelectionChanged();
                };

            geoWatcher = new GeoCoordinateWatcher();
            if (Config.EnabledGeolocation == true)
                geoWatcher.Start();

            refresh = new DelegateCommand((obj) => GetTopics());
            showGlobal = new DelegateCommand((obj) => GetTopics(false));
            GetTopics();
        }

        private void GetTopics(bool useGeolocation = true)
        {
            IsLoading = true;
            if (Config.EnabledGeolocation == true && useGeolocation)
            {
                var location = geoWatcher.Position.Location;
                ServiceDispatcher.GetCurrentService().ListAvailableTrendsLocations(location.Latitude, location.Longitude,
                    ReceiveLocations);
            }
            else
            {
            ServiceDispatcher.GetDefaultService().ListLocalTrendsFor(1, ReceiveTrends);
                PlaceName = Localization.Resources.Global;
            }
        }

        void ReceiveLocations(IEnumerable<WhereOnEarthLocation> locations, TwitterResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK || locations == null || !locations.Any())
            {
                ServiceDispatcher.GetDefaultService().ListLocalTrendsFor(1, ReceiveTrends);
                PlaceName = Localization.Resources.Global;
            }
            else
            {
                ServiceDispatcher.GetDefaultService().ListLocalTrendsFor(locations.FirstOrDefault().WoeId, ReceiveTrends);
                PlaceName = locations.FirstOrDefault().Name;
            }
        }

        private void ReceiveTrends(TweetSharp.TwitterTrends Trends, TweetSharp.TwitterResponse Response)
        {
            IsLoading = false;
            if (Response.StatusCode != HttpStatusCode.OK)
            {
                MessageService.ShowError(Localization.Resources.ErrorLoadingTT);
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
