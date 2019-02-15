
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Google.Cloud.Diagnostics.Common;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WebApp
{
    class TracingXmlRepository : IXmlRepository
    {
        private readonly IXmlRepository _inner;
        private readonly IManagedTracer _tracer;
        private readonly ILogger _logger;

        public TracingXmlRepository(
            IXmlRepository inner,
            IManagedTracer tracer,
            ILoggerFactory loggerFactory)
        {
            this._inner = inner;
            this._tracer = tracer;
            _logger = loggerFactory.CreateLogger<TracingXmlRepository>();
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            _logger.LogTrace("GetAllElements()");
            using (_tracer.StartSpan("GetAllElements()"))
            {
                return _inner.GetAllElements();
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            _logger.LogTrace("StoreElement({0})", friendlyName);
            using (_tracer.StartSpan($"StoreElement({friendlyName})"))
            {
                _inner.StoreElement(element, friendlyName);
            }
        }
    }

    class CloudStorageXmlRepository : IXmlRepository
    {
        private readonly IOptions<Options> _options;
        private readonly ILogger<CloudStorageXmlRepository> _logger;
        private readonly StorageClient _storage;

        public class Options 
        {
            public string BucketName { get; set; }
            public string DirName { get; set; }
        }

        public CloudStorageXmlRepository(IOptions<Options> options,
            ILogger<CloudStorageXmlRepository> logger)
        {
            this._options = options;
            this._logger = logger;
            _storage = StorageClient.Create();
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            // This isn't going to work because cloud storage is eventually
            // consistent.  Elements that were recently stored will not
            // be found.
            throw new System.NotImplementedException();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            var opts = _options.Value;
            var stream = new MemoryStream();
            element.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            _storage.UploadObject(opts.BucketName, friendlyName, "text/xml",
                stream);
        }
    }
}