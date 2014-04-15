using AncoraMVVM.Base;
using AncoraMVVM.Base.Interfaces;
using Ocell.Library;
using Ocell.Library.Twitter;
using Ocell.Localization;
using Ocell.Pages.Search;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Linq;
using System.Windows.Input;
using TweetSharp;


namespace Ocell.Pages
{
    [ImplementPropertyChanged]
    public class TopicsModel : ExtendedViewModelBase
    {
        GeoCoordinateWatcher geoWatcher;
        public string PlaceName { get; set; }
        public object ListSelection { get; set; }
        public IEnumerable<TwitterTrend> Collection { get; set; }
        public ObservableCollection<string> Locations { get; set; }
        public string SelectedLocation { get; set; }
        Dictionary<string, long> LocationMap;

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

        DelegateCommand showLocations;
        public ICommand ShowLocations
        {
            get { return showLocations; }
        }


        long currentLocation = 1;

        public TopicsModel()
        {
            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "ListSelection")
                        OnSelectionChanged();
                    if (e.PropertyName == "SelectedLocation")
                        UserChoseLocation();
                };

            geoWatcher = new GeoCoordinateWatcher();
            if (Config.EnabledGeolocation.Value == true)
                geoWatcher.Start();

            Locations = new ObservableCollection<string>();
            LocationMap = new Dictionary<string, long>();
            refresh = new DelegateCommand((obj) => GetTopics());
            showGlobal = new DelegateCommand((obj) => { currentLocation = 1; PlaceName = Localization.Resources.Global; GetTopics(); });
            showLocations = new DelegateCommand((obj) => RaiseShowLocations(), (obj) => Locations.Any());

            GetLocations();

            Progress.IsLoading = true;
            if (Config.EnabledGeolocation.Value == true && (Config.TopicPlaceId.Value == -1 || Config.TopicPlaceId.Value == null))
                GetMyLocation();
            else
            {
                currentLocation = Config.TopicPlaceId.Value.HasValue ? (long)Config.TopicPlaceId.Value : 1;
                PlaceName = Config.TopicPlace.Value;
                GetTopics();
            }
        }

        // TODO: Check API return values, for what?

        private async void GetMyLocation()
        {
            var response = await ServiceDispatcher.GetCurrentService().ListClosestTrendsLocationsAsync(new ListClosestTrendsLocationsOptions
            {
                Lat = geoWatcher.Position.Location.Latitude,
                Long = geoWatcher.Position.Location.Longitude
            });

            if (!response.RequestSucceeded)
            {
                Notificator.ShowError(Resources.CouldntGetYourPosition);
                showGlobal.Execute(null);
            }

            var locs = response.Content;

            if (response.RequestSucceeded && locs.Any())
            {
                var loc = locs.First();
                PlaceName = loc.Name;
                currentLocation = loc.WoeId;
                Config.TopicPlace.Value = PlaceName;
                Config.TopicPlaceId.Value = currentLocation;
                GetTopics();
            }
        }

        public event EventHandler ShowLocationsPicker;

        private void RaiseShowLocations()
        {
            if (ShowLocationsPicker != null)
                ShowLocationsPicker(this, new EventArgs());
        }

        private async void GetTopics()
        {
            Progress.IsLoading = true;

            var response = await ServiceDispatcher.GetCurrentService().ListLocalTrendsForAsync(new ListLocalTrendsForOptions { Id = (int)currentLocation });

            Progress.IsLoading = false;
            if (!response.RequestSucceeded)
            {
                Notificator.ShowError(Localization.Resources.ErrorLoadingTT);
                Navigator.GoBack();
                return;
            }

            Collection = response.Content;
        }

        private async void GetLocations()
        {
            var response = await ServiceDispatcher.GetCurrentService().ListAvailableTrendsLocationsAsync();

            var locs = response.Content;

            if (response.RequestSucceeded && locs.Any())
            {
                Dispatcher.InvokeIfRequired(() =>
                {
                    foreach (var loc in locs.OrderBy(x => x.Name))
                    {
                        if (!Locations.Contains(loc.Name))
                            Locations.Add(loc.Name);

                        if (!LocationMap.ContainsKey(loc.Name))
                            LocationMap.Add(loc.Name, loc.WoeId);
                    }
                    showLocations.RaiseCanExecuteChanged();
                });
            }
        }

        void UserChoseLocation()
        {
            PlaceName = SelectedLocation;
            LocationMap.TryGetValue(SelectedLocation, out currentLocation);
            Config.TopicPlace.Value = PlaceName;
            Config.TopicPlaceId.Value = currentLocation;
            GetTopics();
        }

        private void OnSelectionChanged()
        {
            TwitterTrend trend = ListSelection as TwitterTrend;

            if (trend == null)
                return;

            ListSelection = null;

            var resource = new TwitterResource
            {
                Data = trend.Name,
                Type = ResourceType.Search,
                User = DataTransfer.CurrentAccount
            };

            Navigator.MessageAndNavigate<ResourceViewModel, TwitterResource>(resource);
        }
    }
}

