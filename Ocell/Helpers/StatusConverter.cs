using TweetSharp;
namespace Ocell
{
    public static class StatusConverter
    {
        public static TwitterStatus SearchToStatus(TwitterSearchStatus search)
        {
            TwitterStatus status = new TwitterStatus
            {
                User = search.Author as TwitterUser,
                Text = search.Text,
                TextAsHtml = search.TextAsHtml,
                Source = search.Source,
                Id = search.Id,
                CreatedDate = search.CreatedDate,
            };
            return status;
        }
    }
}
