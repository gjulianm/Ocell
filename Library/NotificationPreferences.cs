namespace Ocell.Library.Notifications
{
	public struct NotificationPreferences
	{
		public NotificationType MentionsPreferences {get; set;}
		public NotificationType MessagesPreferences { get; set; }
	}

    public enum NotificationType { Tile = 1, Toast = 3, TileAndToast = 2, None = 0 } ;
}