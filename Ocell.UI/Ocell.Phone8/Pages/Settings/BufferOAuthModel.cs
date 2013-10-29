using AncoraMVVM.Rest;
using BufferAPI;
using Newtonsoft.Json.Linq;
using Ocell.Library;
using System;
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

        protected override void PostProcess(string contents)
        {
            var response = JObject.Parse(contents);

            if (response["access_token"] != null)
            {
                Config.BufferAccessToken = response["access_token"].ToString().Replace("\"", "");
                GetBufferProfiles();
            }
            else
            {
                MessageService.ShowError(Localization.Resources.ErrorBufferProfiles);
            }
        }

        private async void GetBufferProfiles()
        {
            var service = new BufferService(Config.BufferAccessToken);
            var response = await service.GetProfiles();

            if (!response.Succeeded)
            {
                MessageService.ShowError(Localization.Resources.ErrorBufferProfiles);
                GoBack();
                return;
            }

            var profiles = response.Content;

            bool added = false;

            foreach (var profile in profiles.Where(x => x.Service.ToLowerInvariant() == "twitter"))
            {
                if (Config.Accounts.Any(x => x.ScreenName == profile.ServiceUsername))
                {
                    added = true;
                    Config.BufferProfiles.Add(profile);
                }
            }

            Config.SaveBufferProfiles();

            if (!added)
            {
                MessageService.ShowWarning(Localization.Resources.NoBufferProfilesAdded);
            }

            GoBack();
        }
    }
}