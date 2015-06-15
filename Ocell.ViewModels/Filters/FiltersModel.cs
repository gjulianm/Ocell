using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Library.Twitter;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TweetSharp;
namespace Ocell.ViewModels
{
    [ImplementPropertyChanged]
    public class FiltersModel : ExtendedViewModelBase
    {
        public SafeObservable<ElementFilter<ITweetable>> Filters { get; set; }
        public DelegateCommand RemoveFilter { get; set; }
        public object SelectedFilter { get; set; }
        public DelegateCommand AddFilter { get; set; }

        private TwitterResource resourceToFilter;
        private ObservableCollection<ElementFilter<ITweetable>> originalFilterCollection;
        private ObservableCollectionReplayer replayer = new ObservableCollectionReplayer();

        public FiltersModel()
        {
            resourceToFilter = ReceiveMessage<TwitterResource>();

            if (resourceToFilter != null)
                originalFilterCollection = Config.Filters.Value.GetOrCreate(resourceToFilter);
            else
                originalFilterCollection = Config.GlobalFilter.Value;

            Filters = new SafeObservable<ElementFilter<ITweetable>>(originalFilterCollection);

            replayer.ReplayTo(Filters, originalFilterCollection);

            RemoveFilter = new DelegateCommand((param) =>
            {
                var filter = param as ElementFilter<ITweetable>;
                if (filter != null)
                    Filters.Remove(filter);
            });

            AddFilter = new DelegateCommand(() =>
            {
                Navigator.MessageAndNavigate<SingleFilterModel, ElementFilter<ITweetable>>(null);
            });

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "SelectedFilter")
                    NavigateToFilterChange();
            };
        }

        private void NavigateToFilterChange()
        {
            if (SelectedFilter != null && SelectedFilter is ElementFilter<ITweetable>)
                Navigator.MessageAndNavigate<SingleFilterModel, ElementFilter<ITweetable>>(SelectedFilter as ElementFilter<ITweetable>);
        }

        public override void OnNavigating(System.ComponentModel.CancelEventArgs e)
        {
            Config.SaveGlobalFilter();
            Config.SaveFilters();

            base.OnNavigating(e);
        }

        public override void OnNavigate()
        {
            var changedFilter = ReceiveMessage<ElementFilter<ITweetable>>();

            if (changedFilter != null)
            {
                if (SelectedFilter != null && SelectedFilter is ElementFilter<ITweetable>)
                    Filters.Remove(SelectedFilter as ElementFilter<ITweetable>);

                Filters.Add(changedFilter);
            }
            SelectedFilter = null;
        }

    }
}
