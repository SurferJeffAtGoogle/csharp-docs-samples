using System.Text;
using System.Xml.Linq;
using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.Options;

namespace WebApp {
    class KmsXmlEncryptorOptions 
    {
        public string ProjectId { get; set; }
        public string LocationId { get; set; }
        public string KeyringId { get; set; }
        public string KeyId { get; set; }
    }

    class KmsXmlEncryptor : IXmlEncryptor
    {
        private readonly KeyManagementServiceClient _kms;
        private readonly IOptions<KmsXmlEncryptorOptions> _options;

        public KmsXmlEncryptor(IOptions<KmsXmlEncryptorOptions> options)
        {
            this._options = options;
            _kms = KeyManagementServiceClient.Create();
        }

        public EncryptedXmlInfo Encrypt(XElement plaintextElement)
        {
            var opts =_options.Value;
            var keyPath = new CryptoKeyPathName(opts.ProjectId, 
                opts.LocationId, opts.KeyringId, opts.KeyId);
            var response = _kms.Encrypt(keyPath,
                ByteString.CopyFromUtf8(plaintextElement.ToString()));
            var encryptedElement = new XElement("encrypted", 
                    new XElement("payload", response.Ciphertext.ToBase64()),
                    new XElement("keypath", keyPath.ToString()));
            return new EncryptedXmlInfo(encryptedElement, 
                typeof(KmsXmlDecryptor));
        }
    }

    class KmsXmlDecryptor : IXmlDecryptor
    {
        private readonly KeyManagementServiceClient _kms
            = KeyManagementServiceClient.Create();

        public XElement Decrypt(XElement encryptedElement)
        {
            var keyPath = CryptoKeyPathName.Parse(
                encryptedElement.Element("keypath").Value);
            var keyName = new CryptoKeyName(keyPath.ProjectId, 
                keyPath.LocationId, keyPath.KeyRingId, keyPath.CryptoKeyPathId);
            var payload = ByteString.FromBase64(
                encryptedElement.Element("payload").Value);
            var response = _kms.Decrypt(keyName, payload);
            return XElement.Parse(response.Plaintext.ToStringUtf8());
        }
    }
}