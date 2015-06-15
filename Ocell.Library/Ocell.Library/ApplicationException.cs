using System;
using TweetSharp;

namespace Ocell.Library
{
    public class ApplicationException : Exception
    {
        public virtual TwitterError TwitterError { get; set; }

        public ApplicationException(string message)
            : base(message)
        {
        }

        public ApplicationException(string message, TwitterError error)
            : base(message)
        {
            TwitterError = error;
        }

        public override string Message
        {
            get
            {
                if (TwitterError != null)
                    return String.Format("{0}: TwitterError - {1}", base.Message, TwitterError.Message);
                else
                    return base.Message;
            }
        }
    }
}
