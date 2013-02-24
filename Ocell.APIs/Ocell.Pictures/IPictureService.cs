using Hammock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ocell.Pictures
{
    public interface IPictureService
    {
        void SendPicture(string text, string fileName, Stream file, Action<RestResponse, string> callback);
    }
}
