using AncoraMVVM.Base.Files;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ocell.Tests.Mocks
{
    public class MockMemoryStream : MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            // Don't dispose!
            // In tests it doesn't matter and allows us to use it without relying on real files.
        }
    }

    public class MockFileManager : BaseFileManager
    {
        private static Dictionary<string, MockMemoryStream> streams = new Dictionary<string, MockMemoryStream>();

        public override void CreateFolder(string folderPath)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public override void DeleteFolder(string folderPath)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetFilesIn(string path)
        {
            throw new NotImplementedException();
        }

        public override AncoraMVVM.Base.Files.File OpenFile(string path, FilePermissions permissions, FileOpenMode mode)
        {
            MockMemoryStream stream;

            if (!streams.TryGetValue(path, out stream))
            {
                stream = new MockMemoryStream();
                streams[path] = stream;
            }

            stream.Seek(0, SeekOrigin.Begin);

            return new AncoraMVVM.Base.Files.File
            {
                CompletePath = path,
                Name = path,
                FileStream = stream,
                Permissions = permissions
            };
        }
    }
}
