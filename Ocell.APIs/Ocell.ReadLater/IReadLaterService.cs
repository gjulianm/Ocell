using AncoraMVVM.Rest;
using System;
using System.Threading.Tasks;

namespace Ocell.Library.ReadLater
{
    public interface IReadLaterService
    {
        Task<HttpResponse> CheckCredentials();
        Task<HttpResponse> AddUrl(string url);
    }
}
