using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using DanielVaughan.ComponentModel;
using DanielVaughan.Windows;
using TweetSharp;
using Ocell.Library;
using Ocell.Library.Twitter;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Threading;
using DanielVaughan;
using DanielVaughan.InversionOfControl;
using DanielVaughan.Net;
using DanielVaughan.Services;
using System.Linq;

namespace Ocell.Pages
{
    public class NewTweetModel : ViewModelBase
    {
    	#region Fields
        bool isLoading;
        public bool IsLoading
        {
        	get { return isLoading; }
        	set { Assign("IsLoading", ref isLoading, value);}
        }
        
        string barText;
        public string BarText
        {
        	get { return barText; }
        	set { Assign("BarText", ref barText, value); }
        }
        
        string tweetText;
        public string TweetText
        {
        	get { return tweetText; }
        	set { Assign("TweetText", ref tweetText, value); }
        }
        
        int remainingChars;
        public int RemainingChars
        {
        	get { return remainingChars; }
        	set { Assign("RemainingChars", ref remainingChars, value); }
        }
        
        bool isScheduled;
        public bool IsScheduled
        {
        	get { return isScheduled; }
        	set { Assign("IsScheduled", ref isScheduled, value); }
        }
        
        DateTime scheduledDate;
        public DateTime ScheduledDate
        {
        	get { return scheduledDate; }
        	set { Assign("ScheduledDate", ref scheduledDate, value); }
        }
        
        DateTime scheduledTime;
        public DateTime ScheduledTime
        {
        	get { return scheduledTime; }
        	set { Assign("ScheduledTime", ref scheduledTime, value); }
        }
        
        bool sendingDM;
        public bool SendingDM
        {
        	get { return sendingDM; }
        	set { Assign("SendingDM", ref sendingDM, value); }
    	}
    	
    	// Whatever type it is. Probably SafeObservable
    	X selectedAccounts;
    	public X SelectedAccounts
    	{
    		get { return selectedAccounts; }
    		set { Assign("SelectedAccounts", ref selectedAccounts, value); }
    	}
    	#endregion
        
        #region Commands
        DelegateCommand sendTweet;
        public ICommand SendTweet
        {
        	get { return sendTweet; }
        }
        
        DelegateCommand scheduleTweet;
        public ICommand ScheduleTweet
        {
        	get { return scheduleTweet; }
        }
        
        DelegateCommand saveDraft;
        public ICommand SaveDraft
        {
        	get { return saveDraft; }
        }
        
        DelegateCommand selectImage;
        public ICommand SelectImage
        {
        	get { return selectImage; }
        }
        #endregion
        
        public NewTweetModel() : base("NewTweet")
        {
        
        }
}
