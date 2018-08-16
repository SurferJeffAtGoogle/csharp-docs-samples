using Google.Cloud.Kms.V1;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SocialAuth.Services.Kms
{
    public class EncryptedFileInfo : IFileInfo
    {
        private readonly KeyManagementServiceClient kms;
        private readonly string path;

        public EncryptedFileInfo(KeyManagementServiceClient kms, string path)
        {
            this.kms = kms;
            this.path = path;
        }

        public bool Exists => IsDirectory || (path.Contains(".encrypted") && File.Exists(path));

        public long Length => CreateReadStream().Length;

        public string PhysicalPath => null;

        public string Name => Path.GetFileName(path);

        public DateTimeOffset LastModified => File.GetLastWriteTime(path);

        public bool IsDirectory => Directory.Exists(path);

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }
    }

    public class EncryptedDirectoryContents : IDirectoryContents
    {
        public bool Exists => throw new System.NotImplementedException();

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }

    public class EncryptedFileProvider : IFileProvider
    {
        private readonly KeyManagementServiceClient kms;
        private readonly string rootPath;

        public EncryptedFileProvider(Google.Cloud.Kms.V1.KeyManagementServiceClient kms,
            string rootPath)
        {
            this.kms = kms;
            this.rootPath = rootPath;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new System.NotImplementedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            throw new System.NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new System.NotImplementedException();
        }
    }
}
