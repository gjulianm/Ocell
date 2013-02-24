using Microsoft.Phone.Tasks;
using Ocell.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ocell
{
    public static class TrialInformation
    {
        const int TrialDays = 7;

        public static bool IsFull
        {
            get
            {
#if OCELL_FULL
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsTrial
        {
            get;
            private set;
        }

        public static void ReloadTrialInfo()
        {
            if (IsFull)
            {
                if (Config.TrialStart == DateTime.MaxValue)
                    Config.TrialStart = DateTime.Now;

                var license = new Microsoft.Phone.Marketplace.LicenseInformation();
                IsTrial = license.IsTrial();
            }
            else
            {
                IsTrial = false;
            }
        }

        public static bool IsFullFeatured
        {
            get
            {
                return IsFull && (!IsTrial || !TrialExpired || Config.CouponCodeValidated == true);
            }
        }

        public static bool TrialExpired
        {
            get
            {
                if (Config.TrialStart == DateTime.MaxValue)
                    Config.TrialStart = DateTime.Now;

                return Config.TrialStart.Value.AddDays(TrialDays) < DateTime.Now;
            }
        }

        public static void ShowBuyDialog()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var result = MessageBox.Show(Localization.Resources.AskBuyFullVersion, "", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    var task = new MarketplaceDetailTask();
                    task.ContentType = MarketplaceContentType.Applications;
                    task.ContentIdentifier = "8644cfe4-1629-43f0-8869-4d6684a7cfcb";
                    task.Show();
                }
            });
        }

        public static string State
        {
            get
            {
                string state = "";
                ReloadTrialInfo();

                if (IsFull)
                {
                    state += "F";
                    if (IsTrial)
                        state += "T";
                    if (IsTrial && TrialExpired)
                        state += "E";
                }
                else
                {
                    state += "L";
                }

                return state;
            }
        }
    }
}
