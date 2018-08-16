using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SocialAuth.Services.Kms
{
    public class EncryptedFileProvider : IFileProvider
    {
        private readonly KeyManagementServiceClient kms;
        private readonly IFileProvider innerProvider;

        public EncryptedFileProvider(
            Google.Cloud.Kms.V1.KeyManagementServiceClient kms,
            IFileProvider innerProvider)
        {
            this.kms = kms;
            this.innerProvider = innerProvider;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var innerContents = GetDirectoryContents(subpath);
            if (innerContents == null)
            {
                return null;
            }
            return new EncryptedDirectoryContents(kms, innerContents);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return EncryptedFileInfo.FromFileInfo(kms,
                innerProvider.GetFileInfo(subpath),
                innerProvider.GetFileInfo(Path.ChangeExtension(subpath, ".keyname")));
        }

        public IChangeToken Watch(string filter)
        {
            return innerProvider.Watch(filter + ".encrypted");
        }
    }

    public class EncryptedFileInfo : IFileInfo
    {
        private readonly KeyManagementServiceClient kms;
        private readonly CryptoKeyName cryptoKeyName;
        private readonly IFileInfo innerFileInfo;
        public static IFileInfo FromFileInfo(KeyManagementServiceClient kms,
            IFileInfo fileInfo, IFileInfo keynameFileInfo)
        {
            if (fileInfo == null)
            {
                return null;
            }
            if (fileInfo.IsDirectory)
            {
                return fileInfo;
            }
            if (!fileInfo.Name.EndsWith(".encrypted")) 
            {
                return null;
            }
            if (keynameFileInfo == null || !keynameFileInfo.Exists || keynameFileInfo.IsDirectory)
            {
                throw new FileNotFoundException("Encrypted file found, but "
                    + "failed to find corresponding keyname file.",
                    keynameFileInfo.Name);
            }

            using (var reader = new StreamReader(keynameFileInfo.CreateReadStream()))
            {
                string line = "";
                while (!reader.EndOfStream) 
                {
                    line = reader.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) 
                    {
                        continue; // blank or comment;
                    }
                    string[] segments = line.Split('/');
                    if (segments.Length == 4) 
                    {
                        return new EncryptedFileInfo(kms, 
                            new CryptoKeyName(segments[0], segments[1], 
                            segments[2], segments[3]),
                            fileInfo);
                    }
                    break;
                }
                throw new Exception(
                    $"Incorrectly formatted keyname file {keynameFileInfo.Name}.\n" +
                    "Expected projectId/locationId/keyringId/keyId\n" +
                    $"Instead, found {line}.");                        
            }
        }

        private EncryptedFileInfo(KeyManagementServiceClient kms,
            CryptoKeyName kryptoKeyName, IFileInfo innerFileInfo)
        {
            this.kms = kms;
            this.cryptoKeyName = kryptoKeyName;
            this.innerFileInfo = innerFileInfo;
        }

        public bool Exists => innerFileInfo.Exists && innerFileInfo.Name.EndsWith(".encrypted");

        public long Length => CreateReadStream().Length;

        public string PhysicalPath => null;

        public string Name => innerFileInfo.Name;

        public DateTimeOffset LastModified => innerFileInfo.LastModified;

        public bool IsDirectory => innerFileInfo.IsDirectory;

        public Stream CreateReadStream()
        {
            if (!Exists)
            {
                throw new FileNotFoundException(innerFileInfo.Name);
            }

            DecryptResponse response;
            using (var stream = innerFileInfo.CreateReadStream())
            {
                response = kms.Decrypt(cryptoKeyName,
                    ByteString.FromStream(stream));
            }
            MemoryStream memStream = new MemoryStream();
            response.Plaintext.WriteTo(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
    }

    public class EncryptedDirectoryContents : IDirectoryContents
    {
        private readonly KeyManagementServiceClient kms;
        private readonly CryptoKeyName cryptoKeyName;
        private readonly IDirectoryContents innerDirectoryContents;
        public EncryptedDirectoryContents(Google.Cloud.Kms.V1.KeyManagementServiceClient kms,
            CryptoKeyName cryptoKeyName, IDirectoryContents innerDirectoryContents)
        {
            this.kms = kms;
            this.innerDirectoryContents = innerDirectoryContents;
            this.cryptoKeyName = cryptoKeyName;
        }

        public bool Exists => innerDirectoryContents.Exists;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            foreach (var fileInfo in innerDirectoryContents)
            {
                if (fileInfo.IsDirectory || fileInfo.Name.EndsWith(".encrypted"))
                {
                    yield return new EncryptedFileInfo(kms, cryptoKeyName, fileInfo);
                } 
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<IFileInfo> x = this.GetEnumerator();
            return x;
        }
    }
}
