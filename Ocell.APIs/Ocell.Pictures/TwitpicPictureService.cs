using Hammock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Ocell.Pictures
{
    public class TwitpicPictureService : IPictureService
    {
        string consumerKey;
        string consumerKeySecret;
        string userKey;
        string userKeySecret;
        RestRequest echoRequest;

        public TwitpicPictureService(string consumerToken, string consumerSecret, string token, string secret)
        {
            consumerKey = consumerToken;
            consumerKeySecret = consumerSecret;
            userKey = token;
            userKeySecret = secret;
        }

        public void SetEchoAuthRequest(RestRequest request)
        {
            echoRequest = request;
        }

        public void SendPicture(string text, string fileName, Stream file, Action<RestResponse, string> callback)
        {
            if (echoRequest == null)
                echoRequest = new RestRequest();


            RestClient client = new RestClient { Authority = "http://api.twitpic.com/", VersionPath = "1" };

            echoRequest.AddFile("media", fileName, file);
            echoRequest.AddField("key", "1abb1622666934158f4c2047f0822d0a");
            echoRequest.AddField("message", text);
            echoRequest.AddField("consumer_token", consumerKey);
            echoRequest.AddField("consumer_secret", consumerKeySecret);
            echoRequest.AddField("oauth_token", userKey);
            echoRequest.AddField("oauth_secret", userKeySecret);
            echoRequest.Path = "upload.xml";
            //req.Method = Hammock.Web.WebMethod.Post;

            client.BeginRequest(echoRequest, (RestCallback)uploadCompleted, callback);
        }

        void uploadCompleted(RestRequest request, RestResponse response, object userstate)
        {
            Action<RestResponse, string> callback = (Action<RestResponse, string>)userstate;
            string url = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                XDocument doc = XDocument.Parse(response.Content);
                XElement node = doc.Descendants("url").FirstOrDefault();
                url = node.Value;
            }

            if (callback != null)
                callback(response, url);
        }
    }
}
