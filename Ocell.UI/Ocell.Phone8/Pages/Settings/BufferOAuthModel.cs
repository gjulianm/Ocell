using System.Linq;
using Hammock.Silverlight.Compat;
using Ocell.Library;
using Hammock;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BufferAPI;
using System.Collections.Generic;

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

        protected override bool VerifyCallbackParams(NameValueCollection parameters)
        {
            return parameters.AllKeys.Contains("code");
        }

        protected override RestRequest CreateTokensRequest(NameValueCollection parameters)
        {
            var request = new RestRequest
            {
                Path = "/1/oauth2/token.json",
                Method = Hammock.Web.WebMethod.Post,
                 
            };

            request.AddParameter("client_id", SensitiveData.BufferClientId);
            request.AddParameter("client_secret", SensitiveData.BufferClientSecret);
            request.AddParameter("redirect_uri", callbackUrl);
            request.AddParameter("code", parameters["code"]);
            request.AddParameter("grant_type", "authorization_code");

            return request;
        }

        protected override void PostProcess(string contents)
        {
            var response = JObject.Parse(contents);

            if (response["access_token"] != null)
            {
                string accessToken = response["access_token"].ToString().Replace("\"", "");

                Config.BufferAccessToken = accessToken;

                var service = new BufferService(accessToken);
                service.GetProfiles(ReceiveProfiles);
            }
            else
            {
                MessageService.ShowError(Localization.Resources.ErrorBufferProfiles);
            }
        }

        void ReceiveProfiles(IEnumerable<BufferProfile> profiles, BufferResponse response)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK || profiles == null)
            {
                MessageService.ShowError(Localization.Resources.ErrorBufferProfiles);
                GoBack();
                return;
            }

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
