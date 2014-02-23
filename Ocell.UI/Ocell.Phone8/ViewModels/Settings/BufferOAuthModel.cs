using AncoraMVVM.Rest;
using BufferAPI;
using Ocell.Library;
using System.Linq;
using System.Net.Http;

namespace Ocell.Pages.Settings
{
    public class BufferOAuthModel : OAuthModel
    {
        public BufferOAuthModel()
        {
            APIAuthority = "https://api.bufferapp.com";
            callbackUrl = "http://ocell.nuncaalaprimera.com";
            AuthAutority = "https://bufferapp.com";
            Version = OAuthVersion.OAuthV2;
        }

        protected override string GetAuthorizationUrl()
        {
            return string.Format("https://bufferapp.com/oauth2/authorize?client_id={0}&redirect_uri={1}&response_type=code", SensitiveData.BufferClientId, callbackUrl);
        }

        protected override bool VerifyCallbackParams(ParameterCollection parameters)
        {
            return parameters.ContainsKey("code");
        }

        protected override HttpRequestMessage CreateTokensRequest(ParameterCollection parameters)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/1/oauth2/token.json");

            var pars = new ParameterCollection(new object[] {
                "client_id", SensitiveData.BufferClientId,
                "client_secret", SensitiveData.BufferClientSecret,
                "redirect_uri", callbackUrl,
                "code", parameters["code"],
                "grant_type", "authorization_code"});

            request.Content = new StringContent(pars.BuildPostContent());

            return request;
        }

        protected override void PostProcess(ParameterCollection parameters)
        {
            if (parameters["access_token"] != null)
            {
                Config.BufferAccessToken.Value = parameters["access_token"].ToString().Replace("\"", "");
                GetBufferProfiles();
            }
            else
            {
                Notificator.ShowError(Localization.Resources.ErrorBufferProfiles);
            }
        }

        private async void GetBufferProfiles()
        {
            var service = new BufferService(Config.BufferAccessToken.Value);
            var response = await service.GetProfiles();

            if (!response.Succeeded)
            {
                Notificator.ShowError(Localization.Resources.ErrorBufferProfiles);
                Navigator.GoBack();
                return;
            }

            var profiles = response.Content;

            bool added = false;

            foreach (var profile in profiles.Where(x => x.Service.ToLowerInvariant() == "twitter"))
            {
                if (Config.Accounts.Value.Any(x => x.ScreenName == profile.ServiceUsername))
                {
                    added = true;
                    Config.BufferProfiles.Value.Add(profile);
                }
            }

            Config.SaveBufferProfiles();

            if (!added)
            {
                Notificator.ShowWarning(Localization.Resources.NoBufferProfilesAdded);
            }

            Navigator.GoBack();
        }
    }
}