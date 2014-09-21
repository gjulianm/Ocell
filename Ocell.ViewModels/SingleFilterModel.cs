using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Library.Filtering;
using Ocell.Localization;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using TweetSharp;

namespace Ocell.ViewModels
{
    [ImplementPropertyChanged]
    public class SingleFilterModel : ExtendedViewModelBase
    {
        public ElementFilter<ITweetable> Filter { get; set; }

        public List<string> FilterTypes = new List<string> { Resources.author, Resources.hashtag, Resources.Source_LC, Resources.TweetText };
        public string SelectedFilterType { get; set; }
        public string FilterDescription
        {
            get
            {
                return String.Format(Resources.TweetsWhere, SelectedFilterType);
            }
        }
        public List<string> InclusionStrings = new List<string> { Resources.ShowOnly, Resources.Remove };
        public string SelectedInclusion { get; set; }
        public string FilterText { get; set; }
        public List<string> FilterTimes { get; set; }
        public string SelectedFilterTime { get; set; }
        public DateTime CustomDate { get; set; }
        public DateTime CustomTime { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand DiscardCommand { get; set; }

        private Dictionary<string, Type> FilterTypeStrings = new Dictionary<string, Type>
        {
            { Resources.author, typeof(UserFilter)},
            { Resources.hashtag, typeof(HashtagFilter)},
            { Resources.Source_LC, typeof(SourceFilter)},
            { Resources.TweetText, typeof(TextFilter)}
        };

        private Dictionary<string, TimeSpan> FilterTimespans = new Dictionary<string, TimeSpan>
        {
             { Resources.OneHour, TimeSpan.FromHours(1) },
             {Resources.EightHours, TimeSpan.FromHours(8) },
             {Resources.OneDay, TimeSpan.FromDays(1) },
             {Resources.OneWeek, TimeSpan.FromDays(7) },
             {Resources.Forever, TimeSpan.MaxValue },
             {Resources.CustomDate, TimeSpan.MinValue }
        };

        public TimeSpan Duration
        {
            get
            {
                TimeSpan timespan;

                if (SelectedFilterTime != Resources.CustomDate && !string.IsNullOrWhiteSpace(SelectedFilterTime) && FilterTimespans.TryGetValue(SelectedFilterTime, out timespan))
                    return timespan;
                else if (SelectedFilterTime != Resources.CustomDate)
                    return (TimeSpan)Config.DefaultMuteTime.Value;
                else
                    return CustomDateTime - DateTime.Now;
            }
        }

        public ExcludeMode ExcludeMode
        {
            get
            {
                return SelectedInclusion == Resources.Remove ? ExcludeMode.ExcludeOnMatch : ExcludeMode.ExcludeOnNoMatch;
            }
            set
            {
                if (ExcludeMode == ExcludeMode.ExcludeOnMatch)
                    SelectedInclusion = Resources.Remove;
                else
                    SelectedInclusion = Resources.ShowOnly;
            }
        }

        public Type FilterType
        {
            get
            {
                Type val;

                return FilterTypeStrings.TryGetValue(SelectedFilterType, out val) ? val : null;
            }
        }

        public DateTime CustomDateTime
        {
            get
            {
                return new DateTime(CustomDate.Year, CustomDate.Month, CustomDate.Day,
                    CustomTime.Hour, CustomTime.Minute, CustomTime.Second, DateTimeKind.Local);
            }
            set
            {
                CustomTime = value;
                CustomDate = value;
            }
        }

        public bool CustomDateEnabled
        {
            get
            {
                return SelectedFilterTime == Resources.CustomDate;
            }
        }

        public SingleFilterModel()
        {
            Filter = ReceiveMessage<ElementFilter<ITweetable>>();

            if (Filter == null)
            {
                SelectedFilterType = FilterTypes[0];
                SelectedInclusion = InclusionStrings[0];
                CustomDateTime = DateTime.Now + (TimeSpan)Config.DefaultMuteTime.Value;
            }
            else
            {
                SelectedFilterType = FilterTypeStrings.First(x => x.Value == Filter.GetType()).Key;
                ExcludeMode = Library.Filtering.ExcludeMode.ExcludeOnMatch;
                FilterText = Filter.Filter;
                SelectedFilterTime = Resources.CustomDate;
                CustomDateTime = Filter.IsValidUntil;
            }

            FilterTimes = FilterTimespans.Keys.ToList();

            SaveCommand = new DelegateCommand(Save, () => !string.IsNullOrWhiteSpace(FilterText) && FilterTypes.Contains(SelectedFilterType) && (!CustomDateEnabled || CustomDateTime > DateTime.Now));
            SaveCommand.BindCanExecuteToProperty(this, "FilterText", "SelectedFilterType", "CustomDateEnabled", "CustomDateTime");

            DiscardCommand = new DelegateCommand(Discard);
        }

        public override void OnNavigating(System.ComponentModel.CancelEventArgs e)
        {
            if (!Notificator.Prompt(Resources.AskDiscardFilter))
                e.Cancel = true;
            else
                base.OnNavigating(e);
        }

        internal ElementFilter<ITweetable> GenerateFilter()
        {
            if (FilterType == null)
                return null;

            return (ElementFilter<ITweetable>)Activator.CreateInstance(FilterType, FilterText, ExcludeMode, Duration);
        }

        private void Save()
        {
            if (Filter != null)
            {
                if (Filter.GetType() == FilterType)
                {
                    Filter.Duration = Duration;
                    Filter.Filter = FilterText;
                    Filter.Mode = ExcludeMode;

                    Messager.SendTo<FilterModel, ElementFilter<ITweetable>>(null); // FilterModel will infer that it just updated the filter, no need to do anything.
                    Navigator.GoBack();
                    return;
                }
            }

            ElementFilter<ITweetable> filter;

            try
            {
                filter = GenerateFilter();
            }
            catch (Exception)
            {
                Notificator.ShowError(Resources.CouldntLoadFilter);
                return;
            }


            Messager.SendTo<FilterModel, ElementFilter<ITweetable>>(filter);
            Navigator.GoBack();
        }

        private void Discard()
        {
            if (Notificator.Prompt(Resources.AskDiscardFilter))
                Navigator.GoBack();
        }
    }
}
