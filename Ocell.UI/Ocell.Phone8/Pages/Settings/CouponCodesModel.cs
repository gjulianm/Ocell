using DanielVaughan.Windows;
using Ocell.Library;
using Ocell.Localization;
using System;
using System.Net;
using System.Windows.Input;

namespace Ocell.Pages.Settings
{
    public class CouponCodesModel : ExtendedViewModelBase
    {
        string code;
        public string Code
        {
            get { return code; }
            set { Assign("Code", ref code, value); }
        }

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

            IsLoading = true;
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

            IsLoading = false;
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
