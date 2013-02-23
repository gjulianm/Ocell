using System;
using Hammock;

namespace Ocell.Library.ReadLater
{
    public interface IReadLaterService
    {
        void CheckCredentials(Action<bool, ReadLaterResponse> action);
        void AddUrl(string url, Action<ReadLaterResponse> action);
    }
}
