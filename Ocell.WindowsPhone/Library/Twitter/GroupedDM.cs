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
using System.Collections.Generic;
using System.Linq;
using DanielVaughan;
using DanielVaughan.ComponentModel;
using DanielVaughan.Linq;

namespace Ocell.Library.Twitter
{   
    public class GroupedDM : ObservableObject, ITweetable
    {
        long lastId;
        UserToken user;
        public GroupedDM(UserToken mainUser)
        {
            user = mainUser;
            Messages = new SafeObservable<TwitterDirectMessage>();
            lastId = -1;
            Messages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(MessagesChanged);
        }

        void MessagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Messages.Count == 0)
                return;

            long max;
            var onlyReceivedByUser = Messages.Where(x => x.SenderId != user.Id);
            if (onlyReceivedByUser.Any())
                max = onlyReceivedByUser.Max(x => x.Id);
            else
                max = Messages.Max(x => x.Id);

            if (lastId == -1 || max != lastId)
            {
                lastId = max;
                text = TrimLength(LastMessage.Text);
                cleantext = TrimLength(LastMessage.CleanText);

                var dispatcher = Deployment.Current.Dispatcher;

                dispatcher.InvokeIfRequired(() =>
                    {
                        OnPropertyChanged("Text");
                        OnPropertyChanged("TextAsHtml");
                        OnPropertyChanged("CreatedDate");
                        OnPropertyChanged("Entities");
                        OnPropertyChanged("RawSource");
                        OnPropertyChanged("AuthorName");
                        OnPropertyChanged("CleanText");
                        OnPropertyChanged("Author");
                        OnPropertyChanged("Author.ProfileImageUrl");
                    });
            }
        }

        public GroupedDM(IEnumerable<TwitterDirectMessage> list, UserToken mainUser)
            : this(mainUser)
        {
            Messages.BulkAdd(list);
        }

        public GroupedDM(TwitterDirectMessage element, UserToken mainUser)
            : this(mainUser)
        {
            Messages.Add(element);
        }

        TwitterDirectMessage lastmsg;
        private TwitterDirectMessage LastMessage
        {
            get
            {
                if (Messages == null || Messages.Count == 0)
                    return null;

                if (lastmsg == null || lastmsg.Id != lastId)
                    lastmsg = Messages.FirstOrDefault(x => x.Id == lastId);

                return lastmsg;
            }
        }

        public SafeObservable<TwitterDirectMessage> Messages { get; protected set; }
        public long Id
        {
            get
            {
                if (LastMessage == null)
                    return 0;

                return LastMessage.Id;
            }
        }

        string text;
        public string Text
        {
            get
            {
                if (LastMessage == null)
                    return "";

                if (string.IsNullOrWhiteSpace(text))
                    text = TrimLength(LastMessage.Text);

                return text;
            }
        }

        private string TrimLength(string origin)
        {
            const int maxLength = 100;

            if (origin == null)
                return "";

            if (origin.Length > maxLength)
            {
                int index = origin.IndexOf(' ', maxLength);

                if (index == -1)
                    return origin;
                else
                    return origin.Substring(0, index) + "...";
            }
            else
            {
                return origin;
            }
        }
        public string TextAsHtml
        {
            get
            {
                if (LastMessage == null)
                    return "";

                return LastMessage.TextAsHtml;
            }
        }
        public ITweeter Author
        {
            get
            {
                if (LastMessage == null)
                    return null;

                return LastMessage.Author;
            }
        }
        public DateTime CreatedDate
        {
            get
            {
                if (LastMessage == null)
                    return DateTime.Now;

                return LastMessage.CreatedDate;
            }
        }
        public TwitterEntities Entities
        {
            get
            {
                if (LastMessage == null)
                    return new TwitterEntities();

                return LastMessage.Entities;
            }
        }
        public bool IsRetweeted
        {
            get
            {
                return false;
            }
        }
        public string RawSource
        {
            get
            {
                if (LastMessage == null)
                    return "";

                return LastMessage.RawSource;
            }
            set
            {
            }
        }
        public string AuthorName
        {
            get
            {
                if (LastMessage == null)
                    return "";

                return LastMessage.AuthorName;
            }
        }

        string cleantext;
        public string CleanText
        {
            get
            {
                if (LastMessage == null)
                    return "";

                if (string.IsNullOrWhiteSpace(cleantext))
                    cleantext = TrimLength(LastMessage.CleanText);

                return cleantext;
            }
        }

        Pair<string, string> converserNames;
        public Pair<string, string> ConverserNames
        {
            get
            {
                if (converserNames != null)
                    return converserNames;

                var first = Messages.FirstOrDefault();

                if (first == null)
                    return new Pair<string, string>("", "");

                converserNames = new Pair<string, string>(first.SenderScreenName, first.RecipientScreenName);

                return converserNames;                
            }
        }
    }
}
