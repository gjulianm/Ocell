using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSharp;
using System.Collections.ObjectModel;

namespace Ocell.Library
{
    public class ColumnFilter
    {
        private Collection<ITweetableFilter> _predicates;
        public TwitterResource Resource { get; set; }

        public ColumnFilter()
        {
            _predicates = new Collection<ITweetableFilter>();
        }

        public bool Evaluate(object item)
        {
            ITweetable tweet = item as ITweetable;
            
            if(item == null)
                return false;

            foreach(var filter in _predicates)
                if(filter.Evaluate(tweet) == false)
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return Resource.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Resource.GetHashCode();
        }

        public Predicate<object> Predicate
        {
            get
            {
                return new Predicate<object>(Evaluate);
            }
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
