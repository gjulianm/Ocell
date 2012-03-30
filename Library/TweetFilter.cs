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
using TweetSharp;

namespace Ocell.Library
{
    public enum IncludeOrExclude
    { Include, Exclude };

    public interface ITweetableFilter
    {
        string Filter { get; set; }
        IncludeOrExclude Inclusion { get; set; }
        bool Evaluate(ITweetable item);
    }

    public class UserFilter : ITweetableFilter
    {
        public string Filter { get; set; }
        public IncludeOrExclude Inclusion {get; set;}
        
        public bool Evaluate(ITweetable item)
        {
            if(Filter == null)
                Filter = "";

            if(item == null)
                return false;

            bool result = item.Author.ScreenName.Contains(Filter);

            if (Inclusion == IncludeOrExclude.Exclude)
                return !result;
            else
                return result;
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && (obj as UserFilter != null) && ((obj as UserFilter).Filter == Filter);
        }

        public override int GetHashCode()
        {
            return Filter.GetHashCode() ^ (int)Inclusion;
        }
    }

    public class SourceFilter : ITweetableFilter
    {
        public string Filter { get; set; }
        public IncludeOrExclude Inclusion { get; set; }

        public bool Evaluate(ITweetable item)
        {
            if (Filter == null)
                Filter = "";

            if (item == null)
                return false;

            bool result;

            if (item is TwitterStatus)
                result = (item as TwitterStatus).Source.Contains(Filter);
            else if (item is TwitterSearchStatus)
                result = (item as TwitterSearchStatus).Source.Contains(Filter);
            else
                result = false;

            if (Inclusion == IncludeOrExclude.Exclude)
                return !result;
            else
                return result;
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && (obj as SourceFilter != null) && ((obj as SourceFilter).Filter == Filter);
        }

        public override int GetHashCode()
        {
            return Filter.GetHashCode() ^ (int)Inclusion;
        }
    }

    public class TextFilter : ITweetableFilter
    {
        public string Filter { get; set; }
        public IncludeOrExclude Inclusion { get; set; }

        public bool Evaluate(ITweetable item)
        {
            if (Filter == null)
                Filter = "";

            if (item == null)
                return false;

            bool result = item.Text.Contains(Filter);

            if (Inclusion == IncludeOrExclude.Exclude)
                return !result;
            else
                return result;
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && (obj as TextFilter != null) && ((obj as TextFilter).Filter == Filter);
        }

        public override int GetHashCode()
        {
            return Filter.GetHashCode() ^ (int)Inclusion;
        }
    }
}
