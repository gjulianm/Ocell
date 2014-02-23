using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using Ocell.Controls;
using Ocell.Library;
using Ocell.Library.Twitter;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace Ocell.Pages
{
    [ImplementPropertyChanged]
    public class ColumnModel : ExtendedViewModelBase
    {
        public TweetLoader Loader { get; set; }
        public TwitterResource Resource { get; set; }
        public string Title { get; set; }
        public ExtendedListBox Listbox { get; set; }
        public ITweetable SelectedItem { get; set; }

        public event EventHandler RequestRecoverPositionPopup;
        private bool goTopOnNextLoad;
        private bool isListLoading;

        public ColumnModel(TwitterResource resource)
        {
            Resource = resource;
            Loader = new TweetLoader(resource);
            Title = Resource.Title;
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Listbox" && Listbox != null)
                    BindToListbox();
                else if (e.PropertyName == "Loader" && Loader != null)
                    BindToLoader();
            };

            BindToLoader();
        }

        private void BindToListbox()
        {
        }

        private void RaiseRequestRecoverPositionPopup()
        {
            if (RequestRecoverPositionPopup != null)
                RequestRecoverPositionPopup(this, null);
        }

        private void BindToLoader()
        {
            Loader.TweetsToLoadPerRequest = (int)Config.TweetsPerRequest.Value;
            Loader.LoadRetweetsAsMentions = (bool)Config.RetweetAsMentions.Value;
            Loader.ActivateLoadMoreButton = true;

            Loader.CacheLoad += Loader_CacheLoad;
            Loader.LoadFinished += Loader_LoadFinished;
            
            Loader.PropertyChanged += (sender1, e1) =>
            {
                if (e1.PropertyName == "IsLoading" && Loader.IsLoading != isListLoading)
                {
                    isListLoading = Loader.IsLoading;
                    Dependency.Resolve<IProgressIndicator>().IsLoading = Loader.IsLoading;
                }
            };
        }

        private void Loader_LoadFinished(object sender, EventArgs e)
        {
            if (goTopOnNextLoad)
            {
                goTopOnNextLoad = false;
                ScrollToTop();
            }
        }

        private void Loader_CacheLoad(object sender, EventArgs e)
        {
            if (Config.ReloadOptions.Value == ColumnReloadOptions.AskPosition)
                RaiseRequestRecoverPositionPopup();
            else if (Config.ReloadOptions.Value == ColumnReloadOptions.KeepPosition)
                RecoverPosition();
            else
                goTopOnNextLoad = true;
        }

        public void ScrollToTop()
        {
            if (Listbox != null)
                Listbox.ScrollToTop();
        }

        public void Reload()
        {
            Task.Factory.StartNew(Listbox.AutoReload);
        }

        public bool CanRecoverPosition()
        {
            return Config.ReloadOptions.Value == ColumnReloadOptions.AskPosition && Listbox != null && Listbox.CanRecoverPosition();
        }

        public bool RecoverPosition()
        {
            if (Listbox != null && Config.ReloadOptions.Value == ColumnReloadOptions.AskPosition && Listbox.CanRecoverPosition())
            {
                Listbox.ResumeReading();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AutoLoad()
        {
            if(Listbox != null)
                Listbox.AutoReload();
        }

        public void InitialLoad()
        {
            Loader.LoadCache();
            AutoLoad();
        }
    }
}
