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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO.IsolatedStorage;

namespace Ocell.Library
{
	public struct NotificationPreferences
	{
		public NotificationType MentionsPreferences {get; set;}
		public NotificationType MessagesPreferences { get; set; }
	}

    public enum NotificationType { Tile = 1, Toast = 3, TileAndToast = 2, None = 0 } ;
}