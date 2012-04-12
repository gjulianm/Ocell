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

namespace Ocell
{
    public static class Uris
    {
        public static Uri MainPage = new Uri("/MainPage.xaml", UriKind.Relative);
        public static Uri Filters = new Uri("/Pages/Filtering/Filters.xaml", UriKind.Relative);
        public static Uri SingleFilter = new Uri("/Pages/Filtering/ManageFilter.xaml", UriKind.Relative);
        public static Uri WriteTweet = new Uri("/Pages/NewTweet.xaml", UriKind.Relative);
        public static Uri ViewDM = new Uri("/Pages/Elements/DMView.xaml", UriKind.Relative);
        public static Uri ViewTweet = new Uri("/Pages/Elements/Tweet.xaml", UriKind.Relative);
        public static Uri ViewUser = new Uri("/Pages/User.xaml", UriKind.Relative);
        public static Uri Conversation = new Uri("/Pages/Elements/Conversation.xaml", UriKind.Relative);
        public static Uri SearchForm = new Uri("/Pages/Search/EnterSearch.xaml", UriKind.Relative);
        public static Uri About = new Uri("/Pages/Settings/About.xaml", UriKind.Relative);
        public static Uri TrendingTopics = new Uri("/Pages/Topics.xaml", UriKind.Relative);
        public static Uri SelectUserForDM = new Uri("/Pages/SelectUser.xaml", UriKind.Relative);
        public static Uri Settings = new Uri("/Pages/Settings/Default.xaml", UriKind.Relative);
        public static Uri SelectUserForColumn = new Uri("/Pages/Columns/SelectAccount.xaml", UriKind.Relative);
        public static Uri LoginPage = new Uri("/Pages/Settings/OAuth.xaml", UriKind.Relative);
        public static Uri Columns = new Uri("/Pages/Columns/ManageColumns.xaml", UriKind.Relative);
        public static Uri AddColumn = new Uri("/Pages/Columns/AddColumn.xaml", UriKind.Relative);
    }
}
