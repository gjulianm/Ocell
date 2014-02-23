using AncoraMVVM.Base;
using Ocell.Library;
using Ocell.Localization;
using PropertyChanged;
using System;
using System.Net;
using System.Windows.Input;

namespace Ocell.Pages.Settings
{
    [ImplementPropertyChanged]
    public class CouponCodesModel : ExtendedViewModelBase
    {
        public string Code { get; set; }

        DelegateCommand validate;
        public ICommand Validate
        {
            get { return validate; }
        }

        public CouponCodesModel()
        {
            validate = new DelegateCommand(ValidateCode, x => !Progress.IsLoading);
            Code = "";
        }

        async void ValidateCode(object param)
        {
            if (String.IsNullOrWhiteSpace(Code))
            {
                Notificator.ShowError(Resources.CodeInvalid);
                return;
            }

            var query = String.Format(SensitiveData.ValidateCodeFormat, Code);

            var request = (HttpWebRequest)WebRequest.Create(query);

            Progress.IsLoading = true;
            validate.RaiseCanExecuteChanged();

            HttpWebResponse response = null;
            bool failed = false;
            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch (Exception)
            {
                failed = true;
            }

            Progress.IsLoading = false;
            validate.RaiseCanExecuteChanged();
            if (failed || response.StatusCode != HttpStatusCode.OK)
            {
                Notificator.ShowError(Resources.CodeInvalid);
            }
            else
            {
                Config.CouponCodeValidated.Value = true;
                Notificator.ShowMessage(Resources.CodeValid);
                Navigator.GoBack();
            }
        }
    }
}
