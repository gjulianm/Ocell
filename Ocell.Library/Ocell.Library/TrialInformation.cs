using AncoraMVVM.Base.Interfaces;
using AncoraMVVM.Base.IoC;
using AncoraMVVM.Base.Tasks;
using Ocell.Library;
using System;

namespace Ocell
{
    public static class TrialInformation
    {
        const int TrialDays = 7;

        public static Func<bool> RequestTrialInfo { get; set; }

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
                if (Config.TrialStart.Value == DateTime.MaxValue)
                    Config.TrialStart.Value = DateTime.Now;

                IsTrial = RequestTrialInfo();
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
                return IsFull && (!IsTrial || !TrialExpired || Config.CouponCodeValidated.Value == true);
            }
        }

        public static bool TrialExpired
        {
            get
            {
                if (Config.TrialStart.Value == DateTime.MaxValue)
                    Config.TrialStart.Value = DateTime.Now;

                return Config.TrialStart.Value.Value.AddDays(TrialDays) < DateTime.Now;
            }
        }

        public static void ShowBuyDialog()
        {
            var notificator = Dependency.Resolve<INotificationService>();
            var accepts = notificator.Prompt(Localization.Resources.AskBuyFullVersion);
            if (accepts)
            {
                Dependency.Resolve<ITaskFactory>().CreateAppStoreTask(AppStoreContentType.Applications, "8644cfe4-1629-43f0-8869-4d6684a7cfcb");
            }

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
