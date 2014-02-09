using AncoraMVVM.Base;
using PropertyChanged;

namespace Ocell
{
    [ImplementPropertyChanged]
    public class ExtendedViewModelBase : ViewModelBase
    {
        public bool IsFull { get; set; }
        public bool IsFullFeatured { get; set; }

        public ExtendedViewModelBase()
        {
            TrialInformation.ReloadTrialInfo();
            IsFull = TrialInformation.IsFull;
            IsFullFeatured = TrialInformation.IsFullFeatured;
        }
    }
}
