using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using DanielVaughan.ComponentModel;

namespace Ocell
{
    public class ExtendedViewModelBase : ViewModelBase
    {
        public ExtendedViewModelBase(string message)
            : base(message)
        {
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
                    base.GoBack();
                else
                    dispatcher.BeginInvoke(GoBack);
            }
            catch (Exception)
            {
            }
        }
    }
}
