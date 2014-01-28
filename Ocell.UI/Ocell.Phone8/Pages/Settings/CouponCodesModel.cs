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
            : base("Backgrounds")
        {
            validate = new DelegateCommand(ValidateCode, x => !IsLoading);
            Code = "";
        }

        async void ValidateCode(object param)
        {
            if (String.IsNullOrWhiteSpace(Code))
            {
                MessageService.ShowError(Resources.CodeInvalid);
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
                MessageService.ShowError(Resources.CodeInvalid);
            }
            else
            {
                Config.CouponCodeValidated = true;
                MessageService.ShowMessage(Resources.CodeValid);
                GoBack();
            }
        }
    }
}
