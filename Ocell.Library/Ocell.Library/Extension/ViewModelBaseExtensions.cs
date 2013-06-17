using System.Windows;
using DanielVaughan.ComponentModel;
using System;
using Microsoft.Phone.Controls;

namespace Ocell
{
    public class ExtendedViewModelBase : ViewModelBase
    {
        protected bool CanGoBack
        {
            get
            {
                return ((PhoneApplicationFrame)Application.Current.RootVisual).CanGoBack;
            }
        }

        bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Assign("IsLoading", ref isLoading, value); }
        }

        string barText;
        public string BarText
        {
            get { return barText; }
            set { Assign("BarText", ref barText, value);}
        }

        bool isWP7;
        public bool IsWP7
        {
            get { return isWP7; }
            set { Assign("IsWP7", ref isWP7, value); }
        }

        bool isWP8;
        public bool IsWP8
        {
            get { return isWP8; }
            set { Assign("IsWP8", ref isWP8, value); }
        }

        public ExtendedViewModelBase()
            : base()
        {
#if WP8
            IsWP8 = true;
            IsWP7 = false;
#elif WP7
            IsWP8 = false;
            IsWP7 = true;
#endif
        }

        bool isFull;
        public bool IsFull
        {
            get { return isFull; }
            set { Assign("IsFull", ref isFull, value); }
        }

        bool isFullFeatured;
        public bool IsFullFeatured
        {
            get { return isFullFeatured; }
            set { Assign("IsFullFeatured", ref isFullFeatured, value); }
        }

        public ExtendedViewModelBase(string message)
            : base(message)
        {
            TrialInformation.ReloadTrialInfo();
            IsFull = TrialInformation.IsFull;
            IsFullFeatured = TrialInformation.IsFullFeatured;
#if WP8
            IsWP8 = true;
            IsWP7 = false;
#elif WP7
            IsWP8 = false;
            IsWP7 = true;
#endif
        }

        /* I'll go to hell for this, probably. 
         * I know that using new is not good because it breaks polymorphism. 
         * But as I'm not using polymorphism for ViewModelBase, here the benefits are more than
         *  the disadvantages, so I'll keep with it
         *  
         * I also know that using an empty catch block is not good. But as I can't find the specific
         *  type of exception that GoBack() can cause, I can't catch it and act in consequence. I would
         *  really like to control when navigation is in progress, wait for it to end and then performing the GoBack().
         *  But I can't. I will go further in this later, but for now this snippet will do the trick.
         */
        protected new void GoBack()
        {
            try
            {
                var dispatcher = Deployment.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                {
                    if (CanGoBack)
                        base.GoBack();
                }
                else
                {
                    dispatcher.BeginInvoke(() => {
                        if (CanGoBack)
                            base.GoBack();
                    });
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
