using System;
using System.Collections.Generic;
using Ocell.Library.Twitter;
using TweetSharp;
using System.Linq;

namespace Ocell.Library.Filtering
{
    public class ColumnFilter 
    {
        private List<ITweetableFilter> _predicates;
        public TwitterResource Resource { get; set; }
        public ColumnFilter Global { get; set; }
        public List<ITweetableFilter> Predicates
        {
            get
            {
                return _predicates;
            }
        }

        public ColumnFilter()
        {
            _predicates = new List<ITweetableFilter>();
            Global = null;
        }

        public bool Evaluate(object item)
        {
            ITweetable tweet = item as ITweetable;
            
            if(item == null)
                return false;

            foreach(var filter in _predicates)
                if(filter.Evaluate(tweet) == false)
                    return false;

            if (Global != null)
                if (Global.Evaluate(item) == false)
                    return false;

            return true;
        }

        public void CleanOldFilters()
        {
            foreach(var item in _predicates.Where(item=> item.IsValidUntil < DateTime.Now))
                _predicates.Remove(item);
        }

        public Predicate<object> getPredicate()
        {
             return new Predicate<object>(Evaluate);
        }

        public void AddFilter(ITweetableFilter predicate)
        {
            if(!_predicates.Contains(predicate))
                _predicates.Add(predicate);
        }

        public void RemoveFilter(ITweetableFilter predicate)
        {
            try
            {
                _predicates.Remove(predicate);
            }
            catch
            {
            }
        }
    }
}
