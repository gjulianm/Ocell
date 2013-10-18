using System;
using System.Net;
using TweetSharp;
using Ocell.Library.Twitter;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Ocell.Library.Tasks
{
    public delegate void TwitterHandler(object sender, TwitterResponse response);

    public class TaskExecutor
    {
        public TwitterStatusTask Task { get; set; }

        public TaskExecutor(TwitterStatusTask task)
        {
            Task = task;
        }

        public async Task Execute()
        {
            try
            {
                await UnsafeExecute();
            }
            catch (Exception ex)
            {
                if (Error != null)
                    Error(this, null);

                Debug.WriteLine("Error executing task: {0}", ex);
            }
        }

        private async Task UnsafeExecute()
        {
            foreach (var user in Task.Accounts)
            {
                ITwitterService service = new TwitterService(SensitiveData.ConsumerToken, SensitiveData.ConsumerSecret,
                    user.Key, user.Secret);

                var result = await service.SendTweetAsync(new SendTweetOptions { Status = Task.Text, InReplyToStatusId = Task.InReplyTo });

                if(!result.RequestSucceeded)
                {
                    if (Error != null)
                        Error(this, result);
                }
            }
            if (Completed != null)
                Completed(this, new EventArgs());
        }

        public EventHandler Completed;
        public TwitterHandler Error;
    }
}
