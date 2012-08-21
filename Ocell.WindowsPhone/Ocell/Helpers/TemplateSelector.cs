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
using Ocell.Library;
using Ocell.Library.Twitter;

namespace Ocell
{
    // Thanks to http://www.codeproject.com/Articles/92439/Silverlight-DataTemplateSelector
    public abstract class DataTemplateSelector : ContentControl
    {
        public virtual DataTemplate SelectTemplate(
            object item, DependencyObject container)
        {
            return null;
        }

        protected override void OnContentChanged(
            object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ContentTemplate = SelectTemplate(newContent, this);
        }
    }

    public class TweetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UsualTemplate
        {
            get;
            set;
        }

        public DataTemplate LoadMoreTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(
            object item, DependencyObject container)
        {
            if (item is LoadMoreTweetable)
                return LoadMoreTemplate;
            else
                return UsualTemplate;
        }
    }
}
