using AncoraMVVM.Rest;
using System;
using System.Threading.Tasks;

namespace Ocell.Library.ReadLater
{
    public interface IReadLaterService
    {
        Task<HttpResponse> CheckCredentials(Action<bool, ReadLaterResponse> action);
        Task<HttpResponse> AddUrl(string url, Action<ReadLaterResponse> action);
    }
}
