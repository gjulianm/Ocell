using System.Linq;
using Hammock.Silverlight.Compat;
using Ocell.Library;
using Hammock;
using System;

namespace Ocell.Pages.Settings
{
    public class BufferOAuthModel : OAuthModel
    {
        public BufferOAuthModel()
        {
            APIAuthority = "https://api.bufferapp.com";
            callbackUrl = "http://ocell.nuncaalaprimera.com";
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

        protected override void PostProcess(NameValueCollection parameters)
        {
            if (!parameters.AllKeys.Contains("access_token"))
                return;
        }
    }
}
