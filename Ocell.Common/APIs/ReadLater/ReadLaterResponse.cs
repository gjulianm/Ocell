using Hammock;

namespace Ocell.Library.ReadLater
{
    public class ReadLaterResponse
    {
        public RestResponse ResponseBase { get; set; }
        public ReadLaterResult Result {get; set;}
    }

    public enum ReadLaterResult
    {
        Accepted, RateLimitExceeded, AuthenticationFail, InternalError, Unknown, BadRequest
    }
}
