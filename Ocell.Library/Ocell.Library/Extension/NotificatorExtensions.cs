using AncoraMVVM.Base.Interfaces;
using TweetSharp;

namespace Ocell.Library.Extension
{
    public static class NotificatorExtensions
    {
        public static void ShowTwitterError(this INotificationService notificator, string title, TwitterError error)
        {
            string message = title;

            if (error != null && !string.IsNullOrWhiteSpace(error.Message))
                message += ": " + error.Message;

            notificator.ShowError(message);
        }
    }
}
