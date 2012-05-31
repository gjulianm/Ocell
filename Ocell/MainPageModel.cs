using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using DanielVaughan.ComponentModel;
using DanielVaughan.Windows;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Threading;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using System.Linq;

namespace Ocell.Pages
{
    public class ReloadArgs : EventArgs
    {
        public TwitterResource Resource { get; set; }
        public bool ReloadAll { get; set; }

        public ReloadArgs(TwitterResource resource, bool reloadAll = false)
        {
            Resource = resource;
            ReloadAll = reloadAll;
        }
    }

    public delegate void ReloadEventHandler(object sender, ReloadArgs e);

    public class AddColumnModel : ViewModelBase
    {
        DateTime lastAutoReload;
        const int secondsBetweenReloads = 25;

        public event ReloadEventHandler ReloadLists;
        private void RaiseReload(TwitterResource resource)
        {
            var temp = ReloadLists;
            if (temp != null)
                temp(this, new ReloadArgs(resource, false));
        }

        private void RaiseReloadAll()
        {
            var temp = ReloadLists;
            if (temp != null)
                temp(this, new ReloadArgs(Config.Columns.FirstOrDefault(), true));
        }

        bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Assign("IsLoading", ref isLoading, value); }
        }

        SafeObservable<TwitterResource> pivots;
        public SafeObservable<TwitterResource> Pivots
        {
            get { return pivots; }
            set { Assign("Pivots", ref pivots, value); }
        }

        object selectedPivot;
        public object SelectedPivot
        {
            get { return selectedPivot; }
            set { Assign("SelectedPivot", ref selectedPivot, value); }
        }

        string currentAccountName;
        public string CurrentAccountName
        {
            get { return currentAccountName; }
            set { Assign("CurrentAccountName", ref currentAccountName, value); }
        }

        public AddColumnModel()
            : base("AddColumn")
        {
            if (Config.RetweetAsMentions == null)
                Config.RetweetAsMentions = true;
            if (Config.TweetsPerRequest == null)
                Config.TweetsPerRequest = 40;
            if (Config.DefaultMuteTime == null || Config.DefaultMuteTime == TimeSpan.FromHours(0))
                Config.DefaultMuteTime = TimeSpan.FromHours(8);

            lastAutoReload = DateTime.MinValue;
            Pivots = new SafeObservable<TwitterResource>();

            foreach (var pivot in Config.Columns)
                Pivots.Add(pivot);

            Config.Columns.CollectionChanged += (sender, e) =>
            {
                foreach (var item in e.NewItems)
                {
                    if ((item is TwitterResource) && !Pivots.Contains((TwitterResource)item))
                        Pivots.Add((TwitterResource)item);
                }

                foreach (var item in e.OldItems)
                {
                    if ((item is TwitterResource) && Pivots.Contains((TwitterResource)item))
                        Pivots.Remove((TwitterResource)item);
                }
            };

            this.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "SelectedPivot")
                        UpdatePivot();
                };

            this.NavigatedTo += (sender, e) => ThreadPool.QueueUserWorkItem((context) => RaiseReloadAll());

            string column;
            if (QueryParameters.TryGetValue("column", out column))
            {
                column = Uri.UnescapeDataString(column);
                if(Config.Columns.Any(item => item.String == column))
                    SelectedPivot = Config.Columns.First(item => item.String == column);
            }
        }

        void UpdatePivot()
        {
            if (SelectedPivot is TwitterResource)
            {
                var resource = (TwitterResource)SelectedPivot;
                CurrentAccountName = resource.User.ScreenName;
                if (DateTime.Now > lastAutoReload.AddSeconds(secondsBetweenReloads))
                {
                    lastAutoReload = DateTime.Now;
                    ThreadPool.QueueUserWorkItem((context) => RaiseReload(resource));
                }
                DataTransfer.CurrentAccount = resource.User;
            }
        }
    }
}
